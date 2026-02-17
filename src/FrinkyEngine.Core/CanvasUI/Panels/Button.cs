using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Button : Panel
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateMeasureFunction();
        }
    }

    public override void OnCreated()
    {
        AcceptsFocus = true;
        Style.TextAlign = Styles.TextAlign.Center;
        UpdateMeasureFunction();
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

    private void UpdateMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;
            float textWidth = string.IsNullOrEmpty(_text) ? 0 : _text.Length * fontSize * 0.6f;
            float textHeight = fontSize;

            // Only report content size — Yoga adds padding separately via the style
            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(textWidth, width)
                    : textWidth;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(textHeight, height)
                    : textHeight;

            return MeasureOutput.Make(w, h);
        });
    }
}
