using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Scripting;
using Raylib_cs;

namespace FrinkyEngine.Runtime;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && File.Exists(args[0]) && args[0].EndsWith(".fproject", StringComparison.OrdinalIgnoreCase))
        {
            RunDevMode(args[0]);
        }
        else
        {
            var fassetPath = FindFassetNextToExe();
            if (fassetPath != null)
            {
                RunExportedMode(fassetPath);
            }
            else
            {
                Console.WriteLine("Usage: FrinkyEngine.Runtime <path-to-.fproject>");
                Console.WriteLine("  Or place a .fasset file next to the executable.");
            }
        }
    }

    private static void RunDevMode(string fprojectPath)
    {
        var projectDir = Path.GetDirectoryName(Path.GetFullPath(fprojectPath))!;
        var project = ProjectFile.Load(fprojectPath);

        AssetManager.Instance.AssetsPath = project.GetAbsoluteAssetsPath(projectDir);

        var assemblyLoader = new GameAssemblyLoader();
        if (!string.IsNullOrEmpty(project.GameAssembly))
        {
            var dllPath = Path.Combine(projectDir, project.GameAssembly);
            assemblyLoader.LoadAssembly(dllPath);
        }

        RunGameLoop(project.ProjectName, "Shaders/lighting.vs", "Shaders/lighting.fs",
            project.GetAbsoluteScenePath(projectDir), assemblyLoader);
    }

    private static void RunExportedMode(string fassetPath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"FrinkyRuntime_{Guid.NewGuid():N}");

        try
        {
            FAssetArchive.ExtractAll(fassetPath, tempDir);

            var manifestJson = File.ReadAllText(Path.Combine(tempDir, "manifest.json"));
            var manifest = ExportManifest.FromJson(manifestJson);

            AssetManager.Instance.AssetsPath = Path.Combine(tempDir, "Assets");

            var assemblyLoader = new GameAssemblyLoader();
            if (!string.IsNullOrEmpty(manifest.GameAssembly))
            {
                var dllPath = Path.Combine(tempDir, manifest.GameAssembly);
                if (File.Exists(dllPath))
                    assemblyLoader.LoadAssembly(dllPath);
            }

            var shaderVs = Path.Combine(tempDir, "Shaders", "lighting.vs");
            var shaderFs = Path.Combine(tempDir, "Shaders", "lighting.fs");
            var scenePath = Path.Combine(tempDir, manifest.DefaultScene);

            RunGameLoop(manifest.ProjectName, shaderVs, shaderFs, scenePath, assemblyLoader);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    private static void RunGameLoop(string windowTitle, string shaderVsPath, string shaderFsPath,
        string scenePath, GameAssemblyLoader assemblyLoader)
    {
        RaylibLogger.Install();
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(1280, 720, windowTitle);
        Raylib.SetTargetFPS(60);

        var sceneRenderer = new SceneRenderer();
        sceneRenderer.LoadShader(shaderVsPath, shaderFsPath);

        var scene = SceneManager.Instance.LoadScene(scenePath);

        if (scene == null)
        {
            Console.WriteLine($"Failed to load scene: {scenePath}");
            Raylib.CloseWindow();
            return;
        }

        scene.Start();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            scene.Update(dt);

            var mainCamera = scene.MainCamera;
            if (mainCamera == null)
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText("No MainCamera found in scene.", 10, 10, 20, Color.Red);
                Raylib.EndDrawing();
                continue;
            }

            var camera3D = mainCamera.BuildCamera3D();

            Raylib.BeginDrawing();
            sceneRenderer.Render(scene, camera3D, isEditorMode: false);
            Raylib.EndDrawing();
        }

        sceneRenderer.UnloadShader();
        AssetManager.Instance.UnloadAll();
        assemblyLoader.Unload();
        Raylib.CloseWindow();
    }

    private static string? FindFassetNextToExe()
    {
        var exeDir = AppContext.BaseDirectory;
        var fassetFiles = Directory.GetFiles(exeDir, "*.fasset");
        return fassetFiles.Length > 0 ? fassetFiles[0] : null;
    }
}
