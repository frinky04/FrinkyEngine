using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Rendering;

internal class FontManager
{
    private Font _defaultFont;
    private bool _initialized;
    private readonly Dictionary<string, Font> _cache = new();

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
        }
        else
        {
            _defaultFont = Raylib.GetFontDefault();
        }
    }

    public Font GetFont(string? path)
    {
        if (string.IsNullOrEmpty(path)) return DefaultFont;
        if (_cache.TryGetValue(path, out var cached)) return cached;

        var resolved = FindFont(path);
        if (resolved == null) return DefaultFont;

        var font = Raylib.LoadFontEx(resolved, 32, null, 0);
        Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
        _cache[path] = font;
        return font;
    }

    public void Shutdown()
    {
        if (_initialized && _defaultFont.GlyphCount > 0)
            Raylib.UnloadFont(_defaultFont);

        foreach (var font in _cache.Values)
            Raylib.UnloadFont(font);
        _cache.Clear();
        _initialized = false;
    }

    private static string? FindFont(string relativePath)
    {
        // Check relative to working directory
        if (File.Exists(relativePath)) return relativePath;

        // Check relative to executable
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDir, relativePath);
        if (File.Exists(candidate)) return candidate;

        return null;
    }
}
