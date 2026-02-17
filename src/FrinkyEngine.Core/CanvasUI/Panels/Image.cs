using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Image : Panel
{
    private Texture2D? _texture;

    public Texture2D? Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            UpdateMeasureFunction();
        }
    }

    public Color Tint { get; set; } = new(255, 255, 255, 255);

    public override void OnCreated()
    {
        UpdateMeasureFunction();
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        if (_texture == null) return;
        DrawCommands.TexturedRect(_texture.Value, box.X, box.Y, box.Width, box.Height,
            CanvasRenderer.AlphaBlend(Tint, alpha));
    }

    private void UpdateMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            float intrinsicW = _texture?.Width ?? 0;
            float intrinsicH = _texture?.Height ?? 0;

            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(intrinsicW, width)
                    : intrinsicW;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(intrinsicH, height)
                    : intrinsicH;

            return MeasureOutput.Make(w, h);
        });
        InvalidateLayout();
    }
}
