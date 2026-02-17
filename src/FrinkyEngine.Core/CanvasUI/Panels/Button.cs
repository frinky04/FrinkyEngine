using System.Numerics;
using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Events;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Button : Panel
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            InvalidateLayout();
        }
    }

    public override void OnCreated()
    {
        AcceptsFocus = true;
        Style.TextAlign = Styles.TextAlign.Center;
        OnKeyDown += HandleKeyDown;
        SetMeasureFunction();
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        if (string.IsNullOrEmpty(_text)) return;

        var renderer = CanvasRenderer.Current;
        if (renderer == null) return;

        float padL = YogaNode.LayoutPaddingLeft;
        float padR = YogaNode.LayoutPaddingRight;
        float padT = YogaNode.LayoutPaddingTop;
        float padB = YogaNode.LayoutPaddingBottom;

        float contentX = box.X + padL;
        float contentW = box.Width - padL - padR;
        float contentY = box.Y + padT;
        float contentH = box.Height - padT - padB;

        var font = renderer.FontManager.GetFont(style.FontFamily);
        var textSize = DrawCommands.MeasureText(_text, style.FontSize, font);

        float tx = style.TextAlign switch
        {
            Styles.TextAlign.Center => contentX + (contentW - textSize.X) * 0.5f,
            Styles.TextAlign.Right => contentX + contentW - textSize.X,
            _ => contentX,
        };
        float ty = contentY + (contentH - textSize.Y) * 0.5f;

        DrawCommands.Text(_text, tx, ty, style.FontSize,
            CanvasRenderer.AlphaBlend(style.Color, alpha), font);
    }

    private void SetMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            var (fontSize, font) = GetMeasureFont();
            float textWidth = 0f;
            float textHeight = fontSize;
            if (!string.IsNullOrEmpty(_text))
            {
                var textSize = DrawCommands.MeasureText(_text, fontSize, font);
                textWidth = textSize.X;
                textHeight = textSize.Y > 0 ? textSize.Y : fontSize;
            }
            return ResolveMeasure(textWidth, textHeight, width, widthMode, height, heightMode);
        });
        InvalidateLayout();
    }

    private void HandleKeyDown(KeyboardEvent e)
    {
        if (e.Key is not (KeyboardKey.Enter or KeyboardKey.Space))
            return;

        e.Handled = true;
        RaiseClick(new MouseEvent
        {
            ScreenPos = new Vector2(Box.X, Box.Y),
            LocalPos = Vector2.Zero,
            Button = MouseButton.Left,
            Target = this
        });
    }
}
