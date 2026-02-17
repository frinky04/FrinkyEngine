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

    /// <summary>
    /// Loads a CSS stylesheet from an asset path and applies it to CanvasUI.
    /// </summary>
    public static bool LoadStyleSheetFromAsset(string assetPath)
    {
        return RootPanel.LoadStyleSheetFromAsset(assetPath);
    }

    public static void ClearStyleSheets()
    {
        RootPanel.ClearStyleSheets();
    }

    /// <summary>
    /// Builds UI panels from markup text.
    /// </summary>
    public static Panel LoadMarkup(string markup, object? bindingContext = null, bool clearRoot = true)
    {
        return RootPanel.LoadMarkup(markup, bindingContext, clearRoot);
    }

    /// <summary>
    /// Builds UI panels from a markup asset path.
    /// </summary>
    public static Panel? LoadMarkupFromAsset(string assetPath, object? bindingContext = null, bool clearRoot = true)
    {
        return RootPanel.LoadMarkupFromAsset(assetPath, bindingContext, clearRoot);
    }

    /// <summary>
    /// Assigns the root binding context used by one-way markup bindings.
    /// </summary>
    public static void SetBindingContext(object? context)
    {
        RootPanel.SetBindingContext(context);
    }

    /// <summary>
    /// Enables or disables CanvasUI asset hot reload polling for loaded
    /// markup and stylesheet files.
    /// </summary>
    public static void EnableHotReload(bool enabled = true)
    {
        RootPanel.EnableHotReload(enabled);
    }

    /// <summary>
    /// Remove all child panels from the root, resetting the UI tree.
    /// Call between play sessions to prevent duplicate panels.
    /// </summary>
    public static void Reset()
    {
        _rootPanel?.ResetInput();
        _rootPanel?.ClearStyleSheets();
        _rootPanel?.BindingManager.Clear();
        _rootPanel?.DeleteChildren();
    }

    public static void RegisterFont(string name, string path)
    {
        RootPanel.Renderer.FontManager.RegisterFont(name, path);
    }

    public static void Shutdown()
    {
        _rootPanel?.Shutdown();
        _rootPanel = null;
    }
}
