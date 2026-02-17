using System.Numerics;

namespace FrinkyEngine.Core.CanvasUI.Events;

public class MouseWheelEvent
{
    public Vector2 Delta { get; init; }
    public Panel? Target { get; init; }
    public bool Handled { get; set; }
}
