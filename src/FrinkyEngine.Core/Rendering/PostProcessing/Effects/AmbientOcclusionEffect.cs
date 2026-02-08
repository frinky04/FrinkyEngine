using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering.PostProcessing.Effects;

/// <summary>
/// Screen-space ambient occlusion (SSAO) effect that darkens creases and corners.
/// Uses hemisphere sampling with a noise texture and bilateral blur for smooth results.
/// </summary>
public class AmbientOcclusionEffect : PostProcessEffect
{
    /// <inheritdoc/>
    public override string DisplayName => "Ambient Occlusion";

    /// <inheritdoc/>
    public override bool NeedsDepth => true;

    /// <summary>
    /// Radius of the occlusion hemisphere in world units.
    /// </summary>
    public float Radius { get; set; } = 0.5f;

    /// <summary>
    /// Strength of the occlusion darkening.
    /// </summary>
    public float Intensity { get; set; } = 1.0f;

    /// <summary>
    /// Depth bias to prevent self-occlusion artifacts.
    /// </summary>
    public float Bias { get; set; } = 0.025f;

    /// <summary>
    /// Number of hemisphere samples per pixel (max 64).
    /// </summary>
    public int SampleCount { get; set; } = 16;

    /// <summary>
    /// Size of the bilateral blur kernel (half-extent in pixels).
    /// </summary>
    public int BlurSize { get; set; } = 2;

    private Shader _ssaoShader;
    private Shader _blurShader;
    private Shader _compositeShader;
    private Texture2D _noiseTexture;

    private int _ssaoDepthTexLoc = -1;
    private int _ssaoNoiseTexLoc = -1;
    private int _ssaoScreenSizeLoc = -1;
    private int _ssaoRadiusLoc = -1;
    private int _ssaoBiasLoc = -1;
    private int _ssaoIntensityLoc = -1;
    private int _ssaoSampleCountLoc = -1;
    private int _ssaoNearPlaneLoc = -1;
    private int _ssaoFarPlaneLoc = -1;
    private int _ssaoSamplesLoc = -1;

    private int _blurTexelSizeLoc = -1;
    private int _blurSizeLoc = -1;

    private int _compositeAoTexLoc = -1;

    private float[] _sampleKernel = Array.Empty<float>();

