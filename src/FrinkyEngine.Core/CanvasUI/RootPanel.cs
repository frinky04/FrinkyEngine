using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.CanvasUI.Authoring;
using FrinkyEngine.Core.CanvasUI.Input;
using FrinkyEngine.Core.CanvasUI.Layout;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using FrinkyEngine.Core.CanvasUI.Styles.Css;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.CanvasUI;

public class RootPanel : Panel
{
    internal YogaLayoutEngine LayoutEngine { get; } = new();
    internal CanvasRenderer Renderer { get; } = new();
    internal InputManager InputManager { get; } = new();
    internal CanvasBindingManager BindingManager { get; } = new();
    private readonly CanvasHotReloadService _hotReloadService = new();

    private readonly List<CssStyleRule> _styleRules = new();
    private readonly List<string> _inlineStyleSheets = new();
    private readonly List<AssetStyleSheet> _assetStyleSheets = new();
    private string? _markupAssetPath;
    private object? _markupBindingContext;
    private bool _markupClearRoot = true;
    private bool _layoutDirty = true;
    private bool _stylesDirty = true;
    private int _lastLayoutWidth = -1;
    private int _lastLayoutHeight = -1;

    public void LoadStyleSheet(string css)
    {
        if (string.IsNullOrWhiteSpace(css))
            return;
        _inlineStyleSheets.Add(css);
        RebuildStyleRules();
    }

