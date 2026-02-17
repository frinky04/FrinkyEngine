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
            InvalidateLayout();
        }
    }

    public Color Tint { get; set; } = new(255, 255, 255, 255);

    public override void OnCreated()
    {
        SetMeasureFunction();
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        if (_texture == null) return;
        DrawCommands.TexturedRect(_texture.Value, box.X, box.Y, box.Width, box.Height,
            CanvasRenderer.AlphaBlend(Tint, alpha));
    }

    private void SetMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
            ResolveMeasure(_texture?.Width ?? 0, _texture?.Height ?? 0,
                width, widthMode, height, heightMode));
        InvalidateLayout();
    }
}
