using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing;

/// <summary>
/// Abstract base class for all post-processing effects.
/// Subclass this to create custom effects — public read/write properties are auto-serialized and drawn in the inspector.
/// </summary>
public abstract class PostProcessEffect
{
    /// <summary>
    /// Human-readable name shown in the editor UI.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Whether this effect is active. Disabled effects are skipped during rendering.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Override to return <c>true</c> if this effect requires the depth texture (e.g. fog, SSAO).
    /// </summary>
    public virtual bool NeedsDepth => false;

    /// <summary>
    /// Indicates whether <see cref="Initialize"/> has been called successfully.
    /// </summary>
    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// Called once before the first render. Load shaders and other GPU resources here.
    /// </summary>
    /// <param name="shaderBasePath">Directory containing engine post-processing shaders.</param>
    public virtual void Initialize(string shaderBasePath) { }

    /// <summary>
    /// Renders this effect from <paramref name="source"/> into <paramref name="destination"/>.
    /// </summary>
    /// <param name="source">The input color texture.</param>
    /// <param name="destination">The render target to write to.</param>
    /// <param name="context">Per-frame context data (viewport size, depth texture, temp RT pool, etc.).</param>
    public abstract void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context);

    /// <summary>
    /// Called when this effect is removed or the pipeline shuts down. Unload GPU resources here.
    /// </summary>
    public virtual void Dispose() { }
}
