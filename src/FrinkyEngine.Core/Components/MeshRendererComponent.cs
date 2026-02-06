using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class MeshRendererComponent : RenderableComponent
{
    private string _modelPath = string.Empty;

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

    public void RefreshMaterials()
    {
        RenderModel = null;
    }

    public override void Start()
    {
        EnsureModelReady();
    }
}