    public bool LoadStyleSheetFromAsset(string assetPath)
    {
        var fullPath = ResolveAssetFullPath(assetPath);
        if (fullPath == null)
            return false;

        if (!TryReadTextFile(fullPath, out string css))
            return false;

        int existing = _assetStyleSheets.FindIndex(x => string.Equals(x.FullPath, fullPath, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0)
            _assetStyleSheets[existing] = new AssetStyleSheet(fullPath, css);
        else
            _assetStyleSheets.Add(new AssetStyleSheet(fullPath, css));

        _hotReloadService.WatchFile(fullPath, _ => ReloadAssetStyleSheet(fullPath));
        RebuildStyleRules();
        return true;
    }

    public void ClearStyleSheets()
    {
        _inlineStyleSheets.Clear();
        _assetStyleSheets.Clear();
        _styleRules.Clear();
        MarkLayoutDirty();
    }

    public Vector2 MousePosition => InputManager.MousePosition;

    public void Update(float dt, int screenWidth, int screenHeight, Vector2? mouseOverride = null)
    {
        _hotReloadService.Update();

        // 0. Store mouse position early so Tick can use it (e.g. Slider drag)
        InputManager.MousePosition = mouseOverride ?? new Vector2(Raylib_cs.Raylib.GetMouseX(), Raylib_cs.Raylib.GetMouseY());

        // 1. Tick all panels
        TickRecursive(this, dt);

        // 1.5 Apply one-way bindings from active contexts
        BindingManager.Update();

        // 2. Resolve styles (CSS cascade + inline → computed)
        if (_stylesDirty)
        {
            bool styleChanged = ResolveStylesRecursive(this);
            if (styleChanged)
                _layoutDirty = true;
            _stylesDirty = false;
        }

        if (screenWidth != _lastLayoutWidth || screenHeight != _lastLayoutHeight)
            _layoutDirty = true;

        // 3. Sync styles to Yoga and calculate layout
        if (_layoutDirty)
        {
            LayoutEngine.Calculate(this, screenWidth, screenHeight);
            _layoutDirty = false;
            _lastLayoutWidth = screenWidth;
            _lastLayoutHeight = screenHeight;
        }

        // 4. Read back layout results into Box rects
        // Read each frame to account for scroll-offset changes without forcing Yoga layout.
        ReadLayoutRecursive(this, 0, 0);

        // 5. Process input (must run after layout so hit testing uses current-frame boxes)
        InputManager.ProcessInput(this);

        // 6. Render
        Renderer.Render(this, screenWidth, screenHeight);
    }

    private static void TickRecursive(Panel panel, float dt)
    {
        panel.Tick(dt);
        foreach (var child in panel.Children)
            TickRecursive(child, dt);
    }

    private bool ResolveStylesRecursive(Panel panel, ComputedStyle? parentStyle = null)
    {
        var resolved = StyleResolver.Resolve(panel, _styleRules, parentStyle);
        bool changed = !panel.ComputedStyle.Equals(resolved);
        panel.ComputedStyle = resolved;

        foreach (var child in panel.Children)
            changed |= ResolveStylesRecursive(child, panel.ComputedStyle);

        return changed;
    }

    private static void ReadLayoutRecursive(Panel panel, float parentX, float parentY, float parentScrollY = 0)
    {
        float x = parentX + panel.YogaNode.LayoutX;
        float y = parentY + panel.YogaNode.LayoutY - parentScrollY;
        float w = panel.YogaNode.LayoutWidth;
        float h = panel.YogaNode.LayoutHeight;
        panel.Box = new Box(x, y, w, h);

        foreach (var child in panel.Children)
            ReadLayoutRecursive(child, x, y, panel.ScrollOffsetY);
    }

    public void ResetInput()
    {
        InputManager.Reset();
    }

    public void EnableHotReload(bool enabled = true)
    {
        _hotReloadService.Enabled = enabled;
    }

    public Panel LoadMarkup(string markup, object? bindingContext = null, bool clearRoot = true)
    {
        if (clearRoot)
        {
            BindingManager.Clear();
            DeleteChildren();
        }

        if (bindingContext != null)
            SetBindingContext(bindingContext);

        var created = CanvasMarkupLoader.LoadIntoParent(this, this, markup);
        MarkLayoutDirty();
        BindingManager.NotifyBindingsChanged();
        return created;
    }

    public Panel? LoadMarkupFromAsset(string assetPath, object? bindingContext = null, bool clearRoot = true)
    {
        var fullPath = ResolveAssetFullPath(assetPath);
        if (fullPath == null)
            return null;

        if (!TryReadTextFile(fullPath, out string markup))
            return null;

        var panel = LoadMarkup(markup, bindingContext, clearRoot);
        _markupAssetPath = fullPath;
        _markupBindingContext = bindingContext;
        _markupClearRoot = clearRoot;
        _hotReloadService.WatchFile(fullPath, _ => ReloadMarkupAsset());
        return panel;
    }

    internal void MarkLayoutDirty()
    {
        _layoutDirty = true;
        _stylesDirty = true;
    }

    public void Shutdown()
    {
        InputManager.Reset();
        BindingManager.Clear();
        _hotReloadService.Clear();
        DeleteChildren();
        Renderer.Shutdown();
    }

    private void RebuildStyleRules()
    {
        _styleRules.Clear();

        foreach (var css in _inlineStyleSheets)
            TryAppendStyleRules(css);
        foreach (var entry in _assetStyleSheets)
            TryAppendStyleRules(entry.Css);

        StyleResolver.SortRules(_styleRules);
        MarkLayoutDirty();
    }

    private void TryAppendStyleRules(string css)
    {
        try
        {
            var rules = CssParser.Parse(css);
            _styleRules.AddRange(rules);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI stylesheet parse failed: {ex.Message}");
        }
    }

    private void ReloadAssetStyleSheet(string fullPath)
    {
        if (!TryReadTextFile(fullPath, out string css))
            return;

        int idx = _assetStyleSheets.FindIndex(x => string.Equals(x.FullPath, fullPath, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
            return;

        _assetStyleSheets[idx] = new AssetStyleSheet(fullPath, css);
        RebuildStyleRules();
        FrinkyLog.Info($"CanvasUI hot reloaded stylesheet: {fullPath}");
    }

    private void ReloadMarkupAsset()
    {
        if (string.IsNullOrEmpty(_markupAssetPath))
            return;
        if (!TryReadTextFile(_markupAssetPath, out string markup))
            return;

        try
        {
            LoadMarkup(markup, _markupBindingContext, _markupClearRoot);
            FrinkyLog.Info($"CanvasUI hot reloaded markup: {_markupAssetPath}");
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI markup hot reload failed: {ex.Message}");
        }
    }

    private static bool TryReadTextFile(string fullPath, out string content)
    {
        content = string.Empty;
        try
        {
            if (!File.Exists(fullPath))
                return false;
            content = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI file read failed: {fullPath} ({ex.Message})");
            return false;
        }
    }

    private static string? ResolveAssetFullPath(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
            return null;

        if (File.Exists(assetPath))
            return Path.GetFullPath(assetPath);

        var resolved = AssetDatabase.Instance.ResolveAssetPath(assetPath) ?? assetPath;
        var candidate = AssetManager.Instance.ResolvePath(resolved);
        if (File.Exists(candidate))
            return Path.GetFullPath(candidate);

        var baseCandidate = Path.Combine(AppContext.BaseDirectory, assetPath);
        if (File.Exists(baseCandidate))
            return Path.GetFullPath(baseCandidate);

        return null;
    }

    private readonly record struct AssetStyleSheet(string FullPath, string Css);
}
