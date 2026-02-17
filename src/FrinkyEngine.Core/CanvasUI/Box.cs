namespace FrinkyEngine.Core.CanvasUI;

public struct Box
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public Box(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public readonly float Right => X + Width;
    public readonly float Bottom => Y + Height;

    public readonly bool Contains(float px, float py)
        => px >= X && px <= Right && py >= Y && py <= Bottom;

    public static Box Intersect(Box a, Box b)
    {
        float x = MathF.Max(a.X, b.X);
        float y = MathF.Max(a.Y, b.Y);
        float right = MathF.Min(a.Right, b.Right);
        float bottom = MathF.Min(a.Bottom, b.Bottom);
        float w = MathF.Max(0, right - x);
        float h = MathF.Max(0, bottom - y);
        return new Box(x, y, w, h);
    }
}
