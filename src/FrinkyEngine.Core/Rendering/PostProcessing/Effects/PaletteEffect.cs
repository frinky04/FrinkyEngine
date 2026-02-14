using FrinkyEngine.Core.Assets;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing.Effects;

/// <summary>
/// Snaps rendered colors to the nearest color in a loaded JASC-PAL palette file.
/// Loads the palette as a 1D GPU texture and performs nearest-color matching in the fragment shader.
/// </summary>
public class PaletteEffect : PostProcessEffect
{
    /// <inheritdoc/>
    public override string DisplayName => "Palette";

    /// <summary>
    /// Asset reference to a .pal file (JASC-PAL format).
    /// </summary>
    [AssetFilter(AssetType.Palette)]
    public AssetReference PalettePath { get; set; } = new("");

    private Shader _shader;
    private int _paletteSizeLoc = -1;
    private int _paletteTexLoc = -1;
    private uint _paletteTexId;
    private int _paletteColorCount;
    private string _loadedPalettePath = "";

    /// <inheritdoc/>
    public override void Initialize(string shaderBasePath)
    {
        var vsPath = Path.Combine(shaderBasePath, "postprocess.vs");
        var fsPath = Path.Combine(shaderBasePath, "palette.fs");

        _shader = Raylib.LoadShader(vsPath, fsPath);
        if (_shader.Id == 0) return;

        _paletteSizeLoc = Raylib.GetShaderLocation(_shader, "paletteSize");
        _paletteTexLoc = Raylib.GetShaderLocation(_shader, "paletteTex");

        IsInitialized = true;
    }

    /// <inheritdoc/>
    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        if (!IsInitialized) return;

        // Reload palette if path changed
        if (_loadedPalettePath != PalettePath.Path)
            LoadPalette();

        if (_paletteTexId == 0 || _paletteColorCount == 0) return;

        // Set uniforms
        if (_paletteSizeLoc >= 0)
            Raylib.SetShaderValue(_shader, _paletteSizeLoc, _paletteColorCount, ShaderUniformDataType.Int);

        // Bind palette texture to slot 1
        if (_paletteTexLoc >= 0)
        {
            Raylib.SetShaderValue(_shader, _paletteTexLoc, 1, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(1);
            Rlgl.EnableTexture(_paletteTexId);
        }

        PostProcessContext.Blit(source, destination, _shader);

        // Unbind
        Rlgl.ActiveTextureSlot(1);
        Rlgl.DisableTexture();
        Rlgl.ActiveTextureSlot(0);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (IsInitialized)
        {
            Raylib.UnloadShader(_shader);
            IsInitialized = false;
        }

        UnloadPaletteTexture();
    }

    private void LoadPalette()
    {
        var path = PalettePath.Path;
        _loadedPalettePath = path;
        UnloadPaletteTexture();

        if (PalettePath.IsEmpty) return;

        var dbPath = AssetDatabase.Instance.ResolveAssetPath(path) ?? path;
        var fullPath = AssetManager.Instance.ResolvePath(dbPath);
        if (!File.Exists(fullPath))
        {
            FrinkyLog.Warning($"PaletteEffect: palette file not found: {path}");
            return;
        }

        var colors = ParseJascPal(fullPath);
        if (colors == null || colors.Length == 0)
        {
            FrinkyLog.Warning($"PaletteEffect: failed to parse palette: {path}");
            return;
        }

        _paletteColorCount = colors.Length;

        // Create 1D texture (uploaded as a 2D texture with height=1 since Raylib doesn't expose GL 1D textures directly)
        var img = Raylib.GenImageColor(_paletteColorCount, 1, Color.Black);
        for (int i = 0; i < _paletteColorCount; i++)
        {
            Raylib.ImageDrawPixel(ref img, i, 0, colors[i]);
        }

        var tex = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);

        if (tex.Id == 0)
        {
            FrinkyLog.Warning($"PaletteEffect: failed to create palette texture");
            return;
        }

        Raylib.SetTextureFilter(tex, TextureFilter.Point);
        Raylib.SetTextureWrap(tex, TextureWrap.Clamp);
        _paletteTexId = tex.Id;

        FrinkyLog.Info($"PaletteEffect: loaded {_paletteColorCount} colors from {path}");
    }

    private void UnloadPaletteTexture()
    {
        if (_paletteTexId != 0)
        {
            Rlgl.UnloadTexture(_paletteTexId);
            _paletteTexId = 0;
        }
        _paletteColorCount = 0;
    }

    private static Color[]? ParseJascPal(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 3) return null;

            // Line 0: "JASC-PAL"
            if (lines[0].Trim() != "JASC-PAL") return null;

            // Line 1: "0100" (version)
            // Line 2: color count
            if (!int.TryParse(lines[2].Trim(), out var count) || count <= 0)
                return null;

            var colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                int lineIdx = 3 + i;
                if (lineIdx >= lines.Length) break;

                var parts = lines[lineIdx].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                if (byte.TryParse(parts[0], out var r) &&
                    byte.TryParse(parts[1], out var g) &&
                    byte.TryParse(parts[2], out var b))
                {
                    colors[i] = new Color((int)r, (int)g, (int)b, 255);
                }
            }

            return colors;
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"PaletteEffect: error reading palette file: {ex.Message}");
            return null;
        }
    }
}
