using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Events;

public class KeyboardEvent
{
    public KeyboardKey Key { get; init; }
    public char Character { get; init; }
    public Panel? Target { get; init; }
    public bool Handled { get; set; }
}
