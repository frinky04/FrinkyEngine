using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Prefabs;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Serialization;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public enum IconGenerationStatus { None, Queued, Generating, Failed, Ready }

public sealed class AssetIconService : IDisposable
{
    private const int IconSize = 256;
    private const double MinJobIntervalSeconds = 0.2;
    private const int CacheVersion = 2;
    private const string CacheFolderName = "asset-icons";
    private const string ManifestFileName = "manifest.json";

    private readonly Queue<string> _queue = new();
    private readonly HashSet<string> _queued = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AssetEntry> _eligibleAssets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> _loadedIcons = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _failedKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly JsonSerializerOptions _manifestJsonOptions = new() { WriteIndented = true };

    private Dictionary<string, ManifestEntry> _manifest = new(StringComparer.OrdinalIgnoreCase);
    private string? _cacheDirectory;
    private string? _manifestPath;
    private string? _currentlyGenerating;
    private bool _isInitialized;
    private bool _manifestDirty;
    private int _pendingSaveCounter;
    private RenderTexture2D _renderTarget;
    private bool _hasRenderTarget;
    private double _lastJobTimeSeconds;

    // Perf counters
    private int _totalGenerated;
    private int _totalFailed;
    private int _cacheHitCount;
    private double _lastGenerationMs;

    public void Initialize(string projectDirectory)
    {
        Shutdown();

        var frinkyDir = Path.Combine(projectDirectory, ".frinky");
        _cacheDirectory = Path.Combine(frinkyDir, CacheFolderName);
        _manifestPath = Path.Combine(_cacheDirectory, ManifestFileName);
        Directory.CreateDirectory(_cacheDirectory);

        LoadManifest();
        ReindexEligibleAssets();
        QueueAllEligibleAssets();
        PruneStaleEntries();

        _isInitialized = true;
    }

    public void Tick(SceneRenderer renderer)
    {
        if (!_isInitialized || _queue.Count == 0)
        {
            FlushManifestIfNeeded(force: false);
            return;
        }

        double now = Raylib.GetTime();
        if (_lastJobTimeSeconds > 0.0 && now - _lastJobTimeSeconds < MinJobIntervalSeconds)
            return;

        var key = _queue.Dequeue();
        _queued.Remove(key);
        ProcessSingleAsset(renderer, key);
        _lastJobTimeSeconds = now;
        FlushManifestIfNeeded(force: false);
    }

    public void OnAssetDatabaseRefreshed(HashSet<string>? changedRelativePaths)
    {
        if (!_isInitialized)
            return;

        ReindexEligibleAssets();

        if (changedRelativePaths == null)
        {
            QueueAllEligibleAssets();
        }
        else
        {
            foreach (var relPath in changedRelativePaths)
            {
                var key = BuildProjectKey(relPath);
                if (_eligibleAssets.ContainsKey(key))
                {
                    Queue(key);
                    continue;
                }

                RemoveKey(key);
            }
        }

        PruneStaleEntries();
    }

    public bool TryGetIcon(in AssetEntry asset, out Texture2D texture)
    {
        texture = default;
        if (!_isInitialized || !IsSupportedType(asset.Type))
            return false;

        var key = BuildAssetKey(asset);
        if (_loadedIcons.TryGetValue(key, out texture))
            return true;

        if (!TryLoadCachedTexture(key, out texture))
        {
            Queue(key);
            return false;
        }

        return true;
    }

    public bool TryGetIcon(string assetReferencePath, AssetType expectedType, out Texture2D texture)
    {
        texture = default;
        if (!_isInitialized || string.IsNullOrWhiteSpace(assetReferencePath))
            return false;
        if (expectedType != AssetType.Unknown && !IsSupportedType(expectedType))
            return false;

        var resolved = AssetDatabase.Instance.ResolveAssetPath(assetReferencePath);
        if (string.IsNullOrWhiteSpace(resolved))
            return false;

        bool isEngine = AssetReference.HasEnginePrefix(resolved);
        string relativePath = isEngine ? AssetReference.StripEnginePrefix(resolved) : resolved;
        var key = isEngine ? BuildEngineKey(relativePath) : BuildProjectKey(relativePath);

        if (_eligibleAssets.TryGetValue(key, out var asset))
            return TryGetIcon(asset, out texture);

        return false;
    }

    public int QueueLength => _queue.Count;
    public int LoadedIconCount => _loadedIcons.Count;
    public int EligibleAssetCount => _eligibleAssets.Count;
    public int TotalGenerated => _totalGenerated;
    public int TotalFailed => _totalFailed;
    public int CacheHits => _cacheHitCount;
    public double LastGenerationMs => _lastGenerationMs;

