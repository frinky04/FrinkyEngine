using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Capsule collider aligned to the entity local Y axis.
/// </summary>
[ComponentCategory("Physics/Colliders")]
public class CapsuleColliderComponent : ColliderComponent
{
    private float _radius = 0.5f;
    private float _length = 1.0f;

    /// <summary>
    /// Radius of the capsule hemispheres.
    /// </summary>
    public float Radius
    {
        get => _radius;
        set
        {
            var clamped = MathF.Max(0.001f, value);
            if (_radius == clamped)
                return;

            _radius = clamped;
            MarkColliderDirty();
        }
    }

    /// <summary>
    /// Length of the cylindrical section between hemispherical caps.
    /// </summary>
    public float Length
    {
        get => _length;
        set
        {
            var clamped = MathF.Max(0.001f, value);
            if (_length == clamped)
                return;

            _length = clamped;
            MarkColliderDirty();
        }
    }
}

