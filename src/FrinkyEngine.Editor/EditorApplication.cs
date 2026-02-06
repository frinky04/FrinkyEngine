using FrinkyEngine.Core.Assets;
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
    public Entity? SelectedEntity { get; set; }
    public EditorMode Mode { get; private set; } = EditorMode.Edit;
    public EditorCamera EditorCamera { get; } = new();
    public SceneRenderer SceneRenderer { get; } = new();
    public GameAssemblyLoader AssemblyLoader { get; } = new();

    public string? ProjectDirectory { get; private set; }
    public ProjectFile? ProjectFile { get; private set; }
    public bool ShouldResetLayout { get; set; }

    private string? _playModeSnapshot;
    private Task<bool>? _buildTask;
    private EditorNotification? _buildNotification;
    private AssetFileWatcher? _assetFileWatcher;

    public ViewportPanel ViewportPanel { get; }
    public HierarchyPanel HierarchyPanel { get; }
    public InspectorPanel InspectorPanel { get; }
    public ConsolePanel ConsolePanel { get; }
    public AssetBrowserPanel AssetBrowserPanel { get; }
    public MenuBar MenuBar { get; }

    public EditorApplication()
    {
        Instance = this;
        ViewportPanel = new ViewportPanel(this);
        HierarchyPanel = new HierarchyPanel(this);
        InspectorPanel = new InspectorPanel(this);
        ConsolePanel = new ConsolePanel(this);
        AssetBrowserPanel = new AssetBrowserPanel(this);
        MenuBar = new MenuBar(this);
    }

    public void Initialize()
    {
        SceneRenderer.LoadShader("Shaders/lighting.vs", "Shaders/lighting.fs");
        NewScene();
        FrinkyLog.Info("FrinkyEngine Editor initialized.");
    }

    public void NewScene()
    {
        SelectedEntity = null;
        CurrentScene = SceneManager.Instance.NewScene("Untitled");

        var cameraEntity = CurrentScene.CreateEntity("Main Camera");
        cameraEntity.Transform.LocalPosition = new System.Numerics.Vector3(0, 5, 10);
        cameraEntity.Transform.EulerAngles = new System.Numerics.Vector3(-20, 0, 0);
        cameraEntity.AddComponent<Core.Components.CameraComponent>();

        var lightEntity = CurrentScene.CreateEntity("Directional Light");
        lightEntity.Transform.LocalPosition = new System.Numerics.Vector3(2, 10, 2);
        lightEntity.AddComponent<Core.Components.LightComponent>();

        UpdateWindowTitle();
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

        if (_assetFileWatcher != null && _assetFileWatcher.PollChanges(out bool scriptsChanged))
        {
            AssetDatabase.Instance.Refresh();
            NotificationManager.Instance.Post("Assets refreshed", NotificationType.Info, 2.5f);

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
        MenuBar.Draw();
        ViewportPanel.Draw();
        HierarchyPanel.Draw();
        InspectorPanel.Draw();
        ConsolePanel.Draw();
        AssetBrowserPanel.Draw();
        NotificationManager.Instance.Draw();
    }

    public void EnterPlayMode()
    {
        if (Mode == EditorMode.Play || CurrentScene == null) return;

        _playModeSnapshot = SceneSerializer.SerializeToString(CurrentScene);
        CurrentScene.Start();
        Mode = EditorMode.Play;
        FrinkyLog.Info("Entered Play mode.");
    }

    public void ExitPlayMode()
    {
        if (Mode == EditorMode.Edit) return;

        if (_playModeSnapshot != null)
        {
            var restored = SceneSerializer.DeserializeFromString(_playModeSnapshot);
            if (restored != null)
            {
                CurrentScene = restored;
                SceneManager.Instance.SetActiveScene(restored);
            }
            _playModeSnapshot = null;
        }

        SelectedEntity = null;
        Mode = EditorMode.Edit;
        FrinkyLog.Info("Exited Play mode.");
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
            }
        }

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
                SelectedEntity = null;
            }
        }
    }

    public void Shutdown()
    {
        _assetFileWatcher?.Dispose();
        _assetFileWatcher = null;
        SceneRenderer.UnloadShader();
        AssetManager.Instance.UnloadAll();
        AssetDatabase.Instance.Clear();
        AssemblyLoader.Unload();
    }
}
