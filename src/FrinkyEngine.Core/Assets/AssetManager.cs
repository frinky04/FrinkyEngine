using Raylib_cs;

namespace FrinkyEngine.Core.Assets;

/// <summary>
/// Singleton that loads and caches models and textures from the project's assets directory.
/// </summary>
public class AssetManager
{
    /// <summary>
    /// The global asset manager instance.
    /// </summary>
    public static AssetManager Instance { get; } = new();

    private readonly Dictionary<string, Model> _models = new();
    private readonly Dictionary<string, Texture2D> _textures = new();

    /// <summary>
    /// Root directory for resolving relative asset paths (defaults to "Assets").
    /// </summary>
    public string AssetsPath { get; set; } = "Assets";

    /// <summary>
    /// Combines a relative asset path with <see cref="AssetsPath"/> to produce a full file path.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The resolved absolute path.</returns>
    public string ResolvePath(string relativePath)
    {
        return Path.Combine(AssetsPath, relativePath);
    }

    /// <summary>
    /// Loads a 3D model from the assets directory, returning a cached copy if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Model"/>.</returns>
    public Model LoadModel(string relativePath)
    {
        if (_models.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var model = Raylib.LoadModel(fullPath);
        _models[relativePath] = model;
        return model;
    }

    /// <summary>
    /// Loads a texture from the assets directory, returning a cached copy if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Texture2D"/>.</returns>
    public Texture2D LoadTexture(string relativePath)
    {
        if (_textures.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var texture = Raylib.LoadTexture(fullPath);
        _textures[relativePath] = texture;
        return texture;
    }

    /// <summary>
    /// Removes a specific asset from the cache and unloads its GPU resources.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root (forward slashes are normalized).</param>
    public void InvalidateAsset(string relativePath)
    {
        // Normalize to forward slashes to match cache keys
        var normalized = relativePath.Replace('\\', '/');
        if (_models.Remove(normalized, out var model))
            Raylib.UnloadModel(model);
        if (_textures.Remove(normalized, out var texture))
            Raylib.UnloadTexture(texture);
    }

    /// <summary>
    /// Unloads all cached models and textures, freeing GPU resources.
    /// </summary>
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
