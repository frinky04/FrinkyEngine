using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class MeshRendererComponent : Component
{
    private string _modelPath = string.Empty;

    public string ModelPath
    {
        get => _modelPath;
        set
        {
            if (_modelPath == value) return;
            _modelPath = value;
            LoadedModel = null;
        }
    }

    public Color Tint { get; set; } = new(255, 255, 255, 255);

    public Model? LoadedModel { get; internal set; }

    internal void EnsureModelLoaded()
    {
        if (LoadedModel.HasValue || string.IsNullOrEmpty(_modelPath)) return;
        LoadedModel = AssetManager.Instance.LoadModel(_modelPath);
    }

    public override void Start()
    {
        EnsureModelLoaded();
    }
}
