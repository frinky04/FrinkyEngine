namespace FrinkyEngine.Core.Components;

/// <summary>
/// Sphere collider shape in local space.
/// </summary>
public class SphereColliderComponent : ColliderComponent
{
    private float _radius = 0.5f;

    /// <summary>
    /// Radius of the sphere in local units.
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
}

