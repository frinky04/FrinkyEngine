using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public abstract class PrimitiveComponent : Component
{
    private MaterialType _materialType = MaterialType.SolidColor;
    private Color _tint = new(255, 255, 255, 255);
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

    public Color Tint
    {
        get => _tint;
        set => _tint = value;
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

    internal Model? GeneratedModel { get; private set; }

    protected abstract Mesh CreateMesh();

    protected void MarkMeshDirty()
    {
        if (GeneratedModel.HasValue)
            RebuildModel();
        else
            _meshDirty = true;
    }

    public override void Start()
    {
        if (!GeneratedModel.HasValue || _meshDirty)
            RebuildModel();
    }

    public override void OnDestroy()
    {
        if (GeneratedModel.HasValue)
        {
            Raylib.UnloadModel(GeneratedModel.Value);
            GeneratedModel = null;
        }
    }

    internal void RebuildModel()
    {
        if (GeneratedModel.HasValue)
            Raylib.UnloadModel(GeneratedModel.Value);

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

        GeneratedModel = model;
        _meshDirty = false;
    }
}
