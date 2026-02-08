using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering.PostProcessing;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Attach to a camera entity to define an ordered stack of post-processing effects.
/// Effects are applied in list order after the scene is rendered.
/// </summary>
[ComponentCategory("Rendering")]
[ComponentDisplayName("Post Process Stack")]
public class PostProcessStackComponent : Component
{
    /// <summary>
    /// Master toggle for all post-processing on this camera.
    /// </summary>
    public bool PostProcessingEnabled { get; set; } = true;

    /// <summary>
    /// Ordered list of post-processing effects to apply.
    /// </summary>
    public List<PostProcessEffect> Effects { get; set; } = new();

    /// <inheritdoc/>
    public override void OnDestroy()
    {
        foreach (var effect in Effects)
        {
            try { effect.Dispose(); }
            catch { /* best effort cleanup */ }
        }
    }
}
