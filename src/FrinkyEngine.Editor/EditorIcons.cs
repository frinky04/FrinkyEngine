using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class EditorIcons
{
    private static readonly Dictionary<AssetType, Texture2D> _icons = new();

    private static readonly Dictionary<AssetType, string> _iconPaths = new()
    {
        { AssetType.Model, "EditorAssets/Icons/3d_model.png" },
        { AssetType.Scene, "EditorAssets/Icons/scene.png" },
        { AssetType.Texture, "EditorAssets/Icons/texture.png" },
        { AssetType.Script, "EditorAssets/Icons/script.png" },
        { AssetType.Unknown, "EditorAssets/Icons/file.png" },
    };

    public static void Load()
    {
        foreach (var (type, path) in _iconPaths)
        {
            if (!File.Exists(path))
            {
                FrinkyLog.Warning($"Editor icon not found: {path}");
                continue;
            }

            var texture = Raylib.LoadTexture(path);
            if (texture.Id > 0)
                _icons[type] = texture;
            else
                FrinkyLog.Warning($"Failed to load editor icon: {path}");
        }
    }

    public static void Unload()
    {
        foreach (var texture in _icons.Values)
            Raylib.UnloadTexture(texture);
        _icons.Clear();
    }

    public static Texture2D? GetIcon(AssetType type)
    {
        if (_icons.TryGetValue(type, out var texture))
            return texture;
        if (type != AssetType.Unknown && _icons.TryGetValue(AssetType.Unknown, out var fallback))
            return fallback;
        return null;
    }
}
