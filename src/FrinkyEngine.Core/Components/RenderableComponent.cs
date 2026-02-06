using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public abstract class RenderableComponent : Component
{
    public Color Tint { get; set; } = new(255, 255, 255, 255);

    internal Model? RenderModel { get; set; }

    internal abstract void EnsureModelReady();
}
