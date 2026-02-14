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

    private readonly Dictionary<string, Model> _models = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CachedModelAnimations> _modelAnimations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Sound> _audioClips = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Music> _audioStreams = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TriplanarParamKey, Texture2D> _triplanarParamsTextures = new();

    /// <summary>
    /// Monotonically increasing counter incremented each time an asset is invalidated.
    /// Components can track this to detect when cached asset pointers may be stale.
    /// </summary>
    public int AssetGeneration { get; private set; }

    /// <summary>
    /// Root directory for resolving relative asset paths (defaults to "Assets").
    /// </summary>
    public string AssetsPath { get; set; } = "Assets";

    /// <summary>
    /// Root directory for resolving engine content asset paths (defaults to "EngineContent").
    /// </summary>
    public string EngineContentPath { get; set; } = "EngineContent";

    /// <summary>
    /// Fallback texture shown when a referenced texture file does not exist on disk.
    /// </summary>
    public Texture2D? ErrorTexture { get; set; }

    /// <summary>
    /// Fallback model shown when a referenced model file does not exist on disk.
    /// </summary>
    public Model? ErrorModel { get; set; }

    /// <summary>
    /// Combines a relative asset path with <see cref="AssetsPath"/> to produce a full file path.
    /// Paths with the <c>engine:</c> prefix are resolved against <see cref="EngineContentPath"/> instead.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The resolved absolute path.</returns>
    public string ResolvePath(string relativePath)
    {
        if (AssetReference.HasEnginePrefix(relativePath))
            return Path.Combine(EngineContentPath, AssetReference.StripEnginePrefix(relativePath));

        return Path.Combine(AssetsPath, relativePath);
    }

    /// <summary>
    /// Loads a 3D model from the assets directory, returning a cached copy if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Model"/>.</returns>
    public Model LoadModel(string relativePath)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var key = relativePath.Replace('\\', '/');
        if (_models.TryGetValue(key, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
            return ErrorModel ?? default;

        var model = Raylib.LoadModel(fullPath);
        _models[key] = model;
        return model;
    }

    /// <summary>
    /// Loads a unique 3D model instance from the assets directory without using the shared model cache.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Model"/>.</returns>
    public Model LoadModelUnique(string relativePath)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
            return ErrorModel ?? default;

        return Raylib.LoadModel(fullPath);
    }

    /// <summary>
    /// Loads skeletal animation clips for a model and caches them by asset path.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <param name="animationCount">Receives the number of loaded clips.</param>
    /// <returns>Pointer to cached animation clips, or <c>null</c> when unavailable.</returns>
    public unsafe ModelAnimation* LoadModelAnimations(string relativePath, out int animationCount)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var key = relativePath.Replace('\\', '/');
        if (_modelAnimations.TryGetValue(key, out var cached))
        {
            animationCount = cached.Count;
            return cached.Pointer;
        }

        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
        {
            animationCount = 0;
            return null;
        }

        int count = 0;
        var animations = Raylib.LoadModelAnimations(fullPath, ref count);
        animationCount = count;
        _modelAnimations[key] = new CachedModelAnimations(animations, animationCount);
        return animations;
    }

    /// <summary>
    /// Checks whether a model has one or more skeletal animation clips.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <param name="animationCount">Receives the number of clips if available.</param>
    /// <returns><c>true</c> when one or more clips exist; otherwise <c>false</c>.</returns>
    public bool ModelHasAnimations(string relativePath, out int animationCount)
    {
        unsafe
        {
            return LoadModelAnimations(relativePath, out animationCount) != null && animationCount > 0;
        }
    }

    /// <summary>
    /// Loads a texture from the assets directory, returning a cached copy if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Texture2D"/>.</returns>
    public Texture2D LoadTexture(string relativePath)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var key = relativePath.Replace('\\', '/');
        if (_textures.TryGetValue(key, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
            return ErrorTexture ?? default;

        var texture = Raylib.LoadTexture(fullPath);
        _textures[key] = texture;
        return texture;
    }

    /// <summary>
    /// Loads a short-form audio clip from the assets directory, returning a cached copy if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Sound"/>.</returns>
    public Sound LoadAudioClip(string relativePath)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var key = relativePath.Replace('\\', '/');
        if (_audioClips.TryGetValue(key, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var sound = Raylib.LoadSound(fullPath);
        _audioClips[key] = sound;
        return sound;
    }

    /// <summary>
    /// Loads a streamed audio asset from the assets directory, returning a cached stream if already loaded.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <returns>The loaded <see cref="Music"/> stream.</returns>
    public Music LoadAudioStream(string relativePath)
    {
        relativePath = ResolveViaDatabase(relativePath);
        var key = relativePath.Replace('\\', '/');
        if (_audioStreams.TryGetValue(key, out var cached))
            return cached;

        var fullPath = ResolvePath(relativePath);
        var music = Raylib.LoadMusicStream(fullPath);
        _audioStreams[key] = music;
        return music;
    }

    /// <summary>
    /// Gets or creates a 1x1 float texture used to pass triplanar material parameters to shaders.
    /// </summary>
    /// <param name="enabled">Whether triplanar mode is enabled for this material.</param>
    /// <param name="scale">Triplanar texture scale.</param>
    /// <param name="blendSharpness">Triplanar axis blend sharpness.</param>
    /// <param name="useWorldSpace">Whether projection uses world-space coordinates.</param>
    /// <returns>A cached 1x1 parameter texture.</returns>
    public Texture2D GetTriplanarParamsTexture(bool enabled, float scale, float blendSharpness, bool useWorldSpace)
    {
        if (!float.IsFinite(scale))
            scale = 1f;
        if (!float.IsFinite(blendSharpness))
            blendSharpness = 4f;

        scale = MathF.Max(0.0001f, scale);
        blendSharpness = MathF.Max(0.0001f, blendSharpness);

        var key = new TriplanarParamKey(enabled, Quantize(scale), Quantize(blendSharpness), useWorldSpace);
        if (_triplanarParamsTextures.TryGetValue(key, out var cached))
            return cached;

        var texture = CreateTriplanarParamsTexture(enabled, scale, blendSharpness, useWorldSpace);
        _triplanarParamsTextures[key] = texture;
        return texture;
    }

    /// <summary>
    /// Removes a specific asset from the cache and unloads its GPU resources.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root (forward slashes are normalized).</param>
    public void InvalidateAsset(string relativePath)
    {
        AssetGeneration++;

        // Normalize to forward slashes to match cache keys
        var normalized = relativePath.Replace('\\', '/');
        if (_models.Remove(normalized, out var model))
            Raylib.UnloadModel(model);
        if (_modelAnimations.Remove(normalized, out var animations))
            animations.Unload();
        if (_textures.Remove(normalized, out var texture))
            Raylib.UnloadTexture(texture);
        if (_audioClips.Remove(normalized, out var clip))
            Raylib.UnloadSound(clip);
        if (_audioStreams.Remove(normalized, out var music))
            Raylib.UnloadMusicStream(music);
    }

    /// <summary>
    /// Unloads all cached models and textures, freeing GPU resources.
    /// </summary>
    public void UnloadAll()
    {
        foreach (var model in _models.Values)
            Raylib.UnloadModel(model);
        _models.Clear();

        foreach (var animations in _modelAnimations.Values)
            animations.Unload();
        _modelAnimations.Clear();

        foreach (var texture in _textures.Values)
            Raylib.UnloadTexture(texture);
        _textures.Clear();

        foreach (var clip in _audioClips.Values)
            Raylib.UnloadSound(clip);
        _audioClips.Clear();

        foreach (var stream in _audioStreams.Values)
            Raylib.UnloadMusicStream(stream);
        _audioStreams.Clear();

        foreach (var texture in _triplanarParamsTextures.Values)
            Raylib.UnloadTexture(texture);
        _triplanarParamsTextures.Clear();
    }

    private static string ResolveViaDatabase(string nameOrPath)
    {
        return AssetDatabase.Instance.ResolveAssetPath(nameOrPath) ?? nameOrPath;
    }

    private static int Quantize(float value) => (int)MathF.Round(value * 1000f);

    private static unsafe Texture2D CreateTriplanarParamsTexture(bool enabled, float scale, float blendSharpness, bool useWorldSpace)
    {
        float[] data =
        {
            enabled ? 1f : 0f,
            scale,
            blendSharpness,
            useWorldSpace ? 1f : 0f
        };

        fixed (float* ptr = data)
        {
            uint textureId = Rlgl.LoadTexture(ptr, 1, 1, PixelFormat.UncompressedR32G32B32A32, 1);
            var texture = new Texture2D
            {
                Id = textureId,
                Width = 1,
                Height = 1,
                Mipmaps = 1,
                Format = PixelFormat.UncompressedR32G32B32A32
            };

            if (texture.Id != 0)
            {
                Raylib.SetTextureFilter(texture, TextureFilter.Point);
                Raylib.SetTextureWrap(texture, TextureWrap.Clamp);
            }

            return texture;
        }
    }

    private readonly record struct TriplanarParamKey(
        bool Enabled,
        int ScaleMilli,
        int BlendSharpnessMilli,
        bool UseWorldSpace);

    private readonly unsafe struct CachedModelAnimations
    {
        public CachedModelAnimations(ModelAnimation* pointer, int count)
        {
            Pointer = pointer;
            Count = count;
        }

        public ModelAnimation* Pointer { get; }
        public int Count { get; }

        public void Unload()
        {
            if (Pointer != null && Count > 0)
                Raylib.UnloadModelAnimations(Pointer, Count);
        }
    }
}
