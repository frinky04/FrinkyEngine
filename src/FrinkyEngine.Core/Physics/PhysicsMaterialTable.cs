using BepuPhysics;
using BepuPhysics.Collidables;

namespace FrinkyEngine.Core.Physics;

internal sealed class PhysicsMaterialTable
{
    private readonly Dictionary<int, PhysicsMaterial> _bodyMaterials = new();
    private readonly Dictionary<int, PhysicsMaterial> _staticMaterials = new();

    public void Set(BodyHandle handle, PhysicsMaterial material)
    {
        _bodyMaterials[handle.Value] = material;
    }

    public void Set(StaticHandle handle, PhysicsMaterial material)
    {
        _staticMaterials[handle.Value] = material;
    }

    public void Remove(BodyHandle handle)
    {
        _bodyMaterials.Remove(handle.Value);
    }

    public void Remove(StaticHandle handle)
    {
        _staticMaterials.Remove(handle.Value);
    }

    public PhysicsMaterial Get(in CollidableReference collidable, float defaultFriction, float defaultRestitution)
    {
        switch (collidable.Mobility)
        {
            case CollidableMobility.Dynamic:
            case CollidableMobility.Kinematic:
                if (_bodyMaterials.TryGetValue(collidable.BodyHandle.Value, out var bodyMaterial))
                    return bodyMaterial;
                break;
            case CollidableMobility.Static:
                if (_staticMaterials.TryGetValue(collidable.StaticHandle.Value, out var staticMaterial))
                    return staticMaterial;
                break;
        }

        return new PhysicsMaterial(defaultFriction, defaultRestitution);
    }

    public void Clear()
    {
        _bodyMaterials.Clear();
        _staticMaterials.Clear();
    }
}

