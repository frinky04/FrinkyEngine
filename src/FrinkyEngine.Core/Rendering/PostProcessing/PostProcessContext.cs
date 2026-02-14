using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing;

/// <summary>
/// Per-frame data passed to post-processing effects during rendering.
/// Provides viewport dimensions, depth texture access, camera parameters, and temporary RT allocation.
/// </summary>
public class PostProcessContext
{
    /// <summary>
    /// Current viewport width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Current viewport height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Linear depth texture (R channel = normalized linear depth 0..1). Only valid when an effect requests depth.
    /// </summary>
    public Texture2D DepthTexture { get; set; }

    /// <summary>
    /// Camera near plane distance.
    /// </summary>
    public float NearPlane { get; set; }

    /// <summary>
    /// Camera far plane distance.
    /// </summary>
    public float FarPlane { get; set; }

    /// <summary>
    /// World-space camera position.
    /// </summary>
    public Vector3 CameraPosition { get; set; }

    /// <summary>
    /// Camera vertical field of view in degrees.
    /// </summary>
    public float FieldOfViewDegrees { get; set; }

    /// <summary>
    /// Viewport aspect ratio (width / height).
    /// </summary>
    public float AspectRatio { get; set; }

    private readonly List<RenderTexture2D> _tempRTs = new();
    private readonly Dictionary<(int w, int h), Queue<RenderTexture2D>> _pool = new();

    /// <summary>
    /// Allocates a temporary render texture from the pool. Released automatically after each effect.
    /// Reuses pooled RTs of the same size to avoid per-frame GPU framebuffer allocation.
    /// </summary>
    /// <param name="width">Texture width (defaults to viewport width).</param>
    /// <param name="height">Texture height (defaults to viewport height).</param>
    /// <returns>A temporary render texture.</returns>
    public RenderTexture2D GetTemporaryRT(int width = 0, int height = 0)
    {
        if (width <= 0) width = Width;
        if (height <= 0) height = Height;

        var key = (width, height);
        RenderTexture2D rt;
        if (_pool.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            rt = queue.Dequeue();
        }
        else
        {
            rt = Raylib.LoadRenderTexture(width, height);
            Raylib.SetTextureFilter(rt.Texture, TextureFilter.Bilinear);
            Raylib.SetTextureWrap(rt.Texture, TextureWrap.Clamp);
        }

        _tempRTs.Add(rt);
        return rt;
    }

    /// <summary>
    /// Returns all temporary render textures allocated during the current effect back to the pool.
    /// Called automatically by the pipeline after each effect's <see cref="PostProcessEffect.Render"/>.
    /// </summary>
    public void ReleaseTemporaryRTs()
    {
        foreach (var rt in _tempRTs)
        {
            var key = (rt.Texture.Width, rt.Texture.Height);
            if (!_pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<RenderTexture2D>();
                _pool[key] = queue;
            }
            queue.Enqueue(rt);
        }
        _tempRTs.Clear();
    }

    /// <summary>
    /// Unloads all pooled render textures, freeing GPU resources.
    /// Called on viewport resize and pipeline shutdown.
    /// </summary>
    public void DisposePool()
    {
        foreach (var queue in _pool.Values)
        {
            foreach (var rt in queue)
                Raylib.UnloadRenderTexture(rt);
        }
        _pool.Clear();
    }

    /// <summary>
    /// Performs a fullscreen blit from <paramref name="source"/> into <paramref name="dest"/>,
    /// optionally applying <paramref name="shader"/>.
    /// </summary>
    /// <param name="source">Source color texture.</param>
    /// <param name="dest">Destination render texture.</param>
    /// <param name="shader">Optional shader to apply. Pass <c>null</c> for a simple copy.</param>
    public static void Blit(Texture2D source, RenderTexture2D dest, Shader? shader = null)
    {
        Raylib.BeginTextureMode(dest);
        Raylib.ClearBackground(new Color(0, 0, 0, 255));

        if (shader.HasValue)
            Raylib.BeginShaderMode(shader.Value);

        var src = new Rectangle(0, 0, source.Width, -source.Height);
        var dst = new Rectangle(0, 0, dest.Texture.Width, dest.Texture.Height);
        Raylib.DrawTexturePro(source, src, dst, System.Numerics.Vector2.Zero, 0f, Color.White);

        if (shader.HasValue)
            Raylib.EndShaderMode();

        Raylib.EndTextureMode();
    }
}
