using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Checkbox : Panel
{
    private bool _checked;
    private string _text = string.Empty;

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            if (_checked)
                PseudoClasses |= PseudoClassFlags.Checked;
            else
                PseudoClasses &= ~PseudoClassFlags.Checked;
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateMeasureFunction();
        }
    }

    public event Action<bool>? OnChanged;

    public override void OnCreated()
    {
        AcceptsFocus = true;
        OnClick += _ =>
        {
            Checked = !Checked;
            OnChanged?.Invoke(Checked);
        };
        UpdateMeasureFunction();
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        var renderer = CanvasRenderer.Current;
        if (renderer == null) return;

        float fontSize = style.FontSize;
        float gap = fontSize * 0.4f;
        var color = CanvasRenderer.AlphaBlend(style.Color, alpha);

        float padL = style.Padding.Left.Unit == LengthUnit.Pixels ? style.Padding.Left.Value : 0;
        float padT = style.Padding.Top.Unit == LengthUnit.Pixels ? style.Padding.Top.Value : 0;

        float bx = box.X + padL;
        float by = box.Y + padT + (box.Height - padT * 2 - fontSize) * 0.5f;

        // Check box outline
        DrawCommands.RectBorder(bx, by, fontSize, fontSize, 3f, 2f, color);

        // Checkmark when checked
        if (_checked)
        {
            float inset = 3f;
            DrawCommands.RoundedRect(bx + inset, by + inset, fontSize - inset * 2, fontSize - inset * 2, 2f, color);
        }

        // Label text
        if (!string.IsNullOrEmpty(_text))
        {
            float tx = bx + fontSize + gap;
            float ty = by;
            DrawCommands.Text(_text, tx, ty, fontSize, color, renderer.FontManager.DefaultFont);
        }
    }

    private void UpdateMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;
            float gap = fontSize * 0.4f;
            float textWidth = string.IsNullOrEmpty(_text) ? 0 : _text.Length * fontSize * 0.6f;
            float totalWidth = fontSize + (textWidth > 0 ? gap + textWidth : 0);
            float totalHeight = fontSize;

            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(totalWidth, width)
                    : totalWidth;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(totalHeight, height)
                    : totalHeight;

            return MeasureOutput.Make(w, h);
        });
    }
}
