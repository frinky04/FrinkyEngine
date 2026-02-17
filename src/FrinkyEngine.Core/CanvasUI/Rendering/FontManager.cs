using FrinkyEngine.Core.Assets;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class FontManager
{
    private Font _defaultFont;
    private bool _initialized;
    private bool _ownsDefaultFont;
    private readonly Dictionary<string, Font> _fonts = new(StringComparer.OrdinalIgnoreCase);

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

    public void RegisterFont(string name, string path, int loadSize = 32)
    {
        var fontPath = FindFont(path) ?? FindFont(AssetManager.Instance.ResolvePath(path));
        if (fontPath == null) return;

        var font = Raylib.LoadFontEx(fontPath, loadSize, null, 0);
        Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);

        if (_fonts.TryGetValue(name, out var existing) && existing.GlyphCount > 0)
            Raylib.UnloadFont(existing);

        _fonts[name] = font;
        DrawCommands.ClearMeasureCache();
    }

    public Font GetFont(string? name)
    {
        if (name != null && _fonts.TryGetValue(name, out var font))
            return font;
        return DefaultFont;
    }

    public void Shutdown()
    {
        foreach (var font in _fonts.Values)
        {
            if (font.GlyphCount > 0)
                Raylib.UnloadFont(font);
        }
        _fonts.Clear();

        if (_initialized && _ownsDefaultFont && _defaultFont.GlyphCount > 0)
            Raylib.UnloadFont(_defaultFont);
        _initialized = false;
        _ownsDefaultFont = false;
        DrawCommands.ClearMeasureCache();
    }

    private static string? FindFont(string relativePath)
    {
        if (File.Exists(relativePath)) return relativePath;

        var candidate = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(candidate)) return candidate;

        return null;
    }
}
