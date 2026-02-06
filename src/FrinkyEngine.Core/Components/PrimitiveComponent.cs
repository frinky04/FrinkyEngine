using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public abstract class PrimitiveComponent : RenderableComponent
{
    private MaterialType _materialType = MaterialType.SolidColor;
    private string _texturePath = string.Empty;
    private bool _meshDirty;

    public MaterialType MaterialType
    {
        get => _materialType;
        set
        {
            if (_materialType == value) return;
            _materialType = value;
            MarkMeshDirty();
        }
    }

    public string TexturePath
    {
        get => _texturePath;
        set
        {
            if (_texturePath == value) return;
            _texturePath = value;
            MarkMeshDirty();
        }
    }

    protected abstract Mesh CreateMesh();

    protected void MarkMeshDirty()
    {
        if (RenderModel.HasValue)
            RebuildModel();
        else
            _meshDirty = true;
    }

    public override void Start()
    {
        if (!RenderModel.HasValue || _meshDirty)
            RebuildModel();
    }

    public override void OnDestroy()
    {
        if (RenderModel.HasValue)
        {
            Raylib.UnloadModel(RenderModel.Value);
            RenderModel = null;
        }
    }

    internal override void EnsureModelReady()
    {
        if (!RenderModel.HasValue || _meshDirty)
            RebuildModel();
    }

    internal void RebuildModel()
    {
        if (RenderModel.HasValue)
            Raylib.UnloadModel(RenderModel.Value);

        var mesh = CreateMesh();
        var model = Raylib.LoadModelFromMesh(mesh);

        if (_materialType == MaterialType.Textured && !string.IsNullOrEmpty(_texturePath))
        {
            var texture = AssetManager.Instance.LoadTexture(_texturePath);
            unsafe
            {
                model.Materials[0].Maps[(int)MaterialMapIndex.Albedo].Texture = texture;
            }
        }

        RenderModel = model;
        _meshDirty = false;
    }
}
