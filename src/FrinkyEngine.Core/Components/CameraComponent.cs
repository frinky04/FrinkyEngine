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
    private float _fieldOfView = 60f;
    private float _nearPlane = 0.1f;
    private float _farPlane = 1000f;

    /// <summary>
    /// Vertical field of view in degrees, used for perspective projection (defaults to 60).
    /// </summary>
    [InspectorLabel("Field of View")]
    [InspectorRange(1f, 179f, 0.5f)]
    [InspectorTooltip("Vertical field of view in degrees")]
    public float FieldOfView
    {
        get => _fieldOfView;
        set => _fieldOfView = float.IsFinite(value) ? Math.Clamp(value, 1f, 179f) : 60f;
    }

    /// <summary>
    /// Distance to the near clipping plane (defaults to 0.1).
    /// </summary>
    [InspectorLabel("Near Plane")]
    [InspectorRange(0.001f, 100f, 0.01f)]
    [InspectorTooltip("Distance to near clipping plane")]
    public float NearPlane
    {
        get => _nearPlane;
        set
        {
            _nearPlane = float.IsFinite(value) ? MathF.Max(value, 0.001f) : 0.1f;
            if (_farPlane <= _nearPlane)
                _farPlane = _nearPlane + 0.001f;
        }
    }

    /// <summary>
    /// Distance to the far clipping plane (defaults to 1000).
    /// </summary>
    [InspectorLabel("Far Plane")]
    [InspectorRange(1f, 10000f, 1f)]
    [InspectorTooltip("Distance to far clipping plane")]
    public float FarPlane
    {
        get => _farPlane;
        set
        {
            var safeValue = float.IsFinite(value) ? value : 1000f;
            _farPlane = MathF.Max(safeValue, _nearPlane + 0.001f);
        }
    }

    /// <summary>
    /// Whether this camera uses perspective or orthographic projection.
    /// </summary>
    public ProjectionType Projection { get; set; } = ProjectionType.Perspective;

    /// <summary>
    /// Background color used to clear the screen before rendering (defaults to dark gray).
    /// </summary>
    [InspectorLabel("Clear Color")]
    public Color ClearColor { get; set; } = new(30, 30, 30, 255);

    /// <summary>
    /// When <c>true</c>, marks this as the primary camera used for rendering.
    /// </summary>
    [InspectorLabel("Is Main")]
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
