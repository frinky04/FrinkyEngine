using FrinkyEngine.Core.CanvasUI.Panels;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class CanvasRenderer
{
    private readonly ScissorStack _scissorStack = new();
    internal FontManager FontManager { get; } = new();

    public void Render(RootPanel root, int screenWidth, int screenHeight)
    {
        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableDepthTest();

        RenderPanel(root, screenWidth, screenHeight);

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
        var bgColor = style.BackgroundColor;
        if (bgColor.A > 0)
        {
            bgColor = new Color(bgColor.R, bgColor.G, bgColor.B, (byte)((bgColor.A * alpha) / 255));
            DrawCommands.RoundedRect(box.X, box.Y, box.Width, box.Height, style.BorderRadius, bgColor);
        }

        // Border
        if (style.BorderWidth > 0 && style.BorderColor.A > 0)
        {
            var borderColor = style.BorderColor;
            borderColor = new Color(borderColor.R, borderColor.G, borderColor.B, (byte)((borderColor.A * alpha) / 255));
            DrawCommands.RectBorder(box.X, box.Y, box.Width, box.Height, style.BorderRadius, style.BorderWidth, borderColor);
        }

        // Content rendering (overridden by Label, Button, etc.)
        RenderContent(panel, box, style, alpha);

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

    private void RenderContent(Panel panel, Box box, ComputedStyle style, byte alpha)
    {
        if (panel is Label label && !string.IsNullOrEmpty(label.Text))
        {
            var font = FontManager.DefaultFont;
            var textColor = style.Color;
            textColor = new Color(textColor.R, textColor.G, textColor.B, (byte)((textColor.A * alpha) / 255));
            float padL = style.Padding.Left.Unit == LengthUnit.Pixels ? style.Padding.Left.Value : 0;
            float padT = style.Padding.Top.Unit == LengthUnit.Pixels ? style.Padding.Top.Value : 0;
            DrawCommands.Text(label.Text, box.X + padL, box.Y + padT, style.FontSize, textColor, font);
        }
        else if (panel is Button button && !string.IsNullOrEmpty(button.Text))
        {
            var font = FontManager.DefaultFont;
            var textColor = style.Color;
            textColor = new Color(textColor.R, textColor.G, textColor.B, (byte)((textColor.A * alpha) / 255));
            var textSize = DrawCommands.MeasureText(button.Text, style.FontSize, font);
            float tx = box.X + (box.Width - textSize.X) * 0.5f;
            float ty = box.Y + (box.Height - textSize.Y) * 0.5f;
            DrawCommands.Text(button.Text, tx, ty, style.FontSize, textColor, font);
        }
    }

    public void Shutdown()
    {
        FontManager.Shutdown();
    }
}