    public IconGenerationStatus GetIconStatus(in AssetEntry asset)
    {
        var key = BuildAssetKey(asset);
        if (_loadedIcons.ContainsKey(key))
            return IconGenerationStatus.Ready;
        if (string.Equals(_currentlyGenerating, key, StringComparison.OrdinalIgnoreCase))
            return IconGenerationStatus.Generating;
        if (_failedKeys.Contains(key))
            return IconGenerationStatus.Failed;
        if (_queued.Contains(key))
            return IconGenerationStatus.Queued;
        return IconGenerationStatus.None;
    }

    public void RegenerateIcon(in AssetEntry asset)
    {
        var key = BuildAssetKey(asset);
        RemoveKey(key);
        _failedKeys.Remove(key);
        Queue(key);
    }

    public void Dispose()
    {
        Shutdown();
    }

    public void Shutdown()
    {
        FlushManifestIfNeeded(force: true);

        foreach (var texture in _loadedIcons.Values)
            Raylib.UnloadTexture(texture);
        _loadedIcons.Clear();

        if (_hasRenderTarget)
        {
            Raylib.UnloadRenderTexture(_renderTarget);
            _renderTarget = default;
            _hasRenderTarget = false;
        }

        _queue.Clear();
        _queued.Clear();
        _eligibleAssets.Clear();
        _manifest.Clear();
        _failedKeys.Clear();
        _cacheDirectory = null;
        _manifestPath = null;
        _currentlyGenerating = null;
        _manifestDirty = false;
        _pendingSaveCounter = 0;
        _lastJobTimeSeconds = 0.0;
        _totalGenerated = 0;
        _totalFailed = 0;
        _cacheHitCount = 0;
        _lastGenerationMs = 0.0;
        _isInitialized = false;
    }

