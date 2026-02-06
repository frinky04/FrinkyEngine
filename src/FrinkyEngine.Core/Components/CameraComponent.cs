using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public enum ProjectionType
{
    Perspective,
    Orthographic
}

public class CameraComponent : Component
{
    public float FieldOfView { get; set; } = 60f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;
    public ProjectionType Projection { get; set; } = ProjectionType.Perspective;
    public Color ClearColor { get; set; } = new(30, 30, 30, 255);
    public bool IsMain { get; set; } = true;

    public Camera3D BuildCamera3D()
    {
        var pos = Entity.Transform.WorldPosition;
        var forward = Entity.Transform.Forward;

        return new Camera3D
        {
            Position = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z),
            Target = new System.Numerics.Vector3(pos.X + forward.X, pos.Y + forward.Y, pos.Z + forward.Z),
            Up = new System.Numerics.Vector3(0, 1, 0),
            FovY = FieldOfView,
            Projection = Projection == ProjectionType.Perspective
                ? CameraProjection.Perspective
                : CameraProjection.Orthographic
        };
    }
}
