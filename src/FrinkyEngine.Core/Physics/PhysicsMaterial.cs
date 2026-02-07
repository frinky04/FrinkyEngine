namespace FrinkyEngine.Core.Physics;

internal readonly struct PhysicsMaterial
{
    public readonly float Friction;
    public readonly float Restitution;

    public PhysicsMaterial(float friction, float restitution)
    {
        Friction = friction;
        Restitution = restitution;
    }
}

