using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Renders a 3D model loaded from a file (e.g. .obj, .gltf, .glb).
/// Supports multiple material slots for per-material mapping and texture assignment.
/// </summary>
[ComponentCategory("Rendering")]
public class MeshRendererComponent : RenderableComponent
{
    private AssetReference _modelPath = new("");
    private int _lastMaterialHash;

    /// <summary>
    /// Asset-relative path to the model file. Changing this triggers a reload on the next frame.
    /// </summary>
    [AssetFilter(AssetType.Model)]
    public AssetReference ModelPath
    {
        get => _modelPath;
        set
        {
            if (_modelPath.Path == value.Path) return;
            _modelPath = value;
            RenderModel = null;
            _lastMaterialHash = 0;
        }
    }

    /// <summary>
    /// Per-material configurations for this model. Slots are auto-created to match the model's material count.
    /// </summary>
    [InspectorFixedListSize]
    public List<Material> MaterialSlots { get; set; } = new();

    internal override void EnsureModelReady()
    {
        if (_modelPath.IsEmpty) return;

        // Load model once
        if (!RenderModel.HasValue)
        {
            var model = AssetManager.Instance.LoadModel(_modelPath.Path);

            // Sync material slots to match model's material count (skip for error model)
            var resolvedPath = AssetDatabase.Instance.ResolveAssetPath(_modelPath.Path) ?? _modelPath.Path;
            if (File.Exists(AssetManager.Instance.ResolvePath(resolvedPath)))
            {
                while (MaterialSlots.Count < model.MaterialCount)
                    MaterialSlots.Add(new Material());
                while (MaterialSlots.Count > model.MaterialCount && model.MaterialCount > 0)
                    MaterialSlots.RemoveAt(MaterialSlots.Count - 1);
            }

            RenderModel = model;
        }

        // Re-apply materials when any slot changed (model is shared via cache)
        var currentHash = ComputeMaterialSlotsHash();
        if (currentHash != _lastMaterialHash && RenderModel.HasValue)
        {
            var model = RenderModel.Value;
            for (int i = 0; i < model.MaterialCount && i < MaterialSlots.Count; i++)
                MaterialApplicator.ApplyToModel(model, i, MaterialSlots[i]);
            _lastMaterialHash = currentHash;
        }
    }

    /// <summary>
    /// Forces the model and materials to be reloaded from disk on the next frame.
    /// </summary>
    public void RefreshMaterials()
    {
        RenderModel = null;
        _lastMaterialHash = 0;
    }

    private int ComputeMaterialSlotsHash()
    {
        var hash = new HashCode();
        foreach (var slot in MaterialSlots)
            hash.Add(slot.GetConfigurationHash());
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override void Start()
    {
        EnsureModelReady();
    }
}
