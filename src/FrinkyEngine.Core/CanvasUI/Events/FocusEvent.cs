namespace FrinkyEngine.Core.CanvasUI.Events;

public class FocusEvent
{
    public Panel? Target { get; init; }
    public Panel? RelatedTarget { get; init; }
}
