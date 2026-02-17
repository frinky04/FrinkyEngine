using System.Numerics;
using FrinkyEngine.Core.CanvasUI.Input;
using FrinkyEngine.Core.CanvasUI.Layout;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using FrinkyEngine.Core.CanvasUI.Styles.Css;

namespace FrinkyEngine.Core.CanvasUI;

public class RootPanel : Panel
{
    internal YogaLayoutEngine LayoutEngine { get; } = new();
    internal CanvasRenderer Renderer { get; } = new();
    internal InputManager InputManager { get; } = new();

    private readonly List<CssStyleRule> _styleRules = new();

    public void LoadStyleSheet(string css)
    {
        var rules = CssParser.Parse(css);
        _styleRules.AddRange(rules);
    }

    public void ClearStyleSheets()
    {
        _styleRules.Clear();
    }

    public Vector2 MousePosition => InputManager.MousePosition;

    public void Update(float dt, int screenWidth, int screenHeight, Vector2? mouseOverride = null)
    {
        // 0. Store mouse position early so Tick can use it (e.g. Slider drag)
        InputManager.MousePosition = mouseOverride ?? new Vector2(Raylib_cs.Raylib.GetMouseX(), Raylib_cs.Raylib.GetMouseY());

        // 1. Tick all panels
        TickRecursive(this, dt);

        // 2. Resolve styles (CSS cascade + inline → computed)
        ResolveStylesRecursive(this);

        // 3. Sync styles to Yoga and calculate layout
        LayoutEngine.Calculate(this, screenWidth, screenHeight);

        // 4. Read back layout results into Box rects
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

    private void ResolveStylesRecursive(Panel panel)
    {
        panel.ComputedStyle = StyleResolver.Resolve(panel, _styleRules);
        foreach (var child in panel.Children)
            ResolveStylesRecursive(child);
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

    public void Shutdown()
    {
        InputManager.Reset();
        DeleteChildren();
        Renderer.Shutdown();
    }
}
