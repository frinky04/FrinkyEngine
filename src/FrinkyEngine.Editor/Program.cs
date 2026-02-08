using System.Numerics;
using FrinkyEngine.Core.Rendering;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class Program
{
    private static bool _layoutInitialized;
    private static bool _cursorWasLockedLastFrame;

    public static void Main(string[] args)
    {
        RaylibLogger.Install();
        var startupEditorSettings = TryLoadStartupEditorSettings(args);
        var flags = ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint;
        if (startupEditorSettings?.VSync == true)
            flags |= ConfigFlags.VSyncHint;

        Raylib.SetConfigFlags(flags);
        Raylib.InitWindow(1600, 900, "FrinkyEngine Editor");
        Raylib.SetTargetFPS(startupEditorSettings?.TargetFps ?? 120);
        Raylib.SetExitKey(0);

        RlImGui.Setup(true, true);

        // Load JetBrains Mono font
        var io = ImGui.GetIO();
        unsafe
        {
            var fontPath = "EditorAssets/Fonts/JetBrains_Mono/static/JetBrainsMono-Regular.ttf";
            if (File.Exists(fontPath))
            {
                io.Fonts.AddFontFromFileTTF(fontPath, 16.0f);
                RlImGui.ReloadFonts();
            }
        }

        // Configure ImGui dark style
        var style = ImGui.GetStyle();
        style.WindowRounding = 2f;
        style.FrameRounding = 2f;
        style.GrabRounding = 2f;
        style.ScrollbarRounding = 2f;
        style.TabRounding = 2f;

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        var app = new EditorApplication();
        app.Initialize();

        // Handle project argument
        if (args.Length > 0 && File.Exists(args[0]))
        {
            app.OpenProject(args[0]);
        }

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            app.Update(dt);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(30, 30, 30, 255));

            // Suppress ImGui mouse input while cursor is locked (camera fly or play mode)
            if (_cursorWasLockedLastFrame)
            {
                io.ConfigFlags |= ImGuiConfigFlags.NoMouse | ImGuiConfigFlags.NoMouseCursorChange;
            }

            RlImGui.Begin(dt);

            DrawDockspace(app);

            app.DrawUI();
            MessageBoxes.Draw();

            RlImGui.End();

            // Clear suppression flags and update tracking for next frame
            io.ConfigFlags &= ~(ImGuiConfigFlags.NoMouse | ImGuiConfigFlags.NoMouseCursorChange);
            _cursorWasLockedLastFrame = app.IsCursorLocked;

            Raylib.EndDrawing();
        }

        app.Shutdown();
        RlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    private static EditorProjectSettings? TryLoadStartupEditorSettings(string[] args)
    {
        if (args.Length == 0)
            return null;

        var path = args[0];
        if (!File.Exists(path) || !path.EndsWith(".fproject", StringComparison.OrdinalIgnoreCase))
            return null;

        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrWhiteSpace(projectDirectory))
            return null;

        return EditorProjectSettings.LoadOrCreate(projectDirectory);
    }

    private static void DrawDockspace(EditorApplication app)
    {
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        var windowFlags =
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.NoBackground;

        ImGui.Begin("##DockSpaceHost", windowFlags);
        ImGui.PopStyleVar(3);

        uint dockspaceId = ImGui.GetID("FrinkyDockspace");

        if (!_layoutInitialized || app.ShouldResetLayout)
        {
            _layoutInitialized = true;
            app.ShouldResetLayout = false;
            BuildDefaultLayout(dockspaceId, viewport.WorkSize);
        }

        ImGui.DockSpace(dockspaceId, Vector2.Zero, ImGuiDockNodeFlags.None);
        ImGui.End();
    }

    private static unsafe void BuildDefaultLayout(uint dockspaceId, Vector2 size)
    {
        ImGuiP.DockBuilderRemoveNode(dockspaceId);
        ImGuiP.DockBuilderAddNode(dockspaceId, ImGuiDockNodeFlags.None);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, size);

        // Split: left (Hierarchy) | rest
        uint leftId, centerId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.18f, &leftId, &centerId);

        // Split rest: center | right (Inspector)
        uint rightId, centerMainId;
        ImGuiP.DockBuilderSplitNode(centerId, ImGuiDir.Right, 0.25f, &rightId, &centerMainId);

        // Split center: top (Viewport) | bottom (Console)
        uint bottomId, topId;
        ImGuiP.DockBuilderSplitNode(centerMainId, ImGuiDir.Down, 0.25f, &bottomId, &topId);

        ImGuiP.DockBuilderDockWindow("Hierarchy", leftId);
        ImGuiP.DockBuilderDockWindow("Viewport", topId);
        ImGuiP.DockBuilderDockWindow("Inspector", rightId);
        ImGuiP.DockBuilderDockWindow("Console", bottomId);
        ImGuiP.DockBuilderDockWindow("Assets", bottomId);

        // Hide the tab bar on the Viewport node when it's the only window
        var node = ImGuiP.DockBuilderGetNode(topId);
        node.LocalFlags |= ImGuiDockNodeFlags.AutoHideTabBar;

        ImGuiP.DockBuilderFinish(dockspaceId);
    }
}
