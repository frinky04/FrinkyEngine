namespace FrinkyEngine.Core.Assets;

public enum AssetType
{
    Unknown,
    Model,
    Scene,
    Texture,
    Script
}

public class AssetEntry
{
    public string RelativePath { get; }
    public string FileName { get; }
    public string Extension { get; }
    public AssetType Type { get; }

    public AssetEntry(string relativePath, AssetType type)
    {
        RelativePath = relativePath.Replace('\\', '/');
        FileName = Path.GetFileName(relativePath);
        Extension = Path.GetExtension(relativePath).ToLowerInvariant();
        Type = type;
    }
}
