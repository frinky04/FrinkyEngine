namespace FrinkyEngine.Core.CanvasUI;

[Flags]
public enum PseudoClassFlags
{
    None = 0,
    Hover = 1 << 0,
    Active = 1 << 1,
    Focus = 1 << 2,
    Disabled = 1 << 3,
    Intro = 1 << 4,
    Outro = 1 << 5,
}
