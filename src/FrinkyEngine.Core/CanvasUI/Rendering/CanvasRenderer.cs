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
            RenderPanel(root, screenWidth, screenHeight, 255);
        }
        finally
        {
            Current = null;
        }

        _scissorStack.Clear();
        Rlgl.DrawRenderBatchActive();
    }

    private void RenderPanel(Panel panel, int screenWidth, int screenHeight, byte inheritedAlpha)
    {
        if (panel.ComputedStyle.Display == Display.None) return;
        if (inheritedAlpha == 0) return;

        var box = panel.Box;
        var style = panel.ComputedStyle;
        float opacity = Math.Clamp(style.Opacity, 0f, 1f);
        if (opacity <= 0) return;

        byte localAlpha = (byte)(opacity * 255);
        byte alpha = (byte)((inheritedAlpha * localAlpha) / 255);
        if (alpha == 0) return;

        // Background + Border
        float bw = style.BorderWidth;
        bool hasBorder = bw > 0 && style.BorderColor.A > 0;
        bool hasBg = style.BackgroundColor.A > 0;

        if (hasBorder && hasBg)
        {
            // Draw border as filled rounded rect, then background inset — avoids aliased line drawing
            DrawCommands.RoundedRect(box.X, box.Y, box.Width, box.Height, style.BorderRadius,
                AlphaBlend(style.BorderColor, alpha));
            float innerRadius = MathF.Max(0, style.BorderRadius - bw);
            DrawCommands.RoundedRect(box.X + bw, box.Y + bw, box.Width - bw * 2, box.Height - bw * 2, innerRadius,
                AlphaBlend(style.BackgroundColor, alpha));
        }
        else if (hasBg)
        {
            DrawCommands.RoundedRect(box.X, box.Y, box.Width, box.Height, style.BorderRadius,
                AlphaBlend(style.BackgroundColor, alpha));
        }
        else if (hasBorder)
        {
            // No background — fall back to line-based border
            DrawCommands.RectBorder(box.X, box.Y, box.Width, box.Height, style.BorderRadius,
                bw, AlphaBlend(style.BorderColor, alpha));
        }

        // Content rendering (virtual method on Panel subclasses)
        panel.RenderContent(box, style, alpha);

        // Scissor clipping for children with overflow hidden
        bool clip = style.Overflow == Overflow.Hidden;
        if (clip)
            _scissorStack.Push(box, screenHeight);

        // Recurse children
        foreach (var child in panel.Children)
            RenderPanel(child, screenWidth, screenHeight, alpha);

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
