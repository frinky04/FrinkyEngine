using System.Diagnostics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Scripting;
using FrinkyEngine.Core.Serialization;
using FrinkyEngine.Editor.Panels;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public enum EditorMode
{
    Edit,
    Play
}

public class EditorApplication
{
    public static EditorApplication Instance { get; private set; } = null!;

    public Core.Scene.Scene? CurrentScene { get; set; }
    private readonly List<Entity> _selectedEntities = new();
    public IReadOnlyList<Entity> SelectedEntities => _selectedEntities;
    public Entity? SelectedEntity
    {
        get => _selectedEntities.Count > 0 ? _selectedEntities[^1] : null;
        set => SetSingleSelection(value);
    }
    public EditorMode Mode { get; private set; } = EditorMode.Edit;
    public EditorCamera EditorCamera { get; } = new();
    public SceneRenderer SceneRenderer { get; } = new();
    public GizmoSystem GizmoSystem { get; } = new();
    public PickingSystem PickingSystem { get; } = new();
    public GameAssemblyLoader AssemblyLoader { get; } = new();
    public UndoRedoManager UndoRedo { get; } = new();

    public string? ProjectDirectory { get; private set; }
    public ProjectFile? ProjectFile { get; private set; }
    public bool ShouldResetLayout { get; set; }

    private string? _playModeSnapshot;
    private Task<bool>? _buildTask;
    private EditorNotification? _buildNotification;
    private Task<bool>? _exportTask;
    private EditorNotification? _exportNotification;
    private AssetFileWatcher? _assetFileWatcher;

    public ViewportPanel ViewportPanel { get; }
    public HierarchyPanel HierarchyPanel { get; }
    public InspectorPanel InspectorPanel { get; }
    public ConsolePanel ConsolePanel { get; }
    public AssetBrowserPanel AssetBrowserPanel { get; }
    public PerformancePanel PerformancePanel { get; }
    public MenuBar MenuBar { get; }

    public EditorApplication()
    {
        Instance = this;
        ViewportPanel = new ViewportPanel(this);
        HierarchyPanel = new HierarchyPanel(this);
        InspectorPanel = new InspectorPanel(this);
        ConsolePanel = new ConsolePanel(this);
        AssetBrowserPanel = new AssetBrowserPanel(this);
        PerformancePanel = new PerformancePanel(this);
        MenuBar = new MenuBar(this);
        RegisterKeybindActions();
    }

    public void Initialize()
    {
        SceneRenderer.LoadShader("Shaders/lighting.vs", "Shaders/lighting.fs");
        NewScene();
        FrinkyLog.Info("FrinkyEngine Editor initialized.");
    }


    public void NewScene()
    {
        ClearSelection();
        CurrentScene = SceneManager.Instance.NewScene("Untitled");

        var cameraEntity = CurrentScene.CreateEntity("Main Camera");
        cameraEntity.Transform.LocalPosition = new System.Numerics.Vector3(0, 5, 10);
        cameraEntity.Transform.EulerAngles = new System.Numerics.Vector3(-20, 0, 0);
        cameraEntity.AddComponent<Core.Components.CameraComponent>();

        var lightEntity = CurrentScene.CreateEntity("Directional Light");
        lightEntity.Transform.LocalPosition = new System.Numerics.Vector3(2, 10, 2);
        lightEntity.AddComponent<Core.Components.LightComponent>();

        EditorCamera.Reset();
        UpdateWindowTitle();
        UndoRedo.Clear();
        UndoRedo.SetBaseline(CurrentScene, Array.Empty<Guid>());
        NotificationManager.Instance.Post("New scene created", NotificationType.Info);
    }

