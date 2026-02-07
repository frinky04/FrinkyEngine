using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Physics;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Scripting;
using Raylib_cs;

namespace FrinkyEngine.Runtime;

public static class Program
{
    private sealed class RuntimeLaunchSettings
    {
        public int TargetFps { get; init; } = 60;
        public bool VSync { get; init; }
        public int WindowWidth { get; init; } = 1280;
        public int WindowHeight { get; init; } = 720;
        public bool Resizable { get; init; } = true;
        public bool Fullscreen { get; init; }
        public bool StartMaximized { get; init; }
        public int ForwardPlusTileSize { get; init; } = ForwardPlusSettings.DefaultTileSize;
        public int ForwardPlusMaxLights { get; init; } = ForwardPlusSettings.DefaultMaxLights;
        public int ForwardPlusMaxLightsPerTile { get; init; } = ForwardPlusSettings.DefaultMaxLightsPerTile;
    }

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
        var settings = ProjectSettings.LoadOrCreate(projectDir, project.ProjectName);
        var sceneRelativePath = settings.ResolveStartupScene(project.DefaultScene);

        AssetManager.Instance.AssetsPath = project.GetAbsoluteAssetsPath(projectDir);
        PhysicsProjectSettings.ApplyFrom(settings.Runtime);

        var assemblyLoader = new GameAssemblyLoader();
        if (!string.IsNullOrEmpty(project.GameAssembly))
        {
            var dllPath = Path.Combine(projectDir, project.GameAssembly);
            assemblyLoader.LoadAssembly(dllPath);
        }

        var scenePath = Path.GetFullPath(Path.Combine(AssetManager.Instance.AssetsPath, sceneRelativePath));
        var windowTitle = string.IsNullOrWhiteSpace(settings.Runtime.WindowTitle)
            ? project.ProjectName
            : settings.Runtime.WindowTitle;

        RunGameLoop("Shaders/lighting.vs", "Shaders/lighting.fs",
            scenePath, assemblyLoader, windowTitle, new RuntimeLaunchSettings
            {
                TargetFps = settings.Runtime.TargetFps,
                VSync = settings.Runtime.VSync,
                WindowWidth = settings.Runtime.WindowWidth,
                WindowHeight = settings.Runtime.WindowHeight,
                Resizable = settings.Runtime.Resizable,
                Fullscreen = settings.Runtime.Fullscreen,
                StartMaximized = settings.Runtime.StartMaximized,
                ForwardPlusTileSize = settings.Runtime.ForwardPlusTileSize,
                ForwardPlusMaxLights = settings.Runtime.ForwardPlusMaxLights,
                ForwardPlusMaxLightsPerTile = settings.Runtime.ForwardPlusMaxLightsPerTile
            });
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
            PhysicsProjectSettings.ApplyFrom(manifest);

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
            var windowTitle = !string.IsNullOrWhiteSpace(manifest.WindowTitle)
                ? manifest.WindowTitle
                : (!string.IsNullOrWhiteSpace(manifest.ProductName) ? manifest.ProductName : manifest.ProjectName);

            RunGameLoop(shaderVs, shaderFs, scenePath, assemblyLoader, windowTitle,
                new RuntimeLaunchSettings
                {
                    TargetFps = manifest.TargetFps ?? 120,
                    VSync = manifest.VSync ?? true,
                    WindowWidth = manifest.WindowWidth ?? 1280,
                    WindowHeight = manifest.WindowHeight ?? 720,
                    Resizable = manifest.Resizable ?? true,
                    Fullscreen = manifest.Fullscreen ?? false,
                    StartMaximized = manifest.StartMaximized ?? false,
                    ForwardPlusTileSize = manifest.ForwardPlusTileSize ?? ForwardPlusSettings.DefaultTileSize,
                    ForwardPlusMaxLights = manifest.ForwardPlusMaxLights ?? ForwardPlusSettings.DefaultMaxLights,
                    ForwardPlusMaxLightsPerTile = manifest.ForwardPlusMaxLightsPerTile ?? ForwardPlusSettings.DefaultMaxLightsPerTile
                });
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

    private static void RunGameLoop(string shaderVsPath, string shaderFsPath,
        string scenePath, GameAssemblyLoader assemblyLoader, string runtimeWindowTitle, RuntimeLaunchSettings launchSettings)
    {
        RaylibLogger.Install();
        launchSettings = SanitizeLaunchSettings(launchSettings);
        var flags = ConfigFlags.Msaa4xHint;
        if (launchSettings.Resizable)
            flags |= ConfigFlags.ResizableWindow;
        if (launchSettings.VSync)
            flags |= ConfigFlags.VSyncHint;
        if (launchSettings.Fullscreen)
            flags |= ConfigFlags.FullscreenMode;
        if (launchSettings.StartMaximized)
            flags |= ConfigFlags.MaximizedWindow;

        Raylib.SetConfigFlags(flags);
        Raylib.InitWindow(launchSettings.WindowWidth, launchSettings.WindowHeight, runtimeWindowTitle);
        Raylib.SetTargetFPS(launchSettings.TargetFps);

        var sceneRenderer = new SceneRenderer();
        sceneRenderer.LoadShader(shaderVsPath, shaderFsPath);
        sceneRenderer.ConfigureForwardPlus(new ForwardPlusSettings(
            launchSettings.ForwardPlusTileSize,
            launchSettings.ForwardPlusMaxLights,
            launchSettings.ForwardPlusMaxLightsPerTile));

        var scene = SceneManager.Instance.LoadScene(scenePath);

        if (scene == null)
        {
            Console.WriteLine($"Failed to load scene: {scenePath}");
            Raylib.CloseWindow();
            return;
        }

        scene.Start();
        Raylib.DisableCursor();

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

    private static RuntimeLaunchSettings SanitizeLaunchSettings(RuntimeLaunchSettings settings)
    {
        static int ClampOrDefault(int value, int min, int max, int fallback)
        {
            if (value < min || value > max)
                return fallback;
            return value;
        }

        return new RuntimeLaunchSettings
        {
            TargetFps = ClampOrDefault(settings.TargetFps, 30, 500, 120),
            VSync = settings.VSync,
            WindowWidth = ClampOrDefault(settings.WindowWidth, 320, 10000, 1280),
            WindowHeight = ClampOrDefault(settings.WindowHeight, 200, 10000, 720),
            Resizable = settings.Resizable,
            Fullscreen = settings.Fullscreen,
            StartMaximized = settings.StartMaximized,
            ForwardPlusTileSize = ClampOrDefault(settings.ForwardPlusTileSize, 8, 64, ForwardPlusSettings.DefaultTileSize),
            ForwardPlusMaxLights = ClampOrDefault(settings.ForwardPlusMaxLights, 16, 2048, ForwardPlusSettings.DefaultMaxLights),
            ForwardPlusMaxLightsPerTile = ClampOrDefault(settings.ForwardPlusMaxLightsPerTile, 8, 256, ForwardPlusSettings.DefaultMaxLightsPerTile)
        };
    }

    private static string? FindFassetNextToExe()
    {
        var exeDir = AppContext.BaseDirectory;
        var fassetFiles = Directory.GetFiles(exeDir, "*.fasset");
        return fassetFiles.Length > 0 ? fassetFiles[0] : null;
    }
}
