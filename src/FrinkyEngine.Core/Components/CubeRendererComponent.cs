using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class CubeRendererComponent : Component
{
    public float Size { get; set; } = 1.0f;
    public Color Tint { get; set; } = new(255, 255, 255, 255);
}
