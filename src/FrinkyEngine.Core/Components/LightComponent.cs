using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public enum LightType
{
    Directional = 0,
    Point = 1,
    Skylight = 2
}

public class LightComponent : Component
{
    public LightType LightType { get; set; } = LightType.Directional;
    public Color LightColor { get; set; } = new(255, 255, 255, 255);
    public float Intensity { get; set; } = 1.0f;
    public float Range { get; set; } = 10.0f;
}
