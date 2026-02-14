using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing.Effects;

/// <summary>
/// PSX-style 4x4 Bayer ordered dithering effect. Quantizes color depth and applies
/// a tiled dither pattern for a retro look.
/// </summary>
public class DitherEffect : PostProcessEffect
{
    /// <inheritdoc/>
    public override string DisplayName => "Dither";

    /// <summary>
    /// Number of color levels per channel. Lower values produce a more retro look (e.g. 32 for PSX-style).
    /// </summary>
    public float ColorLevels { get; set; } = 32f;

    /// <summary>
    /// Blend factor between original and dithered output (0 = original, 1 = fully dithered).
    /// </summary>
    public float DitherStrength { get; set; } = 1f;

    private Shader _shader;
    private int _colorLevelsLoc = -1;
    private int _ditherStrengthLoc = -1;

    /// <inheritdoc/>
    public override void Initialize(string shaderBasePath)
    {
        var vsPath = Path.Combine(shaderBasePath, "postprocess.vs");
        var fsPath = Path.Combine(shaderBasePath, "dither.fs");

        _shader = Raylib.LoadShader(vsPath, fsPath);
        if (_shader.Id == 0) return;

        _colorLevelsLoc = Raylib.GetShaderLocation(_shader, "colorLevels");
        _ditherStrengthLoc = Raylib.GetShaderLocation(_shader, "ditherStrength");

        IsInitialized = true;
    }

    /// <inheritdoc/>
    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        if (!IsInitialized) return;

        if (_colorLevelsLoc >= 0)
            Raylib.SetShaderValue(_shader, _colorLevelsLoc, ColorLevels, ShaderUniformDataType.Float);
        if (_ditherStrengthLoc >= 0)
            Raylib.SetShaderValue(_shader, _ditherStrengthLoc, DitherStrength, ShaderUniformDataType.Float);

        PostProcessContext.Blit(source, destination, _shader);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (IsInitialized)
        {
            Raylib.UnloadShader(_shader);
            IsInitialized = false;
        }
    }
}
