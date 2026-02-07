using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Determines the camera projection mode.
/// </summary>
public enum ProjectionType
{
    /// <summary>
    /// Standard perspective projection with foreshortening.
    /// </summary>
    Perspective,

    /// <summary>
    /// Orthographic projection with no foreshortening.
    /// </summary>
    Orthographic
}

/// <summary>
/// Provides a camera viewpoint that the renderer uses to draw the scene.
/// Attach to an entity to position the camera via the entity's <see cref="TransformComponent"/>.
/// </summary>
[ComponentCategory("Rendering")]
public class CameraComponent : Component
{
    /// <summary>
    /// Vertical field of view in degrees, used for perspective projection (defaults to 60).
    /// </summary>
    public float FieldOfView { get; set; } = 60f;

    /// <summary>
    /// Distance to the near clipping plane (defaults to 0.1).
    /// </summary>
    public float NearPlane { get; set; } = 0.1f;

    /// <summary>
    /// Distance to the far clipping plane (defaults to 1000).
    /// </summary>
    public float FarPlane { get; set; } = 1000f;

    /// <summary>
    /// Whether this camera uses perspective or orthographic projection.
    /// </summary>
    public ProjectionType Projection { get; set; } = ProjectionType.Perspective;

    /// <summary>
    /// Background color used to clear the screen before rendering (defaults to dark gray).
    /// </summary>
    public Color ClearColor { get; set; } = new(30, 30, 30, 255);

    /// <summary>
    /// When <c>true</c>, marks this as the primary camera used for rendering.
    /// </summary>
    public bool IsMain { get; set; } = true;

    /// <summary>
    /// Builds a Raylib <see cref="Camera3D"/> from this component's settings and the entity's transform.
    /// </summary>
    /// <returns>A configured <see cref="Camera3D"/> ready for rendering.</returns>
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
