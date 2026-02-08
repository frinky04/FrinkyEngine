using System.Numerics;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;

namespace PrefabTestingGround.Scripts;

[ComponentCategory("Fun")]
public class RotatorComponent : Component
{
    public float Speed { get; set; } = 45f;
    public Vector3 Axis { get; set; } = Vector3.UnitY;
    public EntityReference Test { get; set; }

    public override void Update(float dt)
    {
        var euler = Entity.Transform.EulerAngles;
        euler += Axis * Speed * dt;
        Entity.Transform.EulerAngles = euler;

        if (Test.IsValid) {
            FrinkyLog.Info($"Test: {Test.Resolve(Entity)?.Transform.WorldPosition}");
        }
    }
}