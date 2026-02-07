using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;
using NativeFileDialogSharp;

namespace FrinkyEngine.Editor.Panels;

public class MenuBar
{
    private readonly EditorApplication _app;
    private string _newProjectName = string.Empty;
    private string _newProjectParentDir = string.Empty;

    private bool _openNewProject;
    private bool _openCreateScript;
    private bool _openProjectSettings;
    private ProjectSettings? _projectSettingsDraft;
    private ProjectSettings? _projectSettingsBaseline;
    private EditorProjectSettings? _editorProjectSettingsDraft;
    private EditorProjectSettings? _editorProjectSettingsBaseline;

    // Create Script modal state
    private string _newScriptName = string.Empty;
    private int _selectedBaseClassIndex;
    private string[] _baseClassOptions = Array.Empty<string>();
    private Type[] _baseClassTypes = Array.Empty<Type>();

    public MenuBar(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Scene", KeybindManager.Instance.GetShortcutText(EditorAction.NewScene)))
                {
                    _app.NewScene();
                }

                if (ImGui.MenuItem("Open Scene...", KeybindManager.Instance.GetShortcutText(EditorAction.OpenScene)))
                {
                    OpenSceneDialog();
                }

                if (ImGui.MenuItem("Save Scene", KeybindManager.Instance.GetShortcutText(EditorAction.SaveScene)))
                {
                    if (_app.CurrentScene != null)
                    {
                        _app.StoreEditorCameraInScene();
                        var path = !string.IsNullOrEmpty(_app.CurrentScene.FilePath)
                            ? _app.CurrentScene.FilePath
                            : "scene.fscene";
                        SceneManager.Instance.SaveScene(path);
                        FrinkyLog.Info($"Scene saved to: {path}");
                        NotificationManager.Instance.Post("Scene saved", NotificationType.Success);
                    }
                }

                if (ImGui.MenuItem("Save Scene As...", KeybindManager.Instance.GetShortcutText(EditorAction.SaveSceneAs)))
                {
                    SaveSceneAs();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("New Project...", KeybindManager.Instance.GetShortcutText(EditorAction.NewProject)))
                    _openNewProject = true;

                if (ImGui.MenuItem("Open Project..."))
                {
                    OpenProjectDialog();
                }

                var hasProjectSettings = _app.ProjectDirectory != null
                                         && _app.ProjectSettings != null
                                         && _app.ProjectEditorSettings != null;
                ImGui.BeginDisabled(!hasProjectSettings);
                if (ImGui.MenuItem("Project Settings..."))
                {
                    OpenProjectSettingsPopup();
                }
                ImGui.EndDisabled();

                ImGui.Separator();

                var hasProjectForAssets = _app.ProjectDirectory != null;
                ImGui.BeginDisabled(!hasProjectForAssets);
                if (ImGui.MenuItem("Open Assets Folder", KeybindManager.Instance.GetShortcutText(EditorAction.OpenAssetsFolder)))
                {
                    if (_app.ProjectDirectory != null && _app.ProjectFile != null)
                    {
                        var assetsPath = _app.ProjectFile.GetAbsoluteAssetsPath(_app.ProjectDirectory);
                        if (Directory.Exists(assetsPath))
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = assetsPath,
                                UseShellExecute = true
                            });
                        }
                    }
                }

                if (ImGui.MenuItem("Open Project in VS Code", KeybindManager.Instance.GetShortcutText(EditorAction.OpenProjectInVSCode)))
                {
                    _app.OpenProjectInVSCode();
                }
                ImGui.EndDisabled();

                ImGui.Separator();

                var canExport = _app.ProjectDirectory != null
                    && _app.Mode == EditorMode.Edit
                    && !GameExporter.IsExporting
                    && !ScriptBuilder.IsBuilding;
                ImGui.BeginDisabled(!canExport);
                if (ImGui.MenuItem("Export Game...", KeybindManager.Instance.GetShortcutText(EditorAction.ExportGame)))
                {
                    ExportGameDialog();
                }
                ImGui.EndDisabled();

                if (GameExporter.IsExporting)
                    ImGui.TextDisabled("Exporting...");

                ImGui.Separator();

                if (ImGui.MenuItem("Exit"))
                {
                    Raylib_cs.Raylib.CloseWindow();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                bool canUndo = _app.UndoRedo.CanUndo && _app.Mode == EditorMode.Edit;
                bool canRedo = _app.UndoRedo.CanRedo && _app.Mode == EditorMode.Edit;

                if (ImGui.MenuItem("Undo", KeybindManager.Instance.GetShortcutText(EditorAction.Undo), false, canUndo))
                    _app.UndoRedo.Undo(_app);
                if (ImGui.MenuItem("Redo", KeybindManager.Instance.GetShortcutText(EditorAction.Redo), false, canRedo))
                    _app.UndoRedo.Redo(_app);

                ImGui.Separator();

                if (ImGui.MenuItem("Select All", KeybindManager.Instance.GetShortcutText(EditorAction.SelectAllEntities)))
                {
                    if (_app.CurrentScene != null)
                        _app.SetSelection(_app.CurrentScene.Entities);
                }

                if (ImGui.MenuItem("Hierarchy Search", KeybindManager.Instance.GetShortcutText(EditorAction.FocusHierarchySearch)))
                {
                    _app.HierarchyPanel.FocusSearch();
                }

                ImGui.Separator();

                var hasSelection = _app.SelectedEntities.Count > 0;
                var hasSingleSelection = _app.SelectedEntities.Count == 1;

                ImGui.BeginDisabled(!hasSelection);
                if (ImGui.MenuItem("Delete", KeybindManager.Instance.GetShortcutText(EditorAction.DeleteEntity)))
                {
                    _app.DeleteSelectedEntities();
                }

                if (ImGui.MenuItem("Duplicate", KeybindManager.Instance.GetShortcutText(EditorAction.DuplicateEntity)))
                {
                    _app.DuplicateSelectedEntities();
                }
                ImGui.EndDisabled();

                ImGui.Separator();

                ImGui.BeginDisabled(!hasSingleSelection);
                if (ImGui.MenuItem("Create Prefab from Selection", KeybindManager.Instance.GetShortcutText(EditorAction.CreatePrefabFromSelection)))
                    _app.CreatePrefabFromSelection();

                var prefabRoot = _app.Prefabs.GetPrefabRoot(_app.SelectedEntity);
                bool isPrefabRootSelection = prefabRoot != null
                                             && _app.SelectedEntity != null
                                             && prefabRoot.Id == _app.SelectedEntity.Id;

                ImGui.BeginDisabled(!isPrefabRootSelection);
                if (ImGui.MenuItem("Apply Prefab", KeybindManager.Instance.GetShortcutText(EditorAction.ApplyPrefab)))
                    _app.ApplySelectedPrefab();
                if (ImGui.MenuItem("Revert Prefab", KeybindManager.Instance.GetShortcutText(EditorAction.RevertPrefab)))
                    _app.RevertSelectedPrefab();
                if (ImGui.MenuItem("Make Unique Prefab", KeybindManager.Instance.GetShortcutText(EditorAction.MakeUniquePrefab)))
                    _app.MakeUniqueSelectedPrefab();
                if (ImGui.MenuItem("Unpack Prefab", KeybindManager.Instance.GetShortcutText(EditorAction.UnpackPrefab)))
                    _app.UnpackSelectedPrefab();
                ImGui.EndDisabled();
                ImGui.EndDisabled();

                ImGui.BeginDisabled(!hasSingleSelection);
                if (ImGui.MenuItem("Rename", KeybindManager.Instance.GetShortcutText(EditorAction.RenameEntity)))
                {
                    _app.InspectorPanel.FocusNameField = true;
                }
                ImGui.EndDisabled();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Scripts"))
            {
                var hasProject = _app.ProjectDirectory != null;
                var isBuilding = ScriptBuilder.IsBuilding;

                ImGui.BeginDisabled(!hasProject || isBuilding);
                if (ImGui.MenuItem("Build Scripts", KeybindManager.Instance.GetShortcutText(EditorAction.BuildScripts)))
                {
                    _app.BuildScripts();
                }
                ImGui.EndDisabled();

                if (isBuilding)
                {
                    ImGui.TextDisabled("Building...");
                }

                ImGui.Separator();

                ImGui.BeginDisabled(!hasProject);
                if (ImGui.MenuItem("Create Script..."))
                {
                    _openCreateScript = true;
                    _newScriptName = string.Empty;
                    _selectedBaseClassIndex = 0;
                    RefreshBaseClassOptions();
                }
                ImGui.EndDisabled();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem(
                        "Game View",
                        KeybindManager.Instance.GetShortcutText(EditorAction.ToggleGameView),
                        _app.IsGameViewEnabled))
                {
                    _app.ToggleGameView();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                ImGui.MenuItem("Viewport", null, true);
                ImGui.MenuItem("Hierarchy", null, true);
                ImGui.MenuItem("Inspector", null, true);
                ImGui.MenuItem("Console", null, true);
                ImGui.MenuItem("Assets", null, true);
                if (ImGui.MenuItem("Performance", null, _app.PerformancePanel.IsVisible))
                    _app.PerformancePanel.IsVisible = !_app.PerformancePanel.IsVisible;
                ImGui.Separator();
                if (ImGui.MenuItem("Reset Layout"))
                {
                    _app.ShouldResetLayout = true;
                }
                ImGui.EndMenu();
            }

            ImGui.Separator();

            var shortcut = KeybindManager.Instance.GetShortcutText(EditorAction.PlayStop);
            if (_app.Mode == EditorMode.Edit)
            {
                if (ImGui.MenuItem("Play", shortcut))
                    _app.EnterPlayMode();
            }
            else
            {
                if (ImGui.MenuItem("Stop", shortcut))
                    _app.ExitPlayMode();
            }

            ImGui.EndMainMenuBar();
        }

        // Open popups at this scope level (outside the menu) so BeginPopup can find them
        if (_openNewProject)
        {
            ImGui.OpenPopup("NewProject");
            _openNewProject = false;
        }

        if (_openCreateScript)
        {
            ImGui.OpenPopup("CreateScript");
            _openCreateScript = false;
        }

        if (_openProjectSettings)
        {
            ImGui.OpenPopup("ProjectSettings");
            _openProjectSettings = false;
        }

        DrawNewProjectPopup();
        DrawCreateScriptPopup();
        DrawProjectSettingsPopup();
    }

    private void RefreshBaseClassOptions()
    {
        var types = new List<Type> { typeof(Component) };
        foreach (var type in ComponentTypeResolver.GetAllComponentTypes())
        {
            if (type != typeof(Core.Components.TransformComponent) && !type.IsAbstract)
                types.Add(type);
        }

        _baseClassTypes = types.ToArray();
        _baseClassOptions = types.Select(t => t.Name).ToArray();
    }

    private void OpenSceneDialog()
    {
        string? defaultPath = null;
        if (_app.ProjectDirectory != null && _app.ProjectFile != null)
        {
            var assetsDir = _app.ProjectFile.GetAbsoluteAssetsPath(_app.ProjectDirectory);
            var scenesDir = Path.Combine(assetsDir, "Scenes");
            if (Directory.Exists(scenesDir))
                defaultPath = scenesDir;
            else if (Directory.Exists(assetsDir))
                defaultPath = assetsDir;
        }

        var result = Dialog.FileOpen("fscene", defaultPath);
        if (!result.IsOk) return;

        SceneManager.Instance.LoadScene(result.Path);
        _app.CurrentScene = SceneManager.Instance.ActiveScene;
        _app.Prefabs.RecalculateOverridesForScene();
        _app.ClearSelection();
        _app.RestoreEditorCameraFromScene();
        _app.UpdateWindowTitle();
        _app.UndoRedo.Clear();
        _app.UndoRedo.SetBaseline(_app.CurrentScene, _app.GetSelectedEntityIds(), _app.SerializeCurrentHierarchyState());
        FrinkyLog.Info($"Opened scene: {result.Path}");
        NotificationManager.Instance.Post($"Scene opened: {_app.CurrentScene?.Name ?? "scene"}", NotificationType.Success);
    }

    private void SaveSceneAs()
    {
        if (_app.CurrentScene == null) return;
        _app.StoreEditorCameraInScene();

        // Default to the project's assets directory if a project is open
        string? defaultPath = null;
        if (_app.ProjectDirectory != null && _app.ProjectFile != null)
        {
            var assetsDir = _app.ProjectFile.GetAbsoluteAssetsPath(_app.ProjectDirectory);
            var scenesDir = Path.Combine(assetsDir, "Scenes");
            if (Directory.Exists(scenesDir))
                defaultPath = scenesDir;
            else if (Directory.Exists(assetsDir))
                defaultPath = assetsDir;
        }

        var result = Dialog.FileSave("fscene", defaultPath);
        if (!result.IsOk) return;

        var path = result.Path;

        // Auto-append .fscene if missing
        if (!path.EndsWith(".fscene", StringComparison.OrdinalIgnoreCase))
            path += ".fscene";

        SceneManager.Instance.SaveScene(path);
        _app.UpdateWindowTitle();
        FrinkyLog.Info($"Scene saved to: {path}");
        NotificationManager.Instance.Post("Scene saved", NotificationType.Success);
    }

    private void OpenProjectDialog()
    {
        var result = Dialog.FileOpen("fproject");
        if (!result.IsOk) return;

        _app.OpenProject(result.Path);
    }

    private void DrawNewProjectPopup()
    {
        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
                                 viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(500, 0), ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal("NewProject", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Project Name", ref _newProjectName, 256);

            ImGui.InputText("Parent Directory", ref _newProjectParentDir, 512);
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                var result = Dialog.FolderPicker(
                    string.IsNullOrWhiteSpace(_newProjectParentDir) ? null : _newProjectParentDir);
                if (result.IsOk)
                    _newProjectParentDir = result.Path;
            }

            if (!string.IsNullOrWhiteSpace(_newProjectName) && !string.IsNullOrWhiteSpace(_newProjectParentDir))
            {
                var targetPath = Path.Combine(_newProjectParentDir, _newProjectName);
                ImGui.TextDisabled($"Target: {targetPath}");

                if (Directory.Exists(targetPath))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0.3f, 0.3f, 1));
                    ImGui.TextWrapped("Target directory already exists!");
                    ImGui.PopStyleColor();
                }
            }

            var parentExists = !string.IsNullOrWhiteSpace(_newProjectParentDir) && Directory.Exists(_newProjectParentDir);
            var nameValid = !string.IsNullOrWhiteSpace(_newProjectName);
            var targetExists = nameValid && parentExists && Directory.Exists(Path.Combine(_newProjectParentDir, _newProjectName));

            ImGui.BeginDisabled(!nameValid || !parentExists || targetExists);
            if (ImGui.Button("Create"))
            {
                _app.CreateAndOpenProject(_newProjectParentDir, _newProjectName);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    public void TriggerOpenScene() => OpenSceneDialog();
    public void TriggerSaveSceneAs() => SaveSceneAs();
    public void TriggerNewProject() => _openNewProject = true;

    public void TriggerExportGame()
    {
        if (_app.ProjectDirectory != null && _app.Mode == EditorMode.Edit
            && !GameExporter.IsExporting && !ScriptBuilder.IsBuilding)
        {
            ExportGameDialog();
        }
    }

    private void ExportGameDialog()
    {
        var result = Dialog.FolderPicker(_app.ProjectDirectory);
        if (!result.IsOk) return;

        // Auto-save scene if it has a file path
        if (_app.CurrentScene != null && !string.IsNullOrEmpty(_app.CurrentScene.FilePath))
        {
            _app.StoreEditorCameraInScene();
            SceneManager.Instance.SaveScene(_app.CurrentScene.FilePath);
        }

        _app.ExportGame(result.Path);
    }

    private void DrawCreateScriptPopup()
    {
        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
                                 viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(450, 0), ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal("CreateScript", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Script Name", ref _newScriptName, 256);

            if (_baseClassOptions.Length > 0)
            {
                ImGui.Combo("Base Class", ref _selectedBaseClassIndex, _baseClassOptions, _baseClassOptions.Length);
            }

            // File path preview
            var nameValid = ScriptCreator.IsValidClassName(_newScriptName);
            if (nameValid && _app.ProjectDirectory != null)
            {
                var filePath = Path.Combine(_app.ProjectDirectory, "Assets", "Scripts", $"{_newScriptName}.cs");
                var relativePath = Path.GetRelativePath(_app.ProjectDirectory, filePath);
                ImGui.TextDisabled($"File: {relativePath}");

                if (File.Exists(filePath))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.3f, 0.3f, 1));
                    ImGui.TextWrapped("A script with this name already exists!");
                    ImGui.PopStyleColor();
                }
            }
            else if (!string.IsNullOrEmpty(_newScriptName) && !nameValid)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.3f, 0.3f, 1));
                ImGui.TextWrapped("Invalid C# class name.");
                ImGui.PopStyleColor();
            }

            var canCreate = nameValid
                && _app.ProjectDirectory != null
                && !File.Exists(Path.Combine(_app.ProjectDirectory, "Assets", "Scripts", $"{_newScriptName}.cs"));

            ImGui.BeginDisabled(!canCreate);
            if (ImGui.Button("Create"))
            {
                var baseType = _selectedBaseClassIndex < _baseClassTypes.Length
                    ? _baseClassTypes[_selectedBaseClassIndex]
                    : typeof(Component);

                var namespaceName = _app.ProjectFile?.ProjectName ?? "Game";
                var scriptsDir = Path.Combine(_app.ProjectDirectory!, "Assets", "Scripts");
                Directory.CreateDirectory(scriptsDir);

                var filePath = Path.Combine(scriptsDir, $"{_newScriptName}.cs");
                var content = ScriptCreator.GenerateScript(_newScriptName, namespaceName, baseType);
                File.WriteAllText(filePath, content);
                var logPath = Path.GetRelativePath(_app.ProjectDirectory!, filePath);
                FrinkyLog.Info($"Created script: {logPath}");
                NotificationManager.Instance.Post($"Created script: {_newScriptName}.cs", NotificationType.Success);

                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    private void OpenProjectSettingsPopup()
    {
        if (_app.ProjectSettings == null || _app.ProjectEditorSettings == null)
            return;

        _projectSettingsDraft = _app.ProjectSettings.Clone();
        _projectSettingsBaseline = _app.ProjectSettings.Clone();
        _editorProjectSettingsDraft = _app.ProjectEditorSettings.Clone();
        _editorProjectSettingsBaseline = _app.ProjectEditorSettings.Clone();
        _openProjectSettings = true;
    }

    private void DrawProjectSettingsPopup()
    {
        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
                                 viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(620, 0), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal("ProjectSettings", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (_projectSettingsDraft == null
            || _projectSettingsBaseline == null
            || _editorProjectSettingsDraft == null
            || _editorProjectSettingsBaseline == null)
        {
            ImGui.TextDisabled("Project settings are not loaded.");
            if (ImGui.Button("Close"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var draft = _projectSettingsDraft;
        var editorDraft = _editorProjectSettingsDraft;

        if (ImGui.CollapsingHeader("Project", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var version = draft.Project.Version;
            EditText("Version", ref version, 64);
            draft.Project.Version = version;

            var author = draft.Project.Author;
            EditText("Author", ref author, 128);
            draft.Project.Author = author;

            var company = draft.Project.Company;
            EditText("Company", ref company, 128);
            draft.Project.Company = company;

            var description = draft.Project.Description;
            EditTextMultiline("Description", ref description, 512, new Vector2(560, 64));
            draft.Project.Description = description;
        }

        if (ImGui.CollapsingHeader("Editor", ImGuiTreeNodeFlags.DefaultOpen))
        {
            int editorFps = editorDraft.TargetFps;
            if (ImGui.InputInt("Editor Target FPS", ref editorFps))
                editorDraft.TargetFps = editorFps;

            bool editorVSync = editorDraft.VSync;
            if (ImGui.Checkbox("Editor VSync", ref editorVSync))
                editorDraft.VSync = editorVSync;
        }

        if (ImGui.CollapsingHeader("Runtime", ImGuiTreeNodeFlags.DefaultOpen))
        {
            int runtimeFps = draft.Runtime.TargetFps;
            if (ImGui.InputInt("Runtime Target FPS", ref runtimeFps))
                draft.Runtime.TargetFps = runtimeFps;

            bool vSync = draft.Runtime.VSync;
            if (ImGui.Checkbox("VSync", ref vSync))
                draft.Runtime.VSync = vSync;

            var windowTitle = draft.Runtime.WindowTitle;
            EditText("Window Title", ref windowTitle, 256);
            draft.Runtime.WindowTitle = windowTitle;

            int windowWidth = draft.Runtime.WindowWidth;
            if (ImGui.InputInt("Window Width", ref windowWidth))
                draft.Runtime.WindowWidth = windowWidth;

            int windowHeight = draft.Runtime.WindowHeight;
            if (ImGui.InputInt("Window Height", ref windowHeight))
                draft.Runtime.WindowHeight = windowHeight;

            bool resizable = draft.Runtime.Resizable;
            if (ImGui.Checkbox("Resizable", ref resizable))
                draft.Runtime.Resizable = resizable;

            bool fullscreen = draft.Runtime.Fullscreen;
            if (ImGui.Checkbox("Fullscreen", ref fullscreen))
                draft.Runtime.Fullscreen = fullscreen;

            bool startMaximized = draft.Runtime.StartMaximized;
            if (ImGui.Checkbox("Start Maximized", ref startMaximized))
                draft.Runtime.StartMaximized = startMaximized;

            int forwardPlusTileSize = draft.Runtime.ForwardPlusTileSize;
            if (ImGui.InputInt("Forward+ Tile Size", ref forwardPlusTileSize))
                draft.Runtime.ForwardPlusTileSize = forwardPlusTileSize;

            int forwardPlusMaxLights = draft.Runtime.ForwardPlusMaxLights;
            if (ImGui.InputInt("Forward+ Max Lights", ref forwardPlusMaxLights))
                draft.Runtime.ForwardPlusMaxLights = forwardPlusMaxLights;

            int forwardPlusMaxLightsPerTile = draft.Runtime.ForwardPlusMaxLightsPerTile;
            if (ImGui.InputInt("Forward+ Max Lights Per Tile", ref forwardPlusMaxLightsPerTile))
                draft.Runtime.ForwardPlusMaxLightsPerTile = forwardPlusMaxLightsPerTile;

            DrawStartupSceneSelector(draft);
            ImGui.TextDisabled("Use .fproject defaultScene or choose any scene asset.");
        }

        if (ImGui.CollapsingHeader("Build", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var outputName = draft.Build.OutputName;
            EditText("Output Name", ref outputName, 128);
            draft.Build.OutputName = outputName;

            var buildVersion = draft.Build.BuildVersion;
            EditText("Build Version", ref buildVersion, 64);
            draft.Build.BuildVersion = buildVersion;
        }

        ImGui.Separator();

        if (ImGui.Button("Apply"))
        {
            var requiresRestartNotice = HasDeferredRuntimeWindowChanges(_projectSettingsBaseline, draft);
            var editorSettingsChanged = _editorProjectSettingsBaseline.TargetFps != editorDraft.TargetFps
                                        || _editorProjectSettingsBaseline.VSync != editorDraft.VSync;
            _app.SaveProjectSettings(draft.Clone());
            _app.SaveEditorProjectSettings(editorDraft.Clone());

            if (requiresRestartNotice)
            {
                NotificationManager.Instance.Post(
                    "Settings saved. Runtime window mode/size changes apply on next launch.",
                    NotificationType.Info, 4.0f);
            }
            else
            {
                NotificationManager.Instance.Post("Project settings saved.", NotificationType.Success);
            }

            if (editorSettingsChanged)
            {
                NotificationManager.Instance.Post(
                    "Editor FPS/VSync applied immediately.",
                    NotificationType.Info, 2.5f);
            }

            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private static bool HasDeferredRuntimeWindowChanges(ProjectSettings before, ProjectSettings after)
    {
        return before.Runtime.VSync != after.Runtime.VSync
               || before.Runtime.WindowWidth != after.Runtime.WindowWidth
               || before.Runtime.WindowHeight != after.Runtime.WindowHeight
               || before.Runtime.Resizable != after.Runtime.Resizable
               || before.Runtime.Fullscreen != after.Runtime.Fullscreen
               || before.Runtime.StartMaximized != after.Runtime.StartMaximized
               || !string.Equals(before.Runtime.WindowTitle, after.Runtime.WindowTitle, StringComparison.Ordinal)
               || !string.Equals(before.Runtime.StartupSceneOverride, after.Runtime.StartupSceneOverride, StringComparison.Ordinal);
    }

    private static void DrawStartupSceneSelector(ProjectSettings draft)
    {
        var current = NormalizeAssetPath(draft.Runtime.StartupSceneOverride);
        var sceneAssets = AssetDatabase.Instance.GetAssets(AssetType.Scene)
            .Select(a => a.RelativePath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var hasCurrent = !string.IsNullOrWhiteSpace(current)
                         && sceneAssets.Any(path => string.Equals(path, current, StringComparison.OrdinalIgnoreCase));

        string preview;
        if (string.IsNullOrWhiteSpace(current))
            preview = "Use .fproject default";
        else if (hasCurrent)
            preview = current;
        else
            preview = $"{current} (missing)";

        if (ImGui.BeginCombo("Startup Scene Override", preview))
        {
            var isDefaultSelected = string.IsNullOrWhiteSpace(current);
            if (ImGui.Selectable("Use .fproject default", isDefaultSelected))
                draft.Runtime.StartupSceneOverride = string.Empty;

            foreach (var scene in sceneAssets)
            {
                var selected = string.Equals(scene, current, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(scene, selected))
                    draft.Runtime.StartupSceneOverride = scene;
            }

            if (!string.IsNullOrWhiteSpace(current) && !hasCurrent)
            {
                ImGui.Separator();
                ImGui.TextDisabled($"Missing: {current}");
            }

            ImGui.EndCombo();
        }
    }

    private static string NormalizeAssetPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim().Replace('\\', '/');
        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized["Assets/".Length..];
        return normalized;
    }

    private static void EditText(string label, ref string value, uint maxLength)
    {
        var local = value;
        if (ImGui.InputText(label, ref local, maxLength))
            value = local;
    }

    private static void EditTextMultiline(string label, ref string value, uint maxLength, Vector2 size)
    {
        var local = value;
        if (ImGui.InputTextMultiline(label, ref local, maxLength, size))
            value = local;
    }
}
