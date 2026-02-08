using FrinkyEngine.Core.UI.Internal;
using HexaGen.Runtime;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Immediate-mode UI entry point for gameplay scripts.
/// </summary>
public static class UI
{
    private static readonly List<Action<UiContext>> DrawCommands = new();
    private static readonly UiContext Context = new();
    private static IUiBackend? _backend;
    private static bool _framePrepared;

    /// <summary>
    /// Gets whether the UI backend is currently initialized.
    /// </summary>
    public static bool IsAvailable => _backend != null;

    /// <summary>
    /// Gets input capture state from the most recently rendered UI frame.
    /// </summary>
    public static UiInputCapture InputCapture => _backend?.InputCapture ?? UiInputCapture.None;

    /// <summary>
    /// Initializes the UI system if it is not already initialized.
    /// </summary>
    public static void Initialize()
    {
        RegisterNativeLibrarySearchPaths();
        _backend ??= new ImGuiUiBackend();
    }

    /// <summary>
    /// Shuts down the UI system and releases backend resources.
    /// </summary>
    public static void Shutdown()
    {
        ClearFrame();
        _backend?.Dispose();
        _backend = null;
    }

    /// <summary>
    /// Queues UI draw commands for the current frame.
    /// </summary>
    /// <param name="draw">Draw callback that receives a wrapper <see cref="UiContext"/>.</param>
    public static void Draw(Action<UiContext> draw)
    {
        if (_backend == null || draw == null)
            return;

        DrawCommands.Add(draw);
    }

    /// <summary>
    /// Prepares frame metadata used to render queued UI commands.
    /// </summary>
    /// <param name="dt">Frame delta time in seconds.</param>
    /// <param name="frameDesc">Target viewport and input metadata.</param>
    public static void BeginFrame(float dt, in UiFrameDesc frameDesc)
    {
        if (_backend == null)
            return;

        _backend.PrepareFrame(dt, frameDesc);
        _framePrepared = true;
    }

    /// <summary>
    /// Renders queued UI commands and clears the frame queue.
    /// </summary>
    public static void EndFrame()
    {
        if (_backend == null)
            return;

        if (!_framePrepared)
        {
            var fallback = new UiFrameDesc(
                Raylib_cs.Raylib.GetScreenWidth(),
                Raylib_cs.Raylib.GetScreenHeight());
            _backend.PrepareFrame(1f / 60f, fallback);
        }

        _backend.RenderFrame(DrawCommands, Context);
        DrawCommands.Clear();
        _framePrepared = false;
    }

    /// <summary>
    /// Clears queued UI commands without rendering.
    /// </summary>
    public static void ClearFrame()
    {
        DrawCommands.Clear();
        _framePrepared = false;
        _backend?.ClearFrame();
    }

    private static void RegisterNativeLibrarySearchPaths()
    {
        // For single-file published apps, .NET extracts native libraries to a temp
        // directory. HexaGen.Runtime.LibraryLoader doesn't check there by default,
        // so we register those directories so it can find cimgui.dll etc.
        if (AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is not string searchDirs)
            return;

        foreach (var dir in searchDirs.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (Directory.Exists(trimmed) && !LibraryLoader.CustomLoadFolders.Contains(trimmed))
                LibraryLoader.CustomLoadFolders.Add(trimmed);
        }
    }
}