    private void ProcessSingleAsset(SceneRenderer renderer, string key)
    {
        _currentlyGenerating = key;
        try
        {
            if (!_eligibleAssets.TryGetValue(key, out var asset))
            {
                RemoveKey(key);
                return;
            }

            string sourcePath = AssetManager.Instance.ResolvePath(BuildAssetKey(asset));
            if (!File.Exists(sourcePath))
            {
                RemoveKey(key);
                return;
            }

            string sourceHash = ComputeSourceHash(asset.Type, sourcePath);
            string iconFileName = GetIconFileName(key);
            string iconPath = Path.Combine(_cacheDirectory!, iconFileName);

            if (_manifest.TryGetValue(key, out var existing)
                && string.Equals(existing.SourceHash, sourceHash, StringComparison.Ordinal)
                && File.Exists(iconPath))
            {
                TryLoadCachedTexture(key, out _);
                return;
            }

            bool generated;
            var sw = Stopwatch.StartNew();
            try
            {
                generated = asset.Type switch
                {
                    AssetType.Texture => GenerateTextureIcon(asset, iconPath),
                    AssetType.Model => GenerateModelIcon(renderer, BuildAssetKey(asset), iconPath),
                    AssetType.Prefab => GeneratePrefabIcon(renderer, BuildAssetKey(asset), iconPath),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                FrinkyLog.Warning($"Asset icon generation failed for '{key}': {ex.Message}");
                generated = false;
            }
            sw.Stop();
            _lastGenerationMs = sw.Elapsed.TotalMilliseconds;

            if (!generated)
            {
                _failedKeys.Add(key);
                _totalFailed++;
                return;
            }

            _failedKeys.Remove(key);
            _totalGenerated++;

            if (_loadedIcons.Remove(key, out var oldTexture))
                Raylib.UnloadTexture(oldTexture);

            var loaded = Raylib.LoadTexture(iconPath);
            if (loaded.Id != 0)
            {
                Raylib.SetTextureFilter(loaded, TextureFilter.Bilinear);
                _loadedIcons[key] = loaded;
            }

            _manifest[key] = new ManifestEntry
            {
                SourceHash = sourceHash,
                IconFile = iconFileName,
                AssetType = asset.Type
            };

            _manifestDirty = true;
            _pendingSaveCounter++;
        }
        finally
        {
            _currentlyGenerating = null;
        }
    }

    private void ReindexEligibleAssets()
    {
        _eligibleAssets.Clear();

        foreach (var asset in AssetDatabase.Instance.GetAssets())
        {
            if (!IsSupportedType(asset.Type))
                continue;
            _eligibleAssets[BuildAssetKey(asset)] = asset;
        }

        foreach (var asset in AssetDatabase.Instance.GetEngineAssets())
        {
            if (!IsSupportedType(asset.Type))
                continue;
            _eligibleAssets[BuildAssetKey(asset)] = asset;
        }
    }

    private void QueueAllEligibleAssets()
    {
        foreach (var key in _eligibleAssets.Keys)
            Queue(key);
    }

    private void Queue(string key)
    {
        if (_queued.Add(key))
            _queue.Enqueue(key);
    }

    private bool TryLoadCachedTexture(string key, out Texture2D texture)
    {
        texture = default;
        if (!_manifest.TryGetValue(key, out var entry))
            return false;

        string iconPath = Path.Combine(_cacheDirectory!, entry.IconFile);
        if (!File.Exists(iconPath))
            return false;

        var loaded = Raylib.LoadTexture(iconPath);
        if (loaded.Id == 0)
            return false;

        Raylib.SetTextureFilter(loaded, TextureFilter.Bilinear);
        if (_loadedIcons.Remove(key, out var oldTexture))
            Raylib.UnloadTexture(oldTexture);
        _loadedIcons[key] = loaded;
        texture = loaded;
        _cacheHitCount++;
        return true;
    }

    private void RemoveKey(string key)
    {
        if (_loadedIcons.Remove(key, out var texture))
            Raylib.UnloadTexture(texture);

        if (_manifest.Remove(key, out var manifestEntry))
        {
            if (!string.IsNullOrWhiteSpace(_cacheDirectory))
            {
                string iconPath = Path.Combine(_cacheDirectory, manifestEntry.IconFile);
                if (File.Exists(iconPath))
                    File.Delete(iconPath);
            }
            _manifestDirty = true;
            _pendingSaveCounter++;
        }
    }

    private void PruneStaleEntries()
    {
        var staleKeys = _manifest.Keys
            .Where(key => !_eligibleAssets.ContainsKey(key))
            .ToList();

        foreach (var key in staleKeys)
            RemoveKey(key);
    }

    private void EnsureRenderTarget()
    {
        if (_hasRenderTarget)
            return;

        _renderTarget = Raylib.LoadRenderTexture(IconSize, IconSize);
        _hasRenderTarget = _renderTarget.Id != 0;
    }

    private bool GenerateTextureIcon(AssetEntry asset, string outputPath)
    {
        EnsureRenderTarget();
        if (!_hasRenderTarget)
            return false;

        var texture = AssetManager.Instance.LoadTexture(BuildAssetKey(asset));
        if (texture.Id == 0 || texture.Width <= 0 || texture.Height <= 0)
            return false;

        Raylib.BeginTextureMode(_renderTarget);
        Raylib.ClearBackground(new Color(24, 24, 26, 255));

        float padding = IconSize * 0.08f;
        float maxDim = IconSize - padding * 2f;
        float scale = MathF.Min(maxDim / texture.Width, maxDim / texture.Height);
        float drawW = texture.Width * scale;
        float drawH = texture.Height * scale;
        float x = (IconSize - drawW) * 0.5f;
        float y = (IconSize - drawH) * 0.5f;
        Raylib.DrawTexturePro(
            texture,
            new Rectangle(0, 0, texture.Width, texture.Height),
            new Rectangle(x, y, drawW, drawH),
            System.Numerics.Vector2.Zero,
            0f,
            Color.White);

        Raylib.EndTextureMode();
        return ExportRenderTarget(outputPath);
    }

    private bool GenerateModelIcon(SceneRenderer renderer, string modelAssetPath, string outputPath)
    {
        EnsureRenderTarget();
        if (!_hasRenderTarget)
            return false;

        var scene = new Scene { Name = "AssetIconPreview" };
        try
        {
            var meshEntity = scene.CreateEntity("PreviewMesh");
            var mesh = meshEntity.AddComponent<MeshRendererComponent>();
            mesh.ModelPath = new AssetReference(modelAssetPath);

            var bounds = mesh.GetWorldBoundingBox();
            if (!bounds.HasValue)
                return false;

            return RenderPreviewScene(renderer, scene, bounds.Value, outputPath);
        }
        finally
        {
            scene.Dispose();
        }
    }

    private bool RenderPreviewScene(SceneRenderer renderer, Scene scene, BoundingBox bounds, string outputPath)
    {
        var lightEntity = scene.CreateEntity("PreviewLight");
        var light = lightEntity.AddComponent<LightComponent>();
        light.LightType = LightType.Directional;
        light.Intensity = 1.35f;
        lightEntity.Transform.EulerAngles = new System.Numerics.Vector3(-35f, -35f, 0f);

        var fillLightEntity = scene.CreateEntity("PreviewFill");
        var fillLight = fillLightEntity.AddComponent<LightComponent>();
        fillLight.LightType = LightType.Skylight;
        fillLight.Intensity = 0.35f;

        var cameraEntity = scene.CreateEntity("PreviewCamera");
        var cameraComponent = cameraEntity.AddComponent<CameraComponent>();
        cameraComponent.IsMain = true;
        cameraComponent.ClearColor = new Color(26, 30, 34, 255);

        const float fovY = 35f;
        var viewDir = System.Numerics.Vector3.Normalize(new System.Numerics.Vector3(1.05f, 0.68f, 1.0f));
        var center = (bounds.Min + bounds.Max) * 0.5f;
        float cameraDistance = ComputePreviewCameraDistance(bounds, viewDir, fovY, targetFill: 0.86f);
        var camPos = center + viewDir * cameraDistance;
        var camera = new Camera3D
        {
            Position = camPos,
            Target = center,
            Up = System.Numerics.Vector3.UnitY,
            FovY = fovY,
            Projection = CameraProjection.Perspective
        };

        renderer.Render(scene, camera, _renderTarget, isEditorMode: false);
        return ExportRenderTarget(outputPath);
    }

    private static float ComputePreviewCameraDistance(
        BoundingBox bounds,
        System.Numerics.Vector3 viewDir,
        float fovYDegrees,
        float targetFill)
    {
        targetFill = Math.Clamp(targetFill, 0.4f, 0.95f);
        float halfFov = fovYDegrees * 0.5f * (MathF.PI / 180f);
        float tanHalfFov = MathF.Tan(halfFov); // square icon — same for X and Y

        var worldUp = System.Numerics.Vector3.UnitY;
        var right = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Cross(worldUp, viewDir));
        if (!float.IsFinite(right.X) || !float.IsFinite(right.Y) || !float.IsFinite(right.Z) || right.LengthSquared() < 1e-6f)
            right = System.Numerics.Vector3.UnitX;
        var up = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Cross(viewDir, right));

