using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace PhysicsStressTest.Scripts;

public class RotatorComponent : Component
{
    public float Speed { get; set; } = 45f;
    public Vector3 Axis { get; set; } = Vector3.UnitY;

    public override void Update(float dt)
    {
        var euler = Entity.Transform.EulerAngles;
        euler += Axis * Speed * dt;
        Entity.Transform.EulerAngles = euler;
    }
}