using FrinkyEngine.Core.Assets;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Applies engine material settings to Raylib model materials.
/// </summary>
public static class MaterialApplicator
{
    /// <summary>
    /// Applies material settings to the specified model material slot.
    /// </summary>
    /// <param name="model">Target model.</param>
    /// <param name="materialIndex">Material index in the model.</param>
    /// <param name="materialType">Material mapping mode.</param>
    /// <param name="texturePath">Asset-relative albedo texture path.</param>
    /// <param name="triplanarScale">Triplanar projection scale.</param>
    /// <param name="triplanarBlendSharpness">Triplanar axis blend sharpness.</param>
    /// <param name="triplanarUseWorldSpace">Whether triplanar uses world-space coordinates.</param>
    public static unsafe void ApplyToModel(
        Model model,
        int materialIndex,
        MaterialType materialType,
        string texturePath,
        float triplanarScale,
        float triplanarBlendSharpness,
        bool triplanarUseWorldSpace)
    {
        if (materialIndex < 0 || materialIndex >= model.MaterialCount)
            return;

        var albedo = ResolveAlbedoTexture(materialType, texturePath);
        bool triplanarEnabled = materialType == MaterialType.TriplanarTexture;
        var triplanarParams = AssetManager.Instance.GetTriplanarParamsTexture(
            triplanarEnabled,
            triplanarScale,
            triplanarBlendSharpness,
            triplanarUseWorldSpace);

        model.Materials[materialIndex].Maps[(int)MaterialMapIndex.Albedo].Texture = albedo;
        model.Materials[materialIndex].Maps[(int)MaterialMapIndex.Albedo].Color = new Color(255, 255, 255, 255);
        model.Materials[materialIndex].Maps[(int)MaterialMapIndex.Brdf].Texture = triplanarParams;
    }

    private static Texture2D ResolveAlbedoTexture(MaterialType materialType, string texturePath)
    {
        if ((materialType == MaterialType.Textured || materialType == MaterialType.TriplanarTexture)
            && !string.IsNullOrEmpty(texturePath))
        {
            return AssetManager.Instance.LoadTexture(texturePath);
        }

        return new Texture2D
        {
            Id = Rlgl.GetTextureIdDefault(),
            Width = 1,
            Height = 1,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8
        };
    }
}
