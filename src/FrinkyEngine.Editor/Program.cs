using System.Numerics;
using FrinkyEngine.Core.Rendering;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

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

        rlImGui.Setup(true, true);

        // Load JetBrains Mono font
        var io = ImGui.GetIO();
        unsafe
        {
            var fontPath = "EditorAssets/Fonts/JetBrains_Mono/static/JetBrainsMono-Regular.ttf";
            if (File.Exists(fontPath))
            {
                io.Fonts.AddFontFromFileTTF(fontPath, 16.0f);
                rlImGui.ReloadFonts();
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

            rlImGui.Begin(dt);

            DrawDockspace(app);

            app.DrawUI();

            rlImGui.End();

            // Clear suppression flags and update tracking for next frame
            io.ConfigFlags &= ~(ImGuiConfigFlags.NoMouse | ImGuiConfigFlags.NoMouseCursorChange);
            _cursorWasLockedLastFrame = app.IsCursorLocked;

            Raylib.EndDrawing();
        }

        app.Shutdown();
        rlImGui.Shutdown();
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

    private static void BuildDefaultLayout(uint dockspaceId, Vector2 size)
    {
        ImGuiDockBuilder.RemoveNode(dockspaceId);
        ImGuiDockBuilder.AddNode(dockspaceId);
        ImGuiDockBuilder.SetNodeSize(dockspaceId, size);

        // Split: left (Hierarchy) | rest
        ImGuiDockBuilder.SplitNode(dockspaceId, ImGuiDir.Left, 0.18f,
            out uint leftId, out uint centerId);

        // Split rest: center | right (Inspector)
        ImGuiDockBuilder.SplitNode(centerId, ImGuiDir.Right, 0.25f,
            out uint rightId, out uint centerMainId);

        // Split center: top (Viewport) | bottom (Console)
        ImGuiDockBuilder.SplitNode(centerMainId, ImGuiDir.Down, 0.25f,
            out uint bottomId, out uint topId);

        ImGuiDockBuilder.DockWindow("Hierarchy", leftId);
        ImGuiDockBuilder.DockWindow("Viewport", topId);
        ImGuiDockBuilder.DockWindow("Inspector", rightId);
        ImGuiDockBuilder.DockWindow("Console", bottomId);
        ImGuiDockBuilder.DockWindow("Assets", bottomId);

        // Hide the tab bar on the Viewport node when it's the only window
        ImGuiDockBuilder.SetNodeLocalFlags(topId, ImGuiDockNodeFlags.AutoHideTabBar);

        ImGuiDockBuilder.Finish(dockspaceId);
    }
}
