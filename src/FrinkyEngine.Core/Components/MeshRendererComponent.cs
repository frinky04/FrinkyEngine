using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class MeshRendererComponent : Component
{
    public string ModelPath { get; set; } = string.Empty;
    public string MaterialPath { get; set; } = string.Empty;
    public Color Tint { get; set; } = new(255, 255, 255, 255);

    public Model? LoadedModel { get; internal set; }

    public override void Start()
    {
        if (!string.IsNullOrEmpty(ModelPath))
        {
            LoadedModel = AssetManager.Instance.LoadModel(ModelPath);
        }
    }
}
