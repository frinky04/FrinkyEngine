using System.Numerics;

namespace FrinkyEngine.Core.CanvasUI.Events;

public class MouseEvent
{
    public Vector2 ScreenPos { get; init; }
    public Vector2 LocalPos { get; init; }
    public Raylib_cs.MouseButton Button { get; init; }
    public Panel? Target { get; init; }
    public bool Handled { get; set; }
}