    public void Update(float dt)
    {
        NotificationManager.Instance.Update(dt);

        if (_buildTask is { IsCompleted: true })
        {
            var success = _buildTask.Result;
            _buildTask = null;

            if (_buildNotification != null)
            {
                if (success)
                    NotificationManager.Instance.Complete(_buildNotification, "Build Succeeded!", NotificationType.Success);
                else
                    NotificationManager.Instance.Complete(_buildNotification, "Build Failed.", NotificationType.Error);
                _buildNotification = null;
            }

            if (success)
                ReloadGameAssembly();
        }

        if (_exportTask is { IsCompleted: true })
        {
            var success = _exportTask.Result;
            _exportTask = null;

            if (_exportNotification != null)
            {
                if (success)
                    NotificationManager.Instance.Complete(_exportNotification, "Export Succeeded!", NotificationType.Success);
                else
                    NotificationManager.Instance.Complete(_exportNotification, "Export Failed.", NotificationType.Error);
                _exportNotification = null;
            }
        }

        if (_assetFileWatcher != null && _assetFileWatcher.PollChanges(out bool scriptsChanged, out var changedPaths))
        {
            AssetDatabase.Instance.Refresh();

            bool assetsReloaded = false;
            if (changedPaths != null && CurrentScene != null)
            {
                var assetsPath = AssetManager.Instance.AssetsPath;
                var relativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var fullPath in changedPaths)
                {
                    if (fullPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var rel = Path.GetRelativePath(assetsPath, fullPath).Replace('\\', '/');
                        relativePaths.Add(rel);
                    }
                }

                if (relativePaths.Count > 0)
                {
                    // Invalidate components first (clear RenderModel) before unloading GPU resources
                    foreach (var renderable in CurrentScene.Renderables)
                    {
                        bool shouldInvalidate = false;
                        if (renderable is MeshRendererComponent meshRenderer)
                        {
                            if (relativePaths.Contains(meshRenderer.ModelPath))
                                shouldInvalidate = true;
                            else
                            {
                                foreach (var slot in meshRenderer.MaterialSlots)
                                {
                                    if (!string.IsNullOrEmpty(slot.TexturePath) && relativePaths.Contains(slot.TexturePath))
                                    {
                                        shouldInvalidate = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (renderable is PrimitiveComponent primitive)
                        {
                            if (!string.IsNullOrEmpty(primitive.TexturePath) && relativePaths.Contains(primitive.TexturePath))
                                shouldInvalidate = true;
                        }

                        if (shouldInvalidate)
                        {
                            renderable.Invalidate();
                            assetsReloaded = true;
                        }
                    }

                    // Now unload stale GPU resources from the cache
                    foreach (var rel in relativePaths)
                        AssetManager.Instance.InvalidateAsset(rel);
                }
            }

            NotificationManager.Instance.Post(
                assetsReloaded ? "Assets reloaded" : "Assets refreshed",
                NotificationType.Info, 2.5f);

            if (scriptsChanged)
                BuildScripts();
        }

        if (Mode == EditorMode.Play && CurrentScene != null)
        {
            CurrentScene.Update(dt);
        }
    }

    public void DrawUI()
    {
        KeybindManager.Instance.ProcessKeybinds();
        MenuBar.Draw();
        ViewportPanel.Draw();
        HierarchyPanel.Draw();
        InspectorPanel.Draw();
        ConsolePanel.Draw();
        AssetBrowserPanel.Draw();
        PerformancePanel.Draw();
        NotificationManager.Instance.Draw();
    }

    public void EnterPlayMode()
    {
        if (Mode == EditorMode.Play || CurrentScene == null) return;

        _playModeSnapshot = SceneSerializer.SerializeToString(CurrentScene);
        CurrentScene.Start();
        Mode = EditorMode.Play;
        FrinkyLog.Info("Entered Play mode.");
        NotificationManager.Instance.Post("Play mode", NotificationType.Info);
    }

    public void ExitPlayMode()
    {
        if (Mode == EditorMode.Edit) return;

        if (_playModeSnapshot != null)
        {
            var restored = SceneSerializer.DeserializeFromString(_playModeSnapshot);
            if (restored != null)
            {
                restored.FilePath = CurrentScene?.FilePath ?? string.Empty;
                CurrentScene = restored;
                SceneManager.Instance.SetActiveScene(restored);
            }
            _playModeSnapshot = null;
        }

        ClearSelection();
        Mode = EditorMode.Edit;
        UndoRedo.SetBaseline(CurrentScene, GetSelectedEntityIds());
        FrinkyLog.Info("Exited Play mode.");
        NotificationManager.Instance.Post("Edit mode", NotificationType.Info);
    }

    public void CreateAndOpenProject(string parentDirectory, string projectName)
    {
        try
        {
            var fprojectPath = ProjectScaffolder.CreateProject(parentDirectory, projectName);
            FrinkyLog.Info($"Created project: {projectName}");
            OpenProject(fprojectPath);
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"Failed to create project: {ex.Message}");
        }
    }

    public void OpenProject(string fprojectPath)
    {
        ProjectDirectory = Path.GetDirectoryName(fprojectPath);
        ProjectFile = Core.Assets.ProjectFile.Load(fprojectPath);

        if (ProjectDirectory != null)
        {
            var assetsPath = ProjectFile.GetAbsoluteAssetsPath(ProjectDirectory);
            AssetManager.Instance.AssetsPath = assetsPath;
            AssetDatabase.Instance.Scan(assetsPath);

            _assetFileWatcher?.Dispose();
            _assetFileWatcher = new AssetFileWatcher();
            _assetFileWatcher.Watch(assetsPath);

            if (!string.IsNullOrEmpty(ProjectFile.GameAssembly))
            {
                var dllPath = Path.Combine(ProjectDirectory, ProjectFile.GameAssembly);
                AssemblyLoader.LoadAssembly(dllPath);
            }

            var scenePath = ProjectFile.GetAbsoluteScenePath(ProjectDirectory);
            if (File.Exists(scenePath))
            {
                SceneManager.Instance.LoadScene(scenePath);
                CurrentScene = SceneManager.Instance.ActiveScene;
                RestoreEditorCameraFromScene();
            }
        }

        KeybindManager.Instance.LoadConfig(ProjectDirectory);
        UndoRedo.Clear();
        ClearSelection();
        UndoRedo.SetBaseline(CurrentScene, GetSelectedEntityIds());
        FrinkyLog.Info($"Opened project: {ProjectFile.ProjectName}");
        NotificationManager.Instance.Post($"Opened: {ProjectFile.ProjectName}", NotificationType.Success);
        UpdateWindowTitle();
    }

    public void UpdateWindowTitle()
    {
        var title = "FrinkyEngine Editor";
        if (ProjectFile != null)
            title += $" - {ProjectFile.ProjectName}";
        if (CurrentScene != null)
            title += $" - {CurrentScene.Name}";
        Raylib.SetWindowTitle(title);
    }

    public void BuildScripts()
    {
        if (ScriptBuilder.IsBuilding || ProjectDirectory == null || ProjectFile == null)
            return;

        var csprojPath = FindGameCsproj();
        if (csprojPath == null)
        {
            FrinkyLog.Error("No game .csproj found in project directory.");
            return;
        }

        _buildNotification = NotificationManager.Instance.PostPersistent("Building Scripts...", NotificationType.Info);
        _buildTask = Task.Run(() => ScriptBuilder.BuildAsync(csprojPath));
    }

    public void ExportGame(string outputDirectory)
    {
        if (GameExporter.IsExporting || ProjectDirectory == null || ProjectFile == null)
            return;

        var runtimeCsproj = GameExporter.FindRuntimeCsproj();
        if (runtimeCsproj == null)
        {
            FrinkyLog.Error("Could not locate Runtime .csproj. Ensure FrinkyEngine.sln is accessible.");
            NotificationManager.Instance.Post("Export failed: Runtime not found.", NotificationType.Error);
            return;
        }

        var config = new ExportConfig
        {
            ProjectName = ProjectFile.ProjectName,
            ProjectDirectory = ProjectDirectory,
            AssetsPath = ProjectFile.GetAbsoluteAssetsPath(ProjectDirectory),
            DefaultScene = ProjectFile.DefaultScene,
            GameCsprojPath = FindGameCsproj(),
            GameAssemblyDll = !string.IsNullOrEmpty(ProjectFile.GameAssembly) ? ProjectFile.GameAssembly : null,
            RuntimeCsprojPath = runtimeCsproj,
            OutputDirectory = outputDirectory
        };

        _exportNotification = NotificationManager.Instance.PostPersistent("Exporting Game...", NotificationType.Info);
        _exportTask = Task.Run(() => GameExporter.ExportAsync(config));
    }

    private string? FindGameCsproj()
    {
        if (ProjectDirectory == null || ProjectFile == null)
            return null;

        if (!string.IsNullOrEmpty(ProjectFile.GameProject))
        {
            var path = Path.Combine(ProjectDirectory, ProjectFile.GameProject);
            if (File.Exists(path))
                return path;
        }

        var csprojFiles = Directory.GetFiles(ProjectDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
        return csprojFiles.Length > 0 ? csprojFiles[0] : null;
    }

    private void ReloadGameAssembly()
    {
        if (ProjectDirectory == null || ProjectFile == null)
            return;

        var dllPath = !string.IsNullOrEmpty(ProjectFile.GameAssembly)
            ? Path.Combine(ProjectDirectory, ProjectFile.GameAssembly)
            : null;

        if (dllPath == null || !File.Exists(dllPath))
        {
            FrinkyLog.Warning("Game assembly DLL not found after build.");
            return;
        }

        AssemblyLoader.ReloadAssembly(dllPath);
        FrinkyLog.Info("Game assembly reloaded.");
        NotificationManager.Instance.Post("Game assembly reloaded.", NotificationType.Info);

        // Re-serialize and deserialize current scene to refresh component instances
        if (CurrentScene != null)
        {
            var snapshot = SceneSerializer.SerializeToString(CurrentScene);
            var refreshed = SceneSerializer.DeserializeFromString(snapshot);
            if (refreshed != null)
            {
                refreshed.Name = CurrentScene.Name;
                refreshed.FilePath = CurrentScene.FilePath;
                CurrentScene = refreshed;
                SceneManager.Instance.SetActiveScene(refreshed);
                ClearSelection();
            }
            UndoRedo.SetBaseline(CurrentScene, GetSelectedEntityIds());
        }
    }

    private void RegisterKeybindActions()
    {
        var km = KeybindManager.Instance;

        km.RegisterAction(EditorAction.NewScene, () => NewScene());

        km.RegisterAction(EditorAction.OpenScene, () => MenuBar.TriggerOpenScene());

        km.RegisterAction(EditorAction.SaveScene, () =>
        {
            if (CurrentScene != null)
            {
                StoreEditorCameraInScene();
                var path = !string.IsNullOrEmpty(CurrentScene.FilePath)
                    ? CurrentScene.FilePath
                    : "scene.fscene";
                SceneManager.Instance.SaveScene(path);
                FrinkyLog.Info($"Scene saved to: {path}");
                NotificationManager.Instance.Post("Scene saved", NotificationType.Success);
            }
        });

        km.RegisterAction(EditorAction.SaveSceneAs, () => MenuBar.TriggerSaveSceneAs());

        km.RegisterAction(EditorAction.Undo, () =>
        {
            if (Mode == EditorMode.Edit)
                UndoRedo.Undo(this);
        });
        km.RegisterAction(EditorAction.Redo, () =>
        {
            if (Mode == EditorMode.Edit)
                UndoRedo.Redo(this);
        });

        km.RegisterAction(EditorAction.BuildScripts, () => BuildScripts());

        km.RegisterAction(EditorAction.PlayStop, () =>
        {
            if (Mode == EditorMode.Edit)
                EnterPlayMode();
            else
                ExitPlayMode();
        });

        km.RegisterAction(EditorAction.DeleteEntity, () =>
        {
            DeleteSelectedEntities();
        });

        km.RegisterAction(EditorAction.DuplicateEntity, () =>
        {
            DuplicateSelectedEntities();
        });

        km.RegisterAction(EditorAction.RenameEntity, () =>
        {
            if (SelectedEntities.Count == 1)
                InspectorPanel.FocusNameField = true;
        });

        km.RegisterAction(EditorAction.NewProject, () => MenuBar.TriggerNewProject());

        km.RegisterAction(EditorAction.GizmoTranslate, () => GizmoSystem.Mode = GizmoMode.Translate);
        km.RegisterAction(EditorAction.GizmoRotate, () => GizmoSystem.Mode = GizmoMode.Rotate);
        km.RegisterAction(EditorAction.GizmoScale, () => GizmoSystem.Mode = GizmoMode.Scale);
        km.RegisterAction(EditorAction.GizmoToggleSpace, () =>
            GizmoSystem.Space = GizmoSystem.Space == GizmoSpace.World ? GizmoSpace.Local : GizmoSpace.World);

        km.RegisterAction(EditorAction.DeselectEntity, () =>
        {
            ClearSelection();
        });

        km.RegisterAction(EditorAction.OpenAssetsFolder, () =>
        {
            if (ProjectDirectory == null || ProjectFile == null) return;
            var assetsPath = ProjectFile.GetAbsoluteAssetsPath(ProjectDirectory);
            if (Directory.Exists(assetsPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = assetsPath,
                    UseShellExecute = true
                });
            }
        });

        km.RegisterAction(EditorAction.ExportGame, () => MenuBar.TriggerExportGame());
    }

    public void StoreEditorCameraInScene()
    {
        if (CurrentScene == null) return;
        CurrentScene.EditorCameraPosition = EditorCamera.Position;
        CurrentScene.EditorCameraYaw = EditorCamera.Yaw;
        CurrentScene.EditorCameraPitch = EditorCamera.Pitch;
    }

    public void RestoreEditorCameraFromScene()
    {
        if (CurrentScene?.EditorCameraPosition != null &&
            CurrentScene.EditorCameraYaw != null &&
            CurrentScene.EditorCameraPitch != null)
        {
            EditorCamera.SetState(
                CurrentScene.EditorCameraPosition.Value,
                CurrentScene.EditorCameraYaw.Value,
                CurrentScene.EditorCameraPitch.Value);
        }
        else
        {
            EditorCamera.Reset();
        }
    }

    public void RecordUndo()
    {
        if (Mode != EditorMode.Edit || CurrentScene == null) return;
        UndoRedo.RecordUndo(GetSelectedEntityIds());
    }

    public void RefreshUndoBaseline()
    {
        if (Mode != EditorMode.Edit || CurrentScene == null) return;
        UndoRedo.RefreshBaseline(CurrentScene, GetSelectedEntityIds());
    }

    public bool IsSelected(Entity entity)
    {
        return _selectedEntities.Any(e => e.Id == entity.Id);
    }

    public void ClearSelection()
    {
        _selectedEntities.Clear();
    }

    public void SetSingleSelection(Entity? entity)
    {
        if (entity == null)
        {
            ClearSelection();
            return;
        }

        SetSelection(new[] { entity });
    }

    public void SetSelection(IEnumerable<Entity> entities)
    {
        _selectedEntities.Clear();

        if (CurrentScene == null)
            return;

        var seen = new HashSet<Guid>();
        foreach (var entity in entities)
        {
            if (entity.Scene != CurrentScene)
                continue;
            if (!seen.Add(entity.Id))
                continue;

            _selectedEntities.Add(entity);
        }
    }

    public void ToggleSelection(Entity entity)
    {
        if (CurrentScene == null || entity.Scene != CurrentScene)
            return;

        var index = _selectedEntities.FindIndex(e => e.Id == entity.Id);
        if (index >= 0)
        {
            _selectedEntities.RemoveAt(index);
            return;
        }

        _selectedEntities.Add(entity);
    }

    public List<Guid> GetSelectedEntityIds()
    {
        return _selectedEntities.Select(e => e.Id).ToList();
    }

    public void DeleteSelectedEntities()
    {
        if (Mode != EditorMode.Edit || CurrentScene == null || _selectedEntities.Count == 0)
            return;

        var entitiesToDelete = _selectedEntities.Where(e => e.Scene == CurrentScene).ToList();
        if (entitiesToDelete.Count == 0)
            return;

        RecordUndo();
        foreach (var entity in entitiesToDelete)
            CurrentScene.RemoveEntity(entity);
        ClearSelection();
        RefreshUndoBaseline();
    }

    public void DuplicateSelectedEntities()
    {
        if (Mode != EditorMode.Edit || CurrentScene == null || _selectedEntities.Count == 0)
            return;

        var selected = _selectedEntities.Where(e => e.Scene == CurrentScene).ToList();
        if (selected.Count == 0)
            return;

        var selectedIds = new HashSet<Guid>(selected.Select(e => e.Id));
        var rootsOnly = selected
            .Where(entity => !HasSelectedAncestor(entity, selectedIds))
            .ToList();

        RecordUndo();
        var duplicates = new List<Entity>();
        foreach (var entity in rootsOnly)
        {
            var duplicate = SceneSerializer.DuplicateEntity(entity, CurrentScene);
            if (duplicate != null)
                duplicates.Add(duplicate);
        }

        SetSelection(duplicates);
        if (duplicates.Count > 0)
        {
            var message = duplicates.Count == 1
                ? $"Duplicated: {duplicates[0].Name}"
                : $"Duplicated {duplicates.Count} entities";
            NotificationManager.Instance.Post(message, NotificationType.Info, 1.5f);
        }
        RefreshUndoBaseline();
    }

    private static bool HasSelectedAncestor(Entity entity, HashSet<Guid> selectedIds)
    {
        var parent = entity.Transform.Parent;
        while (parent != null)
        {
            if (selectedIds.Contains(parent.Entity.Id))
                return true;
            parent = parent.Parent;
        }

        return false;
    }

    public void Shutdown()
    {
        _assetFileWatcher?.Dispose();
        _assetFileWatcher = null;
        ViewportPanel.Shutdown();
        SceneRenderer.UnloadShader();
        AssetManager.Instance.UnloadAll();
        AssetDatabase.Instance.Clear();
        AssemblyLoader.Unload();
    }
}
