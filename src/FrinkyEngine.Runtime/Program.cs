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
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: FrinkyEngine.Runtime <path-to-.fproject>");
            return;
        }

        var fprojectPath = args[0];
        if (!File.Exists(fprojectPath))
        {
            Console.WriteLine($"Project file not found: {fprojectPath}");
            return;
        }

        var projectDir = Path.GetDirectoryName(Path.GetFullPath(fprojectPath))!;
        var project = ProjectFile.Load(fprojectPath);

        AssetManager.Instance.AssetsPath = project.GetAbsoluteAssetsPath(projectDir);

        var assemblyLoader = new GameAssemblyLoader();
        if (!string.IsNullOrEmpty(project.GameAssembly))
        {
            var dllPath = Path.Combine(projectDir, project.GameAssembly);
            assemblyLoader.LoadAssembly(dllPath);
        }

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(1280, 720, project.ProjectName);
        Raylib.SetTargetFPS(60);

        var sceneRenderer = new SceneRenderer();
        sceneRenderer.LoadShader("Shaders/lighting.vs", "Shaders/lighting.fs");

        var scenePath = project.GetAbsoluteScenePath(projectDir);
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
            sceneRenderer.Render(scene, camera3D);
            Raylib.EndDrawing();
        }

        sceneRenderer.UnloadShader();
        AssetManager.Instance.UnloadAll();
        assemblyLoader.Unload();
        Raylib.CloseWindow();
    }
}
