using System.Numerics;

namespace FrinkyEngine.Core.CanvasUI;

public static class CanvasUI
{
    private static RootPanel? _rootPanel;

    public static RootPanel RootPanel
    {
        get
        {
            _rootPanel ??= new RootPanel();
            return _rootPanel;
        }
    }

    public static void Initialize()
    {
        _rootPanel ??= new RootPanel();
    }

    public static void Update(float dt, int screenWidth, int screenHeight, Vector2? mouseOverride = null)
    {
        _rootPanel?.Update(dt, screenWidth, screenHeight, mouseOverride);
    }

    public static void LoadStyleSheet(string css)
    {
        RootPanel.LoadStyleSheet(css);
    }

    public static void ClearStyleSheets()
    {
        RootPanel.ClearStyleSheets();
    }

    /// <summary>
    /// Remove all child panels from the root, resetting the UI tree.
    /// Call between play sessions to prevent duplicate panels.
    /// </summary>
    public static void Reset()
    {
        _rootPanel?.ResetInput();
        _rootPanel?.DeleteChildren();
    }

    public static void Shutdown()
    {
        _rootPanel?.Shutdown();
        _rootPanel = null;
    }
}
