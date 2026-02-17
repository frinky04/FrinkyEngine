using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Events;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class Slider : Panel
{
    private float _value;
    private bool _dragging;

    public float Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0f, 1f);
    }

    public float Min { get; set; }
    public float Max { get; set; } = 1f;
    public float Step { get; set; } = 0.05f;

    public event Action<float>? OnChanged;

    public float MappedValue => Min + _value * (Max - Min);

    public override void OnCreated()
    {
        AcceptsFocus = true;

        OnMouseDown += e =>
        {
            _dragging = true;
            UpdateValueFromMouse(e.ScreenPos.X);
        };

        OnKeyDown += e =>
        {
            switch (e.Key)
            {
                case KeyboardKey.Left:
                    Value -= Step;
                    OnChanged?.Invoke(MappedValue);
                    e.Handled = true;
                    break;
                case KeyboardKey.Right:
                    Value += Step;
                    OnChanged?.Invoke(MappedValue);
                    e.Handled = true;
                    break;
            }
        };

        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            float minW = 100f;
            float minH = 20f;

            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(minW, width)
                    : minW;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(minH, height)
                    : minH;

            return MeasureOutput.Make(w, h);
        });
    }

    public override void Tick(float dt)
    {
        if (_dragging)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                UpdateValueFromMouse(CanvasUI.RootPanel.MousePosition.X);
            }
            else
            {
                _dragging = false;
            }
        }
    }

    private void UpdateValueFromMouse(float mouseX)
    {
        if (Box.Width <= 0) return;
        float relX = mouseX - Box.X;
        float newValue = Math.Clamp(relX / Box.Width, 0f, 1f);
        if (MathF.Abs(newValue - _value) > 0.001f)
        {
            Value = newValue;
            OnChanged?.Invoke(MappedValue);
        }
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        float trackHeight = 4f;
        float trackY = box.Y + (box.Height - trackHeight) * 0.5f;

        // Track
        var trackColor = new Color(80, 80, 80, 255);
        DrawCommands.RoundedRect(box.X, trackY, box.Width, trackHeight, 2f,
            CanvasRenderer.AlphaBlend(trackColor, alpha));

        // Filled portion
        float fillWidth = box.Width * _value;
        var fillColor = CanvasRenderer.AlphaBlend(style.Color, alpha);
        if (fillWidth > 0)
        {
            DrawCommands.RoundedRect(box.X, trackY, fillWidth, trackHeight, 2f, fillColor);
        }

        // Thumb
        float thumbRadius = box.Height * 0.4f;
        float thumbX = box.X + fillWidth;
        float thumbY = box.Y + box.Height * 0.5f;
        Raylib.DrawCircleV(new System.Numerics.Vector2(thumbX, thumbY), thumbRadius, fillColor);
    }
}
