using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Renders a 3D model loaded from a file (e.g. .obj, .gltf, .glb).
/// Supports multiple material slots for per-material mapping and texture assignment.
/// </summary>
[ComponentCategory("Rendering")]
public class MeshRendererComponent : RenderableComponent
{
    private AssetReference _modelPath = new("");
    private bool _requireUniqueModelInstance;
    private bool _ownsUniqueModelInstance;

    /// <summary>
    /// Monotonically increasing counter incremented each time the model is invalidated.
    /// Used by sibling components to detect same-path reloads.
    /// </summary>
    internal int ModelVersion { get; private set; }

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
            Invalidate();
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
            var resolvedPath = AssetDatabase.Instance.ResolveAssetPath(_modelPath.Path) ?? _modelPath.Path;
            bool fileExists = File.Exists(AssetManager.Instance.ResolvePath(resolvedPath));

            var model = (_requireUniqueModelInstance && fileExists)
                ? AssetManager.Instance.LoadModelUnique(_modelPath.Path)
                : AssetManager.Instance.LoadModel(_modelPath.Path);
            _ownsUniqueModelInstance = _requireUniqueModelInstance && fileExists;

            // Sync material slots to match model's material count (skip for error model)
            if (fileExists)
            {
                while (MaterialSlots.Count < model.MaterialCount)
                    MaterialSlots.Add(new Material());
                while (MaterialSlots.Count > model.MaterialCount && model.MaterialCount > 0)
                    MaterialSlots.RemoveAt(MaterialSlots.Count - 1);
            }

            RenderModel = model;
        }

        // Model assets are shared via AssetManager cache, so material state on the underlying
        // Raylib model must be reapplied per renderer to avoid cross-instance bleed-through.
        if (RenderModel.HasValue)
        {
            var model = RenderModel.Value;
            for (int i = 0; i < model.MaterialCount && i < MaterialSlots.Count; i++)
                MaterialApplicator.ApplyToModel(model, i, MaterialSlots[i]);
        }
    }

    /// <summary>
    /// Forces the model and materials to be reloaded from disk on the next frame.
    /// </summary>
    public void RefreshMaterials()
    {
        Invalidate();
    }

    internal void SetRequireUniqueModelInstance(bool required)
    {
        if (_requireUniqueModelInstance == required)
            return;

        _requireUniqueModelInstance = required;
        Invalidate();
    }

    /// <inheritdoc />
    public override void Invalidate()
    {
        if (_ownsUniqueModelInstance && RenderModel.HasValue)
        {
            Raylib.UnloadModel(RenderModel.Value);
            _ownsUniqueModelInstance = false;
        }

        ModelVersion++;
        base.Invalidate();
    }

    /// <inheritdoc />
    public override void Start()
    {
        EnsureModelReady();
    }

    /// <inheritdoc />
    public override void OnDestroy()
    {
        Invalidate();
    }
}
