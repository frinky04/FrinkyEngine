using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing.Effects;

/// <summary>
/// Multi-pass bloom effect using progressive downsample/upsample (UE4/5-style).
/// Extracts bright pixels, blurs through a mip chain, then composites back onto the scene.
/// </summary>
public class BloomEffect : PostProcessEffect
{
    /// <inheritdoc/>
    public override string DisplayName => "Bloom";

    /// <summary>
    /// Brightness threshold above which pixels contribute to bloom.
    /// </summary>
    public float Threshold { get; set; } = 1.0f;

    /// <summary>
    /// Softness of the threshold transition (0 = hard, 1 = very soft).
    /// </summary>
    public float SoftKnee { get; set; } = 0.5f;

    /// <summary>
    /// Strength of the bloom added back to the scene.
    /// </summary>
    public float Intensity { get; set; } = 1.0f;

    /// <summary>
    /// Number of downsample/upsample iterations (more = wider bloom, but more cost).
    /// </summary>
    public int Iterations { get; set; } = 5;

    /// <summary>
    /// Controls how much bloom spreads across mip levels (0 = tight glow, 1 = wide cinematic bloom).
    /// Biases per-mip weights toward lower (larger) mips for a wider scatter.
    /// </summary>
    public float Scatter { get; set; } = 0.7f;

    private Shader _thresholdShader;
    private Shader _downsampleShader;
    private Shader _upsampleShader;
    private Shader _compositeShader;

    private int _thresholdLoc = -1;
    private int _softKneeLoc = -1;
    private int _downsampleTexelSizeLoc = -1;
    private int _upsampleTexelSizeLoc = -1;
    private int _upsampleWeightLoc = -1;
    private int _compositeIntensityLoc = -1;
    private int _compositeBloomTexLoc = -1;

