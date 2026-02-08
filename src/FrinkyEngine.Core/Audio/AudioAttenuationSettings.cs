using System.Numerics;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Distance and panning settings used for spatialized audio playback.
/// </summary>
public struct AudioAttenuationSettings
{
    /// <summary>
    /// Creates attenuation settings with engine defaults.
    /// </summary>
    public AudioAttenuationSettings()
    {
        MinDistance = 1f;
        MaxDistance = 50f;
        Rolloff = AudioRolloffMode.Logarithmic;
        SpatialBlend = 1f;
        PanStereo = 0f;
    }

    /// <summary>
    /// Default attenuation for 2D playback.
    /// </summary>
    public static AudioAttenuationSettings Default2D => new()
    {
        MinDistance = 1f,
        MaxDistance = 50f,
        Rolloff = AudioRolloffMode.Logarithmic,
        SpatialBlend = 0f,
        PanStereo = 0f
    };

    /// <summary>
    /// Default attenuation for 3D playback.
    /// </summary>
    public static AudioAttenuationSettings Default3D => new()
    {
        MinDistance = 1f,
        MaxDistance = 50f,
        Rolloff = AudioRolloffMode.Logarithmic,
        SpatialBlend = 1f,
        PanStereo = 0f
    };

    /// <summary>
    /// Distance where attenuation begins.
    /// </summary>
    public float MinDistance { get; set; } = 1f;

    /// <summary>
    /// Distance where attenuation reaches silence.
    /// </summary>
    public float MaxDistance { get; set; } = 50f;

    /// <summary>
    /// Rolloff curve mode.
    /// </summary>
    public AudioRolloffMode Rolloff { get; set; } = AudioRolloffMode.Logarithmic;

    /// <summary>
    /// 0 = 2D pan only, 1 = fully 3D spatialized.
    /// </summary>
    public float SpatialBlend { get; set; } = 1f;

    /// <summary>
    /// Stereo pan for 2D playback, from -1 (left) to +1 (right).
    /// </summary>
    public float PanStereo { get; set; } = 0f;

    /// <summary>
    /// Clamps values to safe ranges.
    /// </summary>
    public void Normalize()
    {
        if (!float.IsFinite(MinDistance) || MinDistance < 0f)
            MinDistance = 1f;
        if (!float.IsFinite(MaxDistance) || MaxDistance <= 0f)
            MaxDistance = 50f;
        if (MaxDistance < MinDistance)
            MaxDistance = MinDistance;
        if (!float.IsFinite(SpatialBlend))
            SpatialBlend = 1f;
        if (!float.IsFinite(PanStereo))
            PanStereo = 0f;

        SpatialBlend = Math.Clamp(SpatialBlend, 0f, 1f);
        PanStereo = Math.Clamp(PanStereo, -1f, 1f);
    }

    /// <summary>
    /// Computes distance-based gain from 0..1.
    /// </summary>
    /// <param name="distance">Distance from listener to source.</param>
    /// <returns>Gain multiplier in range 0..1.</returns>
    public float EvaluateVolume(float distance)
    {
        var settings = this;
        settings.Normalize();

        if (!float.IsFinite(distance))
            return 0f;
        if (distance <= settings.MinDistance)
            return 1f;
        if (distance >= settings.MaxDistance)
            return 0f;

        var range = MathF.Max(settings.MaxDistance - settings.MinDistance, 0.0001f);
        var t = Math.Clamp((distance - settings.MinDistance) / range, 0f, 1f);

        return settings.Rolloff switch
        {
            AudioRolloffMode.Linear => 1f - t,
            AudioRolloffMode.Logarithmic => Math.Clamp(1f - MathF.Log10(1f + 9f * t), 0f, 1f),
            AudioRolloffMode.CustomCurve => 1f - t,
            _ => 1f - t
        };
    }

    /// <summary>
    /// Computes stereo pan from listener/source transforms.
    /// </summary>
    /// <param name="listenerPosition">Listener world position.</param>
    /// <param name="listenerRight">Listener right unit axis.</param>
    /// <param name="sourcePosition">Source world position.</param>
    /// <returns>Pan in range -1..1.</returns>
    public float EvaluatePan(Vector3 listenerPosition, Vector3 listenerRight, Vector3 sourcePosition)
    {
        var settings = this;
        settings.Normalize();

        var right = SafeNormalize(listenerRight, Vector3.UnitX);
        var toSource = sourcePosition - listenerPosition;
        var toSourceFlat = new Vector3(toSource.X, 0f, toSource.Z);
        var rightFlat = new Vector3(right.X, 0f, right.Z);
        var toSourceNorm = SafeNormalize(toSourceFlat, Vector3.Zero);
        var rightNorm = SafeNormalize(rightFlat, Vector3.UnitX);

        var spatialPan = toSourceNorm == Vector3.Zero
            ? 0f
            : Math.Clamp(-Vector3.Dot(rightNorm, toSourceNorm), -1f, 1f);

        // Blend between authored stereo pan and spatial pan.
        return Math.Clamp(settings.PanStereo + (spatialPan - settings.PanStereo) * settings.SpatialBlend, -1f, 1f);
    }

    private static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
    {
        var lenSq = v.LengthSquared();
        if (lenSq <= 1e-12f)
            return fallback;
        return v / MathF.Sqrt(lenSq);
    }
}
