using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Scripting;
using FrinkyEngine.Core.Serialization;
using FrinkyEngine.Editor.Panels;

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

    public ViewportPanel ViewportPanel { get; }
    public HierarchyPanel HierarchyPanel { get; }
    public InspectorPanel InspectorPanel { get; }
    public ConsolePanel ConsolePanel { get; }
    public MenuBar MenuBar { get; }

    public EditorApplication()
    {
        Instance = this;
        ViewportPanel = new ViewportPanel(this);
        HierarchyPanel = new HierarchyPanel(this);
        InspectorPanel = new InspectorPanel(this);
        ConsolePanel = new ConsolePanel(this);
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
    }

    public void Update(float dt)
    {
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
            AssetManager.Instance.AssetsPath = ProjectFile.GetAbsoluteAssetsPath(ProjectDirectory);

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
    }

    public void Shutdown()
    {
        SceneRenderer.UnloadShader();
        AssetManager.Instance.UnloadAll();
        AssemblyLoader.Unload();
    }
}