    /// <inheritdoc/>
    public override void Initialize(string shaderBasePath)
    {
        var vsPath = Path.Combine(shaderBasePath, "postprocess.vs");

        _thresholdShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "bloom_threshold.fs"));
        _downsampleShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "bloom_downsample.fs"));
        _upsampleShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "bloom_upsample.fs"));
        _compositeShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "bloom_composite.fs"));

        if (_thresholdShader.Id == 0 || _downsampleShader.Id == 0 ||
            _upsampleShader.Id == 0 || _compositeShader.Id == 0)
            return;

        _thresholdLoc = Raylib.GetShaderLocation(_thresholdShader, "threshold");
        _softKneeLoc = Raylib.GetShaderLocation(_thresholdShader, "softKnee");
        _downsampleTexelSizeLoc = Raylib.GetShaderLocation(_downsampleShader, "texelSize");
        _upsampleTexelSizeLoc = Raylib.GetShaderLocation(_upsampleShader, "texelSize");
        _upsampleWeightLoc = Raylib.GetShaderLocation(_upsampleShader, "weight");
        _compositeIntensityLoc = Raylib.GetShaderLocation(_compositeShader, "intensity");
        _compositeBloomTexLoc = Raylib.GetShaderLocation(_compositeShader, "bloomTex");

        IsInitialized = true;
    }

    /// <inheritdoc/>
    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        if (!IsInitialized) return;

        int iterations = Math.Clamp(Iterations, 1, 8);
        float scatter = Math.Clamp(Scatter, 0f, 1f);
        var mipChain = new RenderTexture2D[iterations];

        // Threshold pass — extract bright pixels into first half-res mip
        if (_thresholdLoc >= 0)
            Raylib.SetShaderValue(_thresholdShader, _thresholdLoc, Threshold, ShaderUniformDataType.Float);
        if (_softKneeLoc >= 0)
            Raylib.SetShaderValue(_thresholdShader, _softKneeLoc, SoftKnee, ShaderUniformDataType.Float);

        int mipW = context.Width / 2;
        int mipH = context.Height / 2;
        mipChain[0] = context.GetTemporaryRT(mipW, mipH);
        PostProcessContext.Blit(source, mipChain[0], _thresholdShader);

        // Progressive downsample through mip chain
        for (int i = 1; i < iterations; i++)
        {
            mipW = Math.Max(1, mipW / 2);
            mipH = Math.Max(1, mipH / 2);
            mipChain[i] = context.GetTemporaryRT(mipW, mipH);

            float[] texelSize = { 1.0f / mipChain[i - 1].Texture.Width, 1.0f / mipChain[i - 1].Texture.Height };
            if (_downsampleTexelSizeLoc >= 0)
                Raylib.SetShaderValue(_downsampleShader, _downsampleTexelSizeLoc, texelSize, ShaderUniformDataType.Vec2);

            PostProcessContext.Blit(mipChain[i - 1].Texture, mipChain[i], _downsampleShader);
        }

        // Progressive upsample — blend each mip level back up with per-mip scatter weighting.
        // Higher scatter biases toward lower (wider) mips for a broader bloom.
        for (int i = iterations - 2; i >= 0; i--)
        {
            float[] texelSize = { 1.0f / mipChain[i + 1].Texture.Width, 1.0f / mipChain[i + 1].Texture.Height };
            if (_upsampleTexelSizeLoc >= 0)
                Raylib.SetShaderValue(_upsampleShader, _upsampleTexelSizeLoc, texelSize, ShaderUniformDataType.Vec2);

            // Per-mip weight: lerp between equal weighting and bottom-heavy weighting based on scatter.
            // mipNorm=0 at top (full res), 1 at bottom (smallest mip).
            float mipNorm = (iterations > 1) ? (float)i / (iterations - 1) : 0f;
            float weight = MathF.Pow(1f - mipNorm, 1f - scatter);
            if (_upsampleWeightLoc >= 0)
                Raylib.SetShaderValue(_upsampleShader, _upsampleWeightLoc, weight, ShaderUniformDataType.Float);

            var tempRT = context.GetTemporaryRT(mipChain[i].Texture.Width, mipChain[i].Texture.Height);

            // Copy current mip into temp
            PostProcessContext.Blit(mipChain[i].Texture, tempRT);

            // Additively blend the upsampled lower mip on top
            Raylib.BeginTextureMode(tempRT);
            Raylib.BeginShaderMode(_upsampleShader);
            Rlgl.SetBlendMode(BlendMode.Additive);

            var src = new Rectangle(0, 0, mipChain[i + 1].Texture.Width, -mipChain[i + 1].Texture.Height);
            var dst = new Rectangle(0, 0, tempRT.Texture.Width, tempRT.Texture.Height);
            Raylib.DrawTexturePro(mipChain[i + 1].Texture, src, dst, System.Numerics.Vector2.Zero, 0f, Color.White);

            Rlgl.SetBlendMode(BlendMode.Alpha);
            Raylib.EndShaderMode();
            Raylib.EndTextureMode();

            mipChain[i] = tempRT;
        }

        // Composite: add bloom back onto the original scene
        if (_compositeIntensityLoc >= 0)
            Raylib.SetShaderValue(_compositeShader, _compositeIntensityLoc, Intensity, ShaderUniformDataType.Float);

        if (_compositeBloomTexLoc >= 0)
        {
            Raylib.SetShaderValue(_compositeShader, _compositeBloomTexLoc, 1, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(1);
            Rlgl.EnableTexture(mipChain[0].Texture.Id);
        }

        PostProcessContext.Blit(source, destination, _compositeShader);

        if (_compositeBloomTexLoc >= 0)
        {
            Rlgl.ActiveTextureSlot(1);
            Rlgl.DisableTexture();
            Rlgl.ActiveTextureSlot(0);
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (!IsInitialized) return;

        Raylib.UnloadShader(_thresholdShader);
        Raylib.UnloadShader(_downsampleShader);
        Raylib.UnloadShader(_upsampleShader);
        Raylib.UnloadShader(_compositeShader);
        IsInitialized = false;
    }
}
