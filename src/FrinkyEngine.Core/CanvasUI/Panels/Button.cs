using Facebook.Yoga;
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
            _text = value;
            UpdateMeasureFunction();
        }
    }

    public override void OnCreated()
    {
        AcceptsFocus = true;

        // Default button styling if not set by user
        Style.BackgroundColor ??= new Color(60, 60, 60, 255);
        Style.Padding ??= new Styles.Edges(6, 12, 6, 12);
        Style.BorderRadius ??= 4f;

        UpdateMeasureFunction();
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
