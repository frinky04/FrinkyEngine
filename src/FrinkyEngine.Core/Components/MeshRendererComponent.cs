using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Renders a 3D model loaded from a file (e.g. .obj, .gltf, .glb).
/// Supports multiple material slots for per-material texture assignment.
/// </summary>
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
        unsafe
        {
            for (int i = 0; i < model.MaterialCount && i < MaterialSlots.Count; i++)
            {
                var slot = MaterialSlots[i];
                if (slot.MaterialType == MaterialType.Textured && !string.IsNullOrEmpty(slot.TexturePath))
                {
                    var texture = AssetManager.Instance.LoadTexture(slot.TexturePath);
                    model.Materials[i].Maps[(int)MaterialMapIndex.Albedo].Texture = texture;
                }
                else
                {
                    // Reset to Raylib's default 1x1 white pixel texture
                    model.Materials[i].Maps[(int)MaterialMapIndex.Albedo].Texture = new Texture2D
                    {
                        Id = Rlgl.GetTextureIdDefault(),
                        Width = 1,
                        Height = 1,
                        Mipmaps = 1,
                        Format = PixelFormat.UncompressedR8G8B8A8
                    };
                }

                // Reset albedo map color to white so colDiffuse doesn't zero out the shader output.
                // Model files often set secondary materials to black diffuse, causing pitch-black rendering.
                model.Materials[i].Maps[(int)MaterialMapIndex.Albedo].Color = new Color(255, 255, 255, 255);
            }
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
