using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Label : Panel
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

        float contentX = box.X + padL;
        float contentW = box.Width - padL - padR;

        var font = renderer.FontManager.GetFont(style.FontFamily);
        var textSize = DrawCommands.MeasureText(_text, style.FontSize, font);

        float tx = style.TextAlign switch
        {
            Styles.TextAlign.Center => contentX + (contentW - textSize.X) * 0.5f,
            Styles.TextAlign.Right => contentX + contentW - textSize.X,
            _ => contentX,
        };

        DrawCommands.Text(_text, tx, box.Y + padT, style.FontSize,
            CanvasRenderer.AlphaBlend(style.Color, alpha), font);
    }

    private void SetMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            if (string.IsNullOrEmpty(_text))
                return MeasureOutput.Make(0, 0);

            float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;
            var root = GetRootPanel();
            if (root == null) return MeasureOutput.Make(0, 0);
            var font = root.Renderer.FontManager.GetFont(ComputedStyle.FontFamily);
            var textSize = DrawCommands.MeasureText(_text, fontSize, font);

            float textWidth = textSize.X;
            float textHeight = textSize.Y > 0 ? textSize.Y : fontSize;

            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(textWidth, width)
                    : textWidth;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(textHeight, height)
                    : textHeight;

            return MeasureOutput.Make(w, h);
        });
        InvalidateLayout();
    }
}
