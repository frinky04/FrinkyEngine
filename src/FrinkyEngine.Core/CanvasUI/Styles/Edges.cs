namespace FrinkyEngine.Core.CanvasUI.Styles;

public struct Edges
{
    public Length Top;
    public Length Right;
    public Length Bottom;
    public Length Left;

    public Edges(float all)
    {
        Top = Right = Bottom = Left = Length.Px(all);
    }

    public Edges(float vertical, float horizontal)
    {
        Top = Bottom = Length.Px(vertical);
        Left = Right = Length.Px(horizontal);
    }

    public Edges(float top, float right, float bottom, float left)
    {
        Top = Length.Px(top);
        Right = Length.Px(right);
        Bottom = Length.Px(bottom);
        Left = Length.Px(left);
    }

    public Edges(Length top, Length right, Length bottom, Length left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }
}
