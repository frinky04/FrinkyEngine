using Facebook.Yoga;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Label : Panel
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
        UpdateMeasureFunction();
    }

    private void UpdateMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            if (string.IsNullOrEmpty(_text))
                return MeasureOutput.Make(0, 0);

            float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;

            // Approximate text measurement: ~0.6 * fontSize per character width, fontSize for height
            float textWidth = _text.Length * fontSize * 0.6f;
            float textHeight = fontSize;

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
