using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class FontManager
{
    private Font _defaultFont;
    private bool _initialized;
    private bool _ownsDefaultFont;

    public Font DefaultFont
    {
        get
        {
            EnsureInitialized();
            return _defaultFont;
        }
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        var fontPath = FindFont("EngineContent/Fonts/JetBrainsMono-Regular.ttf");
        if (fontPath != null)
        {
            _defaultFont = Raylib.LoadFontEx(fontPath, 32, null, 0);
            Raylib.SetTextureFilter(_defaultFont.Texture, TextureFilter.Bilinear);
            _ownsDefaultFont = true;
        }
        else
        {
            _defaultFont = Raylib.GetFontDefault();
            _ownsDefaultFont = false;
        }
    }

    public void Shutdown()
    {
        if (_initialized && _ownsDefaultFont && _defaultFont.GlyphCount > 0)
            Raylib.UnloadFont(_defaultFont);
        _initialized = false;
        _ownsDefaultFont = false;
    }

    private static string? FindFont(string relativePath)
    {
        if (File.Exists(relativePath)) return relativePath;

        var candidate = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(candidate)) return candidate;

        return null;
    }
}
