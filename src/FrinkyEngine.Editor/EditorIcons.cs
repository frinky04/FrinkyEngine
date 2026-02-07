using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;
using ImGuiNET;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class EditorIcons
{
    public static float IconScale { get; set; } = 1.0f;
    public const string FolderIconPath = "EditorAssets/Icons/folder.png";

    public static float GetIconSize() => ImGui.GetFrameHeight() * IconScale;

    private static readonly Dictionary<AssetType, Texture2D> _icons = new();
    private static Texture2D _folderIcon;
    private static bool _hasFolderIcon;

    private static Dictionary<AssetType, uint>? _iconTints;
    private static uint _folderIconTint;

    private static readonly Dictionary<AssetType, string> _iconPaths = new()
    {
        { AssetType.Model, "EditorAssets/Icons/3d_model.png" },
        { AssetType.Scene, "EditorAssets/Icons/scene.png" },
        { AssetType.Texture, "EditorAssets/Icons/texture.png" },
        { AssetType.Script, "EditorAssets/Icons/script.png" },
        { AssetType.Prefab, "EditorAssets/Icons/prefab.png" },
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

    public static uint GetIconTint(AssetType type)
    {
        EnsureTintsInitialized();
        return _iconTints!.TryGetValue(type, out var tint) ? tint : _iconTints[AssetType.Unknown];
    }

    public static uint GetFolderIconTint()
    {
        EnsureTintsInitialized();
        return _folderIconTint;
    }

    private static void EnsureTintsInitialized()
    {
        if (_iconTints != null)
            return;

        _iconTints = new Dictionary<AssetType, uint>
        {
            { AssetType.Model,   ImGui.GetColorU32(new System.Numerics.Vector4(0.3f, 0.8f, 0.9f, 1f)) },
            { AssetType.Scene,   ImGui.GetColorU32(new System.Numerics.Vector4(0.5f, 0.5f, 1.0f, 1f)) },
            { AssetType.Texture, ImGui.GetColorU32(new System.Numerics.Vector4(1.0f, 0.8f, 0.3f, 1f)) },
            { AssetType.Script,  ImGui.GetColorU32(new System.Numerics.Vector4(0.4f, 0.9f, 0.4f, 1f)) },
            { AssetType.Prefab,  ImGui.GetColorU32(new System.Numerics.Vector4(0.8f, 0.4f, 0.9f, 1f)) },
            { AssetType.Unknown, ImGui.GetColorU32(new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1f)) },
        };
        _folderIconTint = ImGui.GetColorU32(new System.Numerics.Vector4(0.9f, 0.8f, 0.4f, 1f));
    }
}
