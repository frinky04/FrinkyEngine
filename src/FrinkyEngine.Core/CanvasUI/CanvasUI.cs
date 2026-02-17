using System.Numerics;

namespace FrinkyEngine.Core.CanvasUI;

public static class CanvasUI
{
    private static RootPanel? _rootPanel;

    public static bool IsAvailable => _rootPanel != null;

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

    public static void Update(float dt, int screenWidth, int screenHeight)
    {
        _rootPanel?.Update(dt, screenWidth, screenHeight);
    }

    /// <summary>
    /// Update with an explicit mouse position override (for editor viewport rendering).
    /// </summary>
    public static void Update(float dt, int screenWidth, int screenHeight, Vector2 mousePosition)
    {
        _rootPanel?.Update(dt, screenWidth, screenHeight, mousePosition);
    }

    /// <summary>
    /// Remove all child panels from the root, resetting the UI tree.
    /// Call between play sessions to prevent duplicate panels.
    /// </summary>
    public static void Reset()
    {
        _rootPanel?.DeleteChildren();
    }

    public static void Shutdown()
    {
        _rootPanel?.Shutdown();
        _rootPanel = null;
    }
}
