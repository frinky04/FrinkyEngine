using Raylib_cs;

namespace FrinkyEngine.Core.Assets;

public class AssetManager
{
    public static AssetManager Instance { get; } = new();

    private readonly Dictionary<string, Model> _models = new();
    private readonly Dictionary<string, Texture2D> _textures = new();

    public string AssetsPath { get; set; } = "Assets";

    public string ResolvePath(string relativePath)
    {
        return Path.Combine(AssetsPath, relativePath);
    }

    public Model LoadModel(string relativePath)
    {
        if (_models.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var model = Raylib.LoadModel(fullPath);
        _models[relativePath] = model;
        return model;
    }

    public Texture2D LoadTexture(string relativePath)
    {
        if (_textures.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var texture = Raylib.LoadTexture(fullPath);
        _textures[relativePath] = texture;
        return texture;
    }

    public void InvalidateAsset(string relativePath)
    {
        // Normalize to forward slashes to match cache keys
        var normalized = relativePath.Replace('\\', '/');
        if (_models.Remove(normalized, out var model))
            Raylib.UnloadModel(model);
        if (_textures.Remove(normalized, out var texture))
            Raylib.UnloadTexture(texture);
    }

    public void UnloadAll()
    {
        foreach (var model in _models.Values)
            Raylib.UnloadModel(model);
        _models.Clear();

        foreach (var texture in _textures.Values)
            Raylib.UnloadTexture(texture);
        _textures.Clear();
    }
}
