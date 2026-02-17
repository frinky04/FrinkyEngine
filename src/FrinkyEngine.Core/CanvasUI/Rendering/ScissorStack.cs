using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class ScissorStack
{
    private readonly Stack<Box> _stack = new();

    public void Push(Box clipRect, int screenHeight)
    {
        if (_stack.Count > 0)
            clipRect = Box.Intersect(_stack.Peek(), clipRect);

        _stack.Push(clipRect);
        ApplyScissor(clipRect, screenHeight);
    }

    public void Pop(int screenHeight)
    {
        if (_stack.Count == 0) return;
        _stack.Pop();
        if (_stack.Count > 0)
            ApplyScissor(_stack.Peek(), screenHeight);
        else
            Rlgl.DisableScissorTest();
    }

    public void Clear()
    {
        _stack.Clear();
        Rlgl.DisableScissorTest();
    }

    private static void ApplyScissor(Box rect, int screenHeight)
    {
        Rlgl.EnableScissorTest();
        Rlgl.Scissor(
            (int)rect.X,
            (int)(screenHeight - (rect.Y + rect.Height)),
            (int)rect.Width,
            (int)rect.Height);
    }
}
