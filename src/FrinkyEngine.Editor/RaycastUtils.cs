using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class RaycastUtils
{
    public static Ray GetViewportRay(Camera3D camera, Vector2 mousePos, Vector2 viewportSize)
    {
        int width = Math.Max(1, (int)viewportSize.X);
        int height = Math.Max(1, (int)viewportSize.Y);
        return Raylib.GetScreenToWorldRayEx(mousePos, camera, width, height);
    }
}
