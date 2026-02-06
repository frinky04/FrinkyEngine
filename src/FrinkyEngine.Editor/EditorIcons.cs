using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class EditorIcons
{
    public static float IconScale { get; set; } = 1.0f;
    public const string FolderIconPath = "EditorAssets/Icons/folder.png";

    public static float GetIconSize() => ImGuiNET.ImGui.GetFrameHeight() * IconScale;

    private static readonly Dictionary<AssetType, Texture2D> _icons = new();
    private static Texture2D _folderIcon;
    private static bool _hasFolderIcon;

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

        if (!File.Exists(FolderIconPath))
        {
            FrinkyLog.Warning($"Editor icon not found: {FolderIconPath}");
            return;
        }

        var folderTexture = Raylib.LoadTexture(FolderIconPath);
        if (folderTexture.Id > 0)
        {
            _folderIcon = folderTexture;
            _hasFolderIcon = true;
        }
        else
        {
            FrinkyLog.Warning($"Failed to load editor icon: {FolderIconPath}");
        }
    }

    public static void Unload()
    {
        foreach (var texture in _icons.Values)
            Raylib.UnloadTexture(texture);
        _icons.Clear();

        if (_hasFolderIcon)
        {
            Raylib.UnloadTexture(_folderIcon);
            _folderIcon = default;
            _hasFolderIcon = false;
        }
    }

    public static Texture2D? GetIcon(AssetType type)
    {
        if (_icons.TryGetValue(type, out var texture))
            return texture;
        if (type != AssetType.Unknown && _icons.TryGetValue(AssetType.Unknown, out var fallback))
            return fallback;
        return null;
    }

    public static Texture2D? GetFolderIcon()
    {
        if (_hasFolderIcon)
            return _folderIcon;
        return null;
    }
}
