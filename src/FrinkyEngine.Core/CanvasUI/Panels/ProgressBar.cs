using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class ProgressBar : Panel
{
    private float _value;

    public float Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0f, 1f);
    }

    public Color? TrackColor { get; set; }
    public Color? FillColor { get; set; }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        var trackColor = TrackColor ?? new Color(60, 60, 60, 255);
        var fillColor = FillColor ?? new Color(74, 222, 128, 255);

        float radius = style.BorderRadius;

        // Track (full width)
        DrawCommands.RoundedRect(box.X, box.Y, box.Width, box.Height, radius,
            CanvasRenderer.AlphaBlend(trackColor, alpha));

        // Fill
        float fillWidth = box.Width * _value;
        if (fillWidth > 0)
        {
            float fillRadius = MathF.Min(radius, MathF.Min(fillWidth, box.Height) * 0.5f);
            DrawCommands.RoundedRect(box.X, box.Y, fillWidth, box.Height, fillRadius,
                CanvasRenderer.AlphaBlend(fillColor, alpha));
        }
    }
}
