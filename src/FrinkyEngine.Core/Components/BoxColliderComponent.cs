using System.Numerics;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Axis-aligned box collider shape in local space.
/// </summary>
public class BoxColliderComponent : ColliderComponent
{
    private Vector3 _size = Vector3.One;

    /// <summary>
    /// Full local dimensions of the box.
    /// </summary>
    public Vector3 Size
    {
        get => _size;
        set
        {
            var sanitized = new Vector3(
                MathF.Max(0.001f, value.X),
                MathF.Max(0.001f, value.Y),
                MathF.Max(0.001f, value.Z));

            if (_size == sanitized)
                return;

            _size = sanitized;
            MarkColliderDirty();
        }
    }
}