        var center = (bounds.Min + bounds.Max) * 0.5f;
        float maxX = 0f;
        float maxY = 0f;
        float maxZ = 0f;
        foreach (var corner in GetBoundsCorners(bounds))
        {
            var offset = corner - center;
            maxX = MathF.Max(maxX, MathF.Abs(System.Numerics.Vector3.Dot(offset, right)));
            maxY = MathF.Max(maxY, MathF.Abs(System.Numerics.Vector3.Dot(offset, up)));
            maxZ = MathF.Max(maxZ, MathF.Abs(System.Numerics.Vector3.Dot(offset, viewDir)));
        }

        float tanFill = MathF.Max(1e-4f, tanHalfFov * targetFill);
        float fitDistance = MathF.Max(maxX / tanFill, maxY / tanFill) + maxZ;
        return MathF.Max(0.25f, fitDistance);
    }

    private static IEnumerable<System.Numerics.Vector3> GetBoundsCorners(BoundingBox b)
    {
        yield return new System.Numerics.Vector3(b.Min.X, b.Min.Y, b.Min.Z);
        yield return new System.Numerics.Vector3(b.Max.X, b.Min.Y, b.Min.Z);
        yield return new System.Numerics.Vector3(b.Min.X, b.Max.Y, b.Min.Z);
        yield return new System.Numerics.Vector3(b.Max.X, b.Max.Y, b.Min.Z);
        yield return new System.Numerics.Vector3(b.Min.X, b.Min.Y, b.Max.Z);
        yield return new System.Numerics.Vector3(b.Max.X, b.Min.Y, b.Max.Z);
        yield return new System.Numerics.Vector3(b.Min.X, b.Max.Y, b.Max.Z);
        yield return new System.Numerics.Vector3(b.Max.X, b.Max.Y, b.Max.Z);
    }

    private bool GeneratePrefabIcon(SceneRenderer renderer, string prefabAssetPath, string outputPath)
    {
        EnsureRenderTarget();
        if (!_hasRenderTarget)
            return false;

        var prefab = PrefabDatabase.Instance.Load(prefabAssetPath, resolveVariants: true);
        if (prefab?.Root == null)
            return false;

        var scene = new Scene { Name = "AssetIconPreview" };
        try
        {
            InstantiatePrefabNode(prefab.Root, scene, parent: null);

            var bounds = ComputeSceneBounds(scene);
            if (!bounds.HasValue)
                return false;

            return RenderPreviewScene(renderer, scene, bounds.Value, outputPath);
        }
        finally
        {
            scene.Dispose();
        }
    }

    private static void InstantiatePrefabNode(PrefabNodeData node, Scene scene, TransformComponent? parent)
    {
        var entity = new Entity(node.Name) { Active = node.Active };
        foreach (var component in node.Components)
            PrefabSerializer.ApplyComponentData(entity, component);
        scene.AddEntity(entity);
        if (parent != null)
            entity.Transform.SetParent(parent);
        foreach (var child in node.Children)
            InstantiatePrefabNode(child, scene, entity.Transform);
    }

    private static BoundingBox? ComputeSceneBounds(Scene scene)
    {
        System.Numerics.Vector3? unionMin = null;
        System.Numerics.Vector3? unionMax = null;

        foreach (var renderable in scene.Renderables)
        {
            var bb = renderable.GetWorldBoundingBox();
            if (!bb.HasValue)
                continue;

            if (unionMin.HasValue)
            {
                unionMin = System.Numerics.Vector3.Min(unionMin.Value, bb.Value.Min);
                unionMax = System.Numerics.Vector3.Max(unionMax!.Value, bb.Value.Max);
            }
            else
            {
                unionMin = bb.Value.Min;
                unionMax = bb.Value.Max;
            }
        }

        if (!unionMin.HasValue)
            return null;

        return new BoundingBox(unionMin.Value, unionMax!.Value);
    }

    private bool ExportRenderTarget(string outputPath)
    {
        var image = Raylib.LoadImageFromTexture(_renderTarget.Texture);
        try
        {
            Raylib.ImageFlipVertical(ref image);
            return Raylib.ExportImage(image, outputPath);
        }
        finally
        {
            Raylib.UnloadImage(image);
        }
    }

    public static bool IsSupportedType(AssetType type)
    {
        return type is AssetType.Texture or AssetType.Model or AssetType.Prefab;
    }

    private static string BuildAssetKey(in AssetEntry asset)
    {
        return asset.IsEngineAsset
            ? BuildEngineKey(asset.RelativePath)
            : BuildProjectKey(asset.RelativePath);
    }

    private static string BuildProjectKey(string relativePath)
    {
        return relativePath.Replace('\\', '/');
    }

    private static string BuildEngineKey(string relativePath)
    {
        return AssetReference.EnginePrefix + relativePath.Replace('\\', '/');
    }

    private static string ComputeSourceHash(AssetType type, string absolutePath)
    {
        using var stream = File.OpenRead(absolutePath);
        var hash = SHA256.HashData(stream);
        return $"{CacheVersion}:{type}:{Convert.ToHexString(hash)}";
    }

    private static string GetIconFileName(string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var hash = SHA1.HashData(keyBytes);
        return $"{Convert.ToHexString(hash).ToLowerInvariant()}.png";
    }

    private void LoadManifest()
    {
        _manifest = new Dictionary<string, ManifestEntry>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(_manifestPath) || !File.Exists(_manifestPath))
            return;

        try
        {
            var json = File.ReadAllText(_manifestPath);
            var manifest = JsonSerializer.Deserialize<ManifestModel>(json);
            if (manifest == null || manifest.Version != CacheVersion || manifest.Entries == null)
                return;

            _manifest = new Dictionary<string, ManifestEntry>(manifest.Entries, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to load asset icon manifest: {ex.Message}");
            _manifest = new Dictionary<string, ManifestEntry>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void FlushManifestIfNeeded(bool force)
    {
        if (!_manifestDirty || string.IsNullOrWhiteSpace(_manifestPath))
            return;

        if (!force && _pendingSaveCounter < 8)
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath)!);
        var model = new ManifestModel
        {
            Version = CacheVersion,
            Entries = _manifest
        };
        var json = JsonSerializer.Serialize(model, _manifestJsonOptions);
        File.WriteAllText(_manifestPath, json);
        _manifestDirty = false;
        _pendingSaveCounter = 0;
    }

    private sealed class ManifestModel
    {
        public int Version { get; set; }
        public Dictionary<string, ManifestEntry> Entries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class ManifestEntry
    {
        public string SourceHash { get; set; } = string.Empty;
        public string IconFile { get; set; } = string.Empty;
        public AssetType AssetType { get; set; }
    }
}
