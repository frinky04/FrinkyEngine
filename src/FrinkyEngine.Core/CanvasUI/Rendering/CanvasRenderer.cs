using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class CanvasRenderer
{
    private readonly ScissorStack _scissorStack = new();
    internal FontManager FontManager { get; } = new();

    internal static CanvasRenderer? Current { get; private set; }

    public void Render(RootPanel root, int screenWidth, int screenHeight)
    {
        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableDepthTest();

        Current = this;
        try
        {
            RenderPanel(root, screenWidth, screenHeight);
        }
        finally
        {
            Current = null;
        }

        _scissorStack.Clear();
        Rlgl.DrawRenderBatchActive();
    }

    private void RenderPanel(Panel panel, int screenWidth, int screenHeight)
    {
        if (panel.ComputedStyle.Display == Display.None) return;

        var box = panel.Box;
        var style = panel.ComputedStyle;
        float opacity = style.Opacity;
        if (opacity <= 0) return;

        byte alpha = (byte)(opacity * 255);

        // Background
        if (style.BackgroundColor.A > 0)
            DrawCommands.RoundedRect(box.X, box.Y, box.Width, box.Height, style.BorderRadius,
                AlphaBlend(style.BackgroundColor, alpha));

        // Border
        if (style.BorderWidth > 0 && style.BorderColor.A > 0)
            DrawCommands.RectBorder(box.X, box.Y, box.Width, box.Height, style.BorderRadius,
                style.BorderWidth, AlphaBlend(style.BorderColor, alpha));

        // Content rendering (virtual method on Panel subclasses)
        panel.RenderContent(box, style, alpha);

        // Scissor clipping for children with overflow hidden
        bool clip = style.Overflow == Overflow.Hidden;
        if (clip)
            _scissorStack.Push(box, screenHeight);

        // Recurse children
        foreach (var child in panel.Children)
            RenderPanel(child, screenWidth, screenHeight);

        if (clip)
            _scissorStack.Pop(screenHeight);
    }

    public static Color AlphaBlend(Color color, byte alpha)
        => new(color.R, color.G, color.B, (byte)((color.A * alpha) / 255));

    public void Shutdown()
    {
        FontManager.Shutdown();
    }
}
