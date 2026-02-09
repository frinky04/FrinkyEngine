using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Abstract base class for procedurally generated mesh primitives (cubes, spheres, etc.).
/// Handles mesh generation, material assignment, and automatic rebuilds when properties change.
/// </summary>
public abstract class PrimitiveComponent : RenderableComponent
{
    private MaterialType _materialType = MaterialType.SolidColor;
    private AssetReference _texturePath = new("");
    private float _triplanarScale = 1f;
    private float _triplanarBlendSharpness = 4f;
    private bool _triplanarUseWorldSpace = true;
    private bool _meshDirty;

    /// <summary>
    /// Which material mapping mode the primitive uses (defaults to <see cref="Rendering.MaterialType.SolidColor"/>).
    /// </summary>
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

    /// <summary>
    /// Asset-relative path to the texture file, used when <see cref="MaterialType"/> is
    /// <see cref="Rendering.MaterialType.Textured"/> or <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    [AssetFilter(AssetType.Texture)]
    public AssetReference TexturePath
    {
        get => _texturePath;
        set
        {
            if (_texturePath.Path == value.Path) return;
            _texturePath = value;
            MarkMeshDirty();
        }
    }

    /// <summary>
    /// Triplanar projection scale, used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    public float TriplanarScale
    {
        get => _triplanarScale;
        set
        {
            if (_triplanarScale == value) return;
            _triplanarScale = value;
            MarkMeshDirty();
        }
    }

    /// <summary>
    /// Triplanar axis blend sharpness, used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    public float TriplanarBlendSharpness
    {
        get => _triplanarBlendSharpness;
        set
        {
            if (_triplanarBlendSharpness == value) return;
            _triplanarBlendSharpness = value;
            MarkMeshDirty();
        }
    }

    /// <summary>
    /// Whether triplanar projection uses world-space coordinates (<c>true</c>) or object-space coordinates (<c>false</c>).
    /// Used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    public bool TriplanarUseWorldSpace
    {
        get => _triplanarUseWorldSpace;
        set
        {
            if (_triplanarUseWorldSpace == value) return;
            _triplanarUseWorldSpace = value;
            MarkMeshDirty();
        }
    }

    /// <inheritdoc />
    public override void Invalidate()
    {
        if (RenderModel.HasValue)
            Raylib.UnloadModel(RenderModel.Value);
        RenderModel = null;
        _meshDirty = true;
    }

    /// <summary>
    /// Creates the procedural mesh for this primitive. Subclasses implement this to define their geometry.
    /// </summary>
    /// <returns>The generated <see cref="Mesh"/>.</returns>
    protected abstract Mesh CreateMesh();

    /// <summary>
    /// Flags the mesh as needing a rebuild, triggering regeneration on the next frame.
    /// </summary>
    protected void MarkMeshDirty()
    {
        if (RenderModel.HasValue)
            RebuildModel();
        else
            _meshDirty = true;
    }

    /// <inheritdoc />
    public override void Start()
    {
        if (!RenderModel.HasValue || _meshDirty)
            RebuildModel();
    }

    /// <inheritdoc />
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

        MaterialApplicator.ApplyToModel(
            model,
            0,
            _materialType,
            _texturePath.Path,
            _triplanarScale,
            _triplanarBlendSharpness,
            _triplanarUseWorldSpace);

        RenderModel = model;
        _meshDirty = false;
    }
}
