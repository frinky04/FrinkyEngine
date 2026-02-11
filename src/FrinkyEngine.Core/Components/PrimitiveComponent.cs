using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Abstract base class for procedurally generated mesh primitives (cubes, spheres, etc.).
/// Handles mesh generation, material assignment, and automatic rebuilds when properties change.
/// </summary>
public abstract class PrimitiveComponent : RenderableComponent
{
    private Material _material = new();
    private int _lastMaterialHash;
    private bool _meshDirty;

    /// <summary>
    /// Material configuration for this primitive.
    /// </summary>
    [InspectorOnChanged(nameof(MarkMeshDirty))]
    public Material Material
    {
        get => _material;
        set { _material = value ?? new Material(); MarkMeshDirty(); }
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
        var currentHash = _material.GetConfigurationHash();
        if (!RenderModel.HasValue || _meshDirty || currentHash != _lastMaterialHash)
            RebuildModel();
    }

    internal void RebuildModel()
    {
        if (RenderModel.HasValue)
            Raylib.UnloadModel(RenderModel.Value);

        var mesh = CreateMesh();
        var model = Raylib.LoadModelFromMesh(mesh);

        MaterialApplicator.ApplyToModel(model, 0, _material);

        RenderModel = model;
        _meshDirty = false;
        _lastMaterialHash = _material.GetConfigurationHash();
    }
}
