namespace FrinkyEngine.Core.CanvasUI.Events;

public class KeyboardEvent
{
    public Raylib_cs.KeyboardKey Key { get; init; }
    public int CharCode { get; init; }
    public Panel? Target { get; init; }
    public bool Handled { get; set; }
}
