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
    private string _modelPath = string.Empty;

    /// <summary>
    /// Asset-relative path to the model file. Changing this triggers a reload on the next frame.
    /// </summary>
    public string ModelPath
    {
        get => _modelPath;
        set
        {
            if (_modelPath == value) return;
            _modelPath = value;
            RenderModel = null;
        }
    }

    /// <summary>
    /// Per-material configurations for this model. Slots are auto-created to match the model's material count.
    /// </summary>
    public List<MaterialSlot> MaterialSlots { get; set; } = new();

    internal override void EnsureModelReady()
    {
        if (RenderModel.HasValue || string.IsNullOrEmpty(_modelPath)) return;

        var model = AssetManager.Instance.LoadModel(_modelPath);

        // Extend material slots list to match model's material count
        while (MaterialSlots.Count < model.MaterialCount)
            MaterialSlots.Add(new MaterialSlot());

        // Apply material slots
        for (int i = 0; i < model.MaterialCount && i < MaterialSlots.Count; i++)
        {
            var slot = MaterialSlots[i];
            MaterialApplicator.ApplyToModel(
                model,
                i,
                slot.MaterialType,
                slot.TexturePath,
                slot.TriplanarScale,
                slot.TriplanarBlendSharpness,
                slot.TriplanarUseWorldSpace);
        }

        RenderModel = model;
    }

    /// <summary>
    /// Forces the model and materials to be reloaded from disk on the next frame.
    /// </summary>
    public void RefreshMaterials()
    {
        RenderModel = null;
    }

    /// <inheritdoc />
    public override void Start()
    {
        EnsureModelReady();
    }
}
