using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing.Effects;

/// <summary>
/// Distance-based fog effect that blends scene color toward a fog color based on depth.
/// Supports linear, exponential, and exponential-squared falloff modes.
/// </summary>
public class FogEffect : PostProcessEffect
{
    /// <inheritdoc/>
    public override string DisplayName => "Fog";

    /// <inheritdoc/>
    public override bool NeedsDepth => true;

    /// <summary>
    /// The fog color.
    /// </summary>
    public Color FogColor { get; set; } = new(180, 190, 200, 255);

    /// <summary>
    /// Distance at which linear fog starts (world units).
    /// </summary>
    public float FogStart { get; set; } = 10f;

    /// <summary>
    /// Distance at which linear fog reaches full density (world units).
    /// </summary>
    public float FogEnd { get; set; } = 100f;

    /// <summary>
    /// Density factor for exponential fog modes.
    /// </summary>
    public float Density { get; set; } = 0.02f;

    /// <summary>
    /// Fog falloff mode: Linear, Exponential, or ExponentialSquared.
    /// </summary>
    public FogMode Mode { get; set; } = FogMode.Linear;

    private Shader _shader;
    private int _fogColorLoc = -1;
    private int _fogStartLoc = -1;
    private int _fogEndLoc = -1;
    private int _fogDensityLoc = -1;
    private int _fogModeLoc = -1;
    private int _nearPlaneLoc = -1;
    private int _farPlaneLoc = -1;
    private int _depthTexLoc = -1;

    /// <inheritdoc/>
    public override void Initialize(string shaderBasePath)
    {
        var vsPath = Path.Combine(shaderBasePath, "postprocess.vs");
        var fsPath = Path.Combine(shaderBasePath, "fog.fs");

        _shader = Raylib.LoadShader(vsPath, fsPath);
        if (_shader.Id == 0) return;

        _fogColorLoc = Raylib.GetShaderLocation(_shader, "fogColor");
        _fogStartLoc = Raylib.GetShaderLocation(_shader, "fogStart");
        _fogEndLoc = Raylib.GetShaderLocation(_shader, "fogEnd");
        _fogDensityLoc = Raylib.GetShaderLocation(_shader, "fogDensity");
        _fogModeLoc = Raylib.GetShaderLocation(_shader, "fogMode");
        _nearPlaneLoc = Raylib.GetShaderLocation(_shader, "nearPlane");
        _farPlaneLoc = Raylib.GetShaderLocation(_shader, "farPlane");
        _depthTexLoc = Raylib.GetShaderLocation(_shader, "depthTex");

        IsInitialized = true;
    }

    /// <inheritdoc/>
    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        if (!IsInitialized) return;

        // Set uniforms
        float[] fogColor = { FogColor.R / 255f, FogColor.G / 255f, FogColor.B / 255f };
        if (_fogColorLoc >= 0)
            Raylib.SetShaderValue(_shader, _fogColorLoc, fogColor, ShaderUniformDataType.Vec3);
        if (_fogStartLoc >= 0)
            Raylib.SetShaderValue(_shader, _fogStartLoc, FogStart, ShaderUniformDataType.Float);
        if (_fogEndLoc >= 0)
            Raylib.SetShaderValue(_shader, _fogEndLoc, FogEnd, ShaderUniformDataType.Float);
        if (_fogDensityLoc >= 0)
            Raylib.SetShaderValue(_shader, _fogDensityLoc, Density, ShaderUniformDataType.Float);
        if (_fogModeLoc >= 0)
            Raylib.SetShaderValue(_shader, _fogModeLoc, (int)Mode, ShaderUniformDataType.Int);
        if (_nearPlaneLoc >= 0)
            Raylib.SetShaderValue(_shader, _nearPlaneLoc, context.NearPlane, ShaderUniformDataType.Float);
        if (_farPlaneLoc >= 0)
            Raylib.SetShaderValue(_shader, _farPlaneLoc, context.FarPlane, ShaderUniformDataType.Float);

        // Bind depth texture to slot 1
        if (_depthTexLoc >= 0)
        {
            Raylib.SetShaderValue(_shader, _depthTexLoc, 1, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(1);
            Rlgl.EnableTexture(context.DepthTexture.Id);
        }

        PostProcessContext.Blit(source, destination, _shader);

        if (_depthTexLoc >= 0)
        {
            Rlgl.ActiveTextureSlot(1);
            Rlgl.DisableTexture();
            Rlgl.ActiveTextureSlot(0);
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (IsInitialized)
        {
            Raylib.UnloadShader(_shader);
            IsInitialized = false;
        }
    }
}

/// <summary>
/// Fog falloff modes.
/// </summary>
public enum FogMode
{
    /// <summary>Linear fog between start and end distances.</summary>
    Linear,
    /// <summary>Exponential fog falloff.</summary>
    Exponential,
    /// <summary>Exponential-squared fog falloff.</summary>
    ExponentialSquared
}