    /// <inheritdoc/>
    public override void Initialize(string shaderBasePath)
    {
        var vsPath = Path.Combine(shaderBasePath, "postprocess.vs");

        _ssaoShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "ssao.fs"));
        _blurShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "ssao_blur.fs"));
        _compositeShader = Raylib.LoadShader(vsPath, Path.Combine(shaderBasePath, "ssao_composite.fs"));

        if (_ssaoShader.Id == 0 || _blurShader.Id == 0 || _compositeShader.Id == 0)
            return;

        _ssaoDepthTexLoc = Raylib.GetShaderLocation(_ssaoShader, "depthTex");
        _ssaoNoiseTexLoc = Raylib.GetShaderLocation(_ssaoShader, "noiseTex");
        _ssaoScreenSizeLoc = Raylib.GetShaderLocation(_ssaoShader, "screenSize");
        _ssaoRadiusLoc = Raylib.GetShaderLocation(_ssaoShader, "radius");
        _ssaoBiasLoc = Raylib.GetShaderLocation(_ssaoShader, "bias");
        _ssaoIntensityLoc = Raylib.GetShaderLocation(_ssaoShader, "intensity");
        _ssaoSampleCountLoc = Raylib.GetShaderLocation(_ssaoShader, "sampleCount");
        _ssaoNearPlaneLoc = Raylib.GetShaderLocation(_ssaoShader, "nearPlane");
        _ssaoFarPlaneLoc = Raylib.GetShaderLocation(_ssaoShader, "farPlane");
        _ssaoSamplesLoc = Raylib.GetShaderLocation(_ssaoShader, "samples");

        _blurTexelSizeLoc = Raylib.GetShaderLocation(_blurShader, "texelSize");
        _blurSizeLoc = Raylib.GetShaderLocation(_blurShader, "blurSize");

        _compositeAoTexLoc = Raylib.GetShaderLocation(_compositeShader, "aoTex");

        GenerateSampleKernel(64);
        GenerateNoiseTexture();

        IsInitialized = true;
    }

    private void GenerateSampleKernel(int count)
    {
        var rng = new Random(42); // deterministic for consistency
        _sampleKernel = new float[count * 3];

        for (int i = 0; i < count; i++)
        {
            // Random direction in hemisphere (z > 0)
            var sample = new Vector3(
                (float)(rng.NextDouble() * 2.0 - 1.0),
                (float)(rng.NextDouble() * 2.0 - 1.0),
                (float)rng.NextDouble());

            sample = Vector3.Normalize(sample);
            sample *= (float)rng.NextDouble();

            // Accelerating interpolation: more samples near the center
            float scale = (float)i / count;
            scale = 0.1f + scale * scale * 0.9f;
            sample *= scale;

            _sampleKernel[i * 3 + 0] = sample.X;
            _sampleKernel[i * 3 + 1] = sample.Y;
            _sampleKernel[i * 3 + 2] = sample.Z;
        }
    }

    private unsafe void GenerateNoiseTexture()
    {
        var rng = new Random(123);
        var noiseData = new byte[4 * 4 * 4]; // 4x4 RGBA

        for (int i = 0; i < 16; i++)
        {
            noiseData[i * 4 + 0] = (byte)(rng.NextDouble() * 255); // Random rotation vector
            noiseData[i * 4 + 1] = (byte)(rng.NextDouble() * 255);
            noiseData[i * 4 + 2] = 128; // Z always pointing up-ish
            noiseData[i * 4 + 3] = 255;
        }

        var image = Raylib.GenImageColor(4, 4, Color.Black);
        fixed (byte* ptr = noiseData)
        {
            var srcSpan = new ReadOnlySpan<byte>(ptr, noiseData.Length);
            var dstSpan = new Span<byte>((void*)image.Data, noiseData.Length);
            srcSpan.CopyTo(dstSpan);
        }

        _noiseTexture = Raylib.LoadTextureFromImage(image);
        Raylib.SetTextureFilter(_noiseTexture, TextureFilter.Point);
        Raylib.SetTextureWrap(_noiseTexture, TextureWrap.Repeat);
        Raylib.UnloadImage(image);
    }

    /// <inheritdoc/>
    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        if (!IsInitialized) return;

        int sampleCount = Math.Clamp(SampleCount, 4, 64);

        // Pass 1: SSAO
        float[] screenSize = { context.Width, context.Height };
        if (_ssaoScreenSizeLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoScreenSizeLoc, screenSize, ShaderUniformDataType.Vec2);
        if (_ssaoRadiusLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoRadiusLoc, Radius, ShaderUniformDataType.Float);
        if (_ssaoBiasLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoBiasLoc, Bias, ShaderUniformDataType.Float);
        if (_ssaoIntensityLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoIntensityLoc, Intensity, ShaderUniformDataType.Float);
        if (_ssaoSampleCountLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoSampleCountLoc, sampleCount, ShaderUniformDataType.Int);
        if (_ssaoNearPlaneLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoNearPlaneLoc, context.NearPlane, ShaderUniformDataType.Float);
        if (_ssaoFarPlaneLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoFarPlaneLoc, context.FarPlane, ShaderUniformDataType.Float);

        // Upload sample kernel
        if (_ssaoSamplesLoc >= 0)
            Raylib.SetShaderValue(_ssaoShader, _ssaoSamplesLoc, _sampleKernel, ShaderUniformDataType.Vec3);

        // Bind depth texture to slot 1
        if (_ssaoDepthTexLoc >= 0)
        {
            Raylib.SetShaderValue(_ssaoShader, _ssaoDepthTexLoc, 1, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(1);
            Rlgl.EnableTexture(context.DepthTexture.Id);
        }

        // Bind noise texture to slot 2
        if (_ssaoNoiseTexLoc >= 0)
        {
            Raylib.SetShaderValue(_ssaoShader, _ssaoNoiseTexLoc, 2, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(2);
            Rlgl.EnableTexture(_noiseTexture.Id);
        }

        var ssaoRT = context.GetTemporaryRT();
        PostProcessContext.Blit(source, ssaoRT, _ssaoShader);

        // Unbind extra textures
        Rlgl.ActiveTextureSlot(2);
        Rlgl.DisableTexture();
        Rlgl.ActiveTextureSlot(1);
        Rlgl.DisableTexture();
        Rlgl.ActiveTextureSlot(0);

        // Pass 2: Bilateral blur
        float[] texelSize = { 1.0f / context.Width, 1.0f / context.Height };
        if (_blurTexelSizeLoc >= 0)
            Raylib.SetShaderValue(_blurShader, _blurTexelSizeLoc, texelSize, ShaderUniformDataType.Vec2);
        if (_blurSizeLoc >= 0)
            Raylib.SetShaderValue(_blurShader, _blurSizeLoc, BlurSize, ShaderUniformDataType.Int);

        var blurredRT = context.GetTemporaryRT();
        PostProcessContext.Blit(ssaoRT.Texture, blurredRT, _blurShader);

        // Pass 3: Multiply composite
        if (_compositeAoTexLoc >= 0)
        {
            Raylib.SetShaderValue(_compositeShader, _compositeAoTexLoc, 1, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(1);
            Rlgl.EnableTexture(blurredRT.Texture.Id);
        }

        PostProcessContext.Blit(source, destination, _compositeShader);

        if (_compositeAoTexLoc >= 0)
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

        Raylib.UnloadShader(_ssaoShader);
        Raylib.UnloadShader(_blurShader);
        Raylib.UnloadShader(_compositeShader);
        if (_noiseTexture.Id != 0)
            Raylib.UnloadTexture(_noiseTexture);
        IsInitialized = false;
    }
}
