using System.Numerics;
using FrinkyEngine.Core.CanvasUI.Input;
using FrinkyEngine.Core.CanvasUI.Layout;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI;

public class RootPanel : Panel
{
    internal YogaLayoutEngine LayoutEngine { get; } = new();
    internal CanvasRenderer Renderer { get; } = new();
    internal InputManager InputManager { get; } = new();

    public void Update(float dt, int screenWidth, int screenHeight)
    {
        UpdateCore(dt, screenWidth, screenHeight, null);
    }

    public void Update(float dt, int screenWidth, int screenHeight, Vector2 mousePosition)
    {
        UpdateCore(dt, screenWidth, screenHeight, mousePosition);
    }

    private void UpdateCore(float dt, int screenWidth, int screenHeight, Vector2? mouseOverride)
    {
        // 1. Tick all panels
        TickRecursive(this, dt);

        // 2. Resolve styles (inline → computed)
        ResolveStylesRecursive(this);

        // 3. Sync styles to Yoga and calculate layout
        LayoutEngine.Calculate(this, screenWidth, screenHeight);

        // 4. Read back layout results into Box rects
        ReadLayoutRecursive(this, 0, 0);

        // 5. Process input (must run after layout so hit testing uses current-frame boxes)
        InputManager.ProcessInput(this, mouseOverride);

        // 6. Render
        Renderer.Render(this, screenWidth, screenHeight);
    }

    private static void TickRecursive(Panel panel, float dt)
    {
        panel.Tick(dt);
        foreach (var child in panel.Children)
            TickRecursive(child, dt);
    }

    private static void ResolveStylesRecursive(Panel panel)
    {
        panel.ComputedStyle = StyleResolver.Resolve(panel);
        foreach (var child in panel.Children)
            ResolveStylesRecursive(child);
    }

    private static void ReadLayoutRecursive(Panel panel, float parentX, float parentY)
    {
        float x = parentX + panel.YogaNode.LayoutX;
        float y = parentY + panel.YogaNode.LayoutY;
        float w = panel.YogaNode.LayoutWidth;
        float h = panel.YogaNode.LayoutHeight;
        panel.Box = new Box(x, y, w, h);

        foreach (var child in panel.Children)
            ReadLayoutRecursive(child, x, y);
    }

    public void Shutdown()
    {
        DeleteChildren();
        Renderer.Shutdown();
    }
}
