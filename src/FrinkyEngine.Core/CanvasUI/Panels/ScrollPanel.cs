using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class ScrollPanel : Panel
{
    private float _scrollSpeed = 30f;

    public float ScrollSpeed
    {
        get => _scrollSpeed;
        set => _scrollSpeed = value;
    }

    public override void OnCreated()
    {
        Style.Overflow = Overflow.Hidden;

        OnMouseWheel += delta =>
        {
            float contentHeight = GetContentHeight();
            float viewHeight = Box.Height;
            float maxScroll = MathF.Max(0, contentHeight - viewHeight);

            ScrollOffsetY -= delta.Y * _scrollSpeed;
            ScrollOffsetY = Math.Clamp(ScrollOffsetY, 0f, maxScroll);
        };
    }

    private float GetContentHeight()
    {
        float max = 0;
        foreach (var child in Children)
        {
            float childBottom = child.YogaNode.LayoutY + child.YogaNode.LayoutHeight;
            if (childBottom > max) max = childBottom;
        }
        return max;
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        // Optional: draw a scrollbar indicator
        float contentHeight = GetContentHeight();
        float viewHeight = box.Height;
        if (contentHeight <= viewHeight) return;

        float scrollRatio = viewHeight / contentHeight;
        float barHeight = MathF.Max(viewHeight * scrollRatio, 10f);
        float maxScroll = contentHeight - viewHeight;
        float barY = box.Y + (ScrollOffsetY / maxScroll) * (viewHeight - barHeight);
        float barX = box.X + box.Width - 4f;

        var barColor = new Color(255, 255, 255, 40);
        DrawCommands.RoundedRect(barX, barY, 3f, barHeight, 1.5f,
            CanvasRenderer.AlphaBlend(barColor, alpha));
    }
}
