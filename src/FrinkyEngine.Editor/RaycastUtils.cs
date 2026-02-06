using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class RaycastUtils
{
    public static Ray GetViewportRay(Camera3D camera, Vector2 mousePos, Vector2 viewportSize)
    {
        float ndcX = 2f * mousePos.X / viewportSize.X - 1f;
        float ndcY = 1f - 2f * mousePos.Y / viewportSize.Y;

        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        float fovRad = camera.FovY * MathF.PI / 180f;
        float aspect = viewportSize.X / viewportSize.Y;
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspect, 0.1f, 1000f);

        var vp = view * proj;
        Matrix4x4.Invert(vp, out var vpInverse);

        var nearPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 0f, 1f), vpInverse);
        nearPoint /= nearPoint.W;

        var farPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 1f, 1f), vpInverse);
        farPoint /= farPoint.W;

        var origin = new Vector3(nearPoint.X, nearPoint.Y, nearPoint.Z);
        var direction = Vector3.Normalize(
            new Vector3(farPoint.X, farPoint.Y, farPoint.Z) - origin);

        return new Ray(origin, direction);
    }
}
