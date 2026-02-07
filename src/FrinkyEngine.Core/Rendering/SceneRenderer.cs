using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Renders scenes using a Forward+ tiled lighting pipeline with support for multiple light types.
/// </summary>
public class SceneRenderer
{
    private const int LightTexelsPerLight = 4;
    private const int PackedTextureMaxWidth = 1024;
    private const int ForwardPlusWarningLogIntervalFrames = 180;
    private const float MinPerspectiveDepth = 0.01f;

    private Shader _lightingShader;
    private bool _shaderLoaded;
    private Shader _selectionMaskShader;
    private bool _selectionMaskShaderLoaded;

    private int _ambientLoc = -1;
    private int _viewPosLoc = -1;
    private int _screenSizeLoc = -1;
    private int _tileCountLoc = -1;
    private int _tileSizeLoc = -1;
    private int _totalLightsLoc = -1;
    private int _lightDataTexLoc = -1;
    private int _tileHeaderTexLoc = -1;
    private int _tileIndexTexLoc = -1;
    private int _triplanarParamsTexLoc = -1;
    private int _lightDataTexSizeLoc = -1;
    private int _tileHeaderTexSizeLoc = -1;
    private int _tileIndexTexSizeLoc = -1;

    private ForwardPlusSettings _forwardPlusSettings = ForwardPlusSettings.Default;
    private int _viewportWidth;
    private int _viewportHeight;
    private int _tileCountX;
    private int _tileCountY;
    private int _tileCount;

    private Texture2D _lightDataTexture;
    private Texture2D _tileHeaderTexture;
    private Texture2D _tileIndexTexture;

    private int _lightDataEntries;
    private int _tileHeaderEntries;
    private int _tileIndexEntries;
    private int _lightDataTexWidth;
    private int _lightDataTexHeight;
    private int _tileHeaderTexWidth;
    private int _tileHeaderTexHeight;
    private int _tileIndexTexWidth;
    private int _tileIndexTexHeight;

    private float[] _lightDataBuffer = Array.Empty<float>();
    private float[] _tileHeaderBuffer = Array.Empty<float>();
    private float[] _tileIndexBuffer = Array.Empty<float>();
    private int[] _tileLightCounts = Array.Empty<int>();
    private float[] _tileLightScores = Array.Empty<float>();

    private readonly List<PackedLight> _frameLights = new();
    private readonly List<PointLightCandidate> _pointCandidates = new();
    private int _forwardPlusDroppedTileLights;
    private int _forwardPlusClippedLights;
    private int _frameCounter;
    private int _lastSceneLightCount;
    private int _lastVisibleLightCount;
    private int _lastSkylightCount;
    private int _lastDirectionalLightCount;
    private int _lastPointLightCount;
    private float _lastAverageLightsPerTile;
    private int _lastMaxLightsPerTile;
    private bool _lastStatsValid;

    /// <summary>
    /// Diagnostic statistics from the most recent Forward+ frame.
    /// </summary>
    public readonly record struct ForwardPlusFrameStats(
        bool Valid,
        int SceneLights,
        int VisibleLights,
        int Skylights,
        int DirectionalLights,
        int PointLights,
        int AssignedLights,
        int ClippedLights,
        int DroppedTileLinks,
        int TileSize,
        int TilesX,
        int TilesY,
        int MaxLights,
        int MaxLightsPerTile,
        float AverageLightsPerTile,
        int PeakLightsPerTile);

    /// <summary>
    /// Gets diagnostic statistics from the most recent Forward+ render pass.
    /// </summary>
    /// <returns>A snapshot of the current frame's lighting statistics.</returns>
    public ForwardPlusFrameStats GetForwardPlusFrameStats()
    {
        return new ForwardPlusFrameStats(
            _lastStatsValid,
            _lastSceneLightCount,
            _lastVisibleLightCount,
            _lastSkylightCount,
            _lastDirectionalLightCount,
            _lastPointLightCount,
            _frameLights.Count,
            _forwardPlusClippedLights,
            _forwardPlusDroppedTileLights,
            _forwardPlusSettings.TileSize,
            _tileCountX,
            _tileCountY,
            _forwardPlusSettings.MaxLights,
            _forwardPlusSettings.MaxLightsPerTile,
            _lastAverageLightsPerTile,
            _lastMaxLightsPerTile);
    }

    /// <summary>
    /// Applies new Forward+ configuration settings, reallocating tile buffers if needed.
    /// </summary>
    /// <param name="settings">The new settings to apply (values will be normalized/clamped).</param>
    public void ConfigureForwardPlus(ForwardPlusSettings settings)
    {
        var normalized = settings.Normalize();
        if (normalized == _forwardPlusSettings)
            return;

        _forwardPlusSettings = normalized;
        _tileCountX = 0;
        _tileCountY = 0;
        _tileCount = 0;
        _lightDataEntries = 0;
        _tileHeaderEntries = 0;
        _tileIndexEntries = 0;
    }

    /// <summary>
    /// Loads the lighting shader from vertex and fragment shader files.
    /// </summary>
    /// <param name="vsPath">Path to the vertex shader file.</param>
    /// <param name="fsPath">Path to the fragment shader file.</param>
    public void LoadShader(string vsPath, string fsPath)
    {
        _lightingShader = Raylib.LoadShader(vsPath, fsPath);
        _viewPosLoc = Raylib.GetShaderLocation(_lightingShader, "viewPos");
        _ambientLoc = Raylib.GetShaderLocation(_lightingShader, "ambient");
        _screenSizeLoc = Raylib.GetShaderLocation(_lightingShader, "screenSize");
        _tileCountLoc = Raylib.GetShaderLocation(_lightingShader, "tileCount");
        _tileSizeLoc = Raylib.GetShaderLocation(_lightingShader, "tileSize");
        _totalLightsLoc = Raylib.GetShaderLocation(_lightingShader, "totalLights");
        _lightDataTexLoc = Raylib.GetShaderLocation(_lightingShader, "lightDataTex");
        _tileHeaderTexLoc = Raylib.GetShaderLocation(_lightingShader, "tileHeaderTex");
        _tileIndexTexLoc = Raylib.GetShaderLocation(_lightingShader, "tileIndexTex");
        _triplanarParamsTexLoc = Raylib.GetShaderLocation(_lightingShader, "triplanarParamsTex");
        _lightDataTexSizeLoc = Raylib.GetShaderLocation(_lightingShader, "lightDataTexSize");
        _tileHeaderTexSizeLoc = Raylib.GetShaderLocation(_lightingShader, "tileHeaderTexSize");
        _tileIndexTexSizeLoc = Raylib.GetShaderLocation(_lightingShader, "tileIndexTexSize");

        // Map forward+ sampler uniforms to unused material map slots so DrawMesh
        // binds them reliably (SetShaderValueTexture uses activeTextureId which
        // gets cleared by DrawRenderBatchActive, causing texture unit mismatches).
        unsafe
        {
            _lightingShader.Locs[(int)ShaderLocationIndex.MapOcclusion] = _lightDataTexLoc;
            _lightingShader.Locs[(int)ShaderLocationIndex.MapEmission] = _tileHeaderTexLoc;
            _lightingShader.Locs[(int)ShaderLocationIndex.MapHeight] = _tileIndexTexLoc;
            _lightingShader.Locs[(int)ShaderLocationIndex.MapBrdf] = _triplanarParamsTexLoc;
        }

        float[] ambient = { 0.15f, 0.15f, 0.15f, 1.0f };
        Raylib.SetShaderValue(_lightingShader, _ambientLoc, ambient, ShaderUniformDataType.Vec4);

        _shaderLoaded = true;

        var shaderDir = Path.GetDirectoryName(vsPath) ?? "Shaders";
        var selectionMaskVsPath = Path.Combine(shaderDir, "selection_mask.vs");
        var selectionMaskFsPath = Path.Combine(shaderDir, "selection_mask.fs");

        if (File.Exists(selectionMaskVsPath) && File.Exists(selectionMaskFsPath))
        {
            _selectionMaskShader = Raylib.LoadShader(selectionMaskVsPath, selectionMaskFsPath);
            _selectionMaskShaderLoaded = true;
        }
    }

    /// <summary>
    /// Unloads all shaders and releases Forward+ GPU textures.
    /// </summary>
    public void UnloadShader()
    {
        ReleaseForwardPlusTextures();

        if (_shaderLoaded)
        {
            Raylib.UnloadShader(_lightingShader);
            _shaderLoaded = false;
        }

        if (_selectionMaskShaderLoaded)
        {
            Raylib.UnloadShader(_selectionMaskShader);
            _selectionMaskShaderLoaded = false;
        }
    }

    /// <summary>
    /// Renders the scene from the given camera, optionally into a render texture.
    /// </summary>
    /// <param name="scene">The scene to render.</param>
    /// <param name="camera">The camera viewpoint.</param>
    /// <param name="renderTarget">Optional render texture target (renders to screen if <c>null</c>).</param>
    /// <param name="postSceneRender">Optional callback invoked after 3D drawing but before EndMode3D.</param>
    /// <param name="isEditorMode">When <c>true</c>, editor-only objects and the grid are drawn.</param>
    public void Render(Scene.Scene scene, Camera3D camera, RenderTexture2D? renderTarget = null, Action? postSceneRender = null, bool isEditorMode = true)
    {
        if (renderTarget.HasValue)
            Raylib.BeginTextureMode(renderTarget.Value);

        var mainCam = scene.MainCamera;
        Color clearColor = mainCam != null ? mainCam.ClearColor : new Color(30, 30, 30, 255);
        Raylib.ClearBackground(clearColor);

        Raylib.BeginMode3D(camera);

        if (_shaderLoaded)
        {
            float[] cameraPos = { camera.Position.X, camera.Position.Y, camera.Position.Z };
            Raylib.SetShaderValue(_lightingShader, _viewPosLoc, cameraPos, ShaderUniformDataType.Vec3);

            var viewportWidth = renderTarget?.Texture.Width ?? Raylib.GetScreenWidth();
            var viewportHeight = renderTarget?.Texture.Height ?? Raylib.GetScreenHeight();
            UpdateForwardPlusData(scene, camera, viewportWidth, viewportHeight, isEditorMode);
            BindForwardPlusShaderData(viewportWidth, viewportHeight);
        }

        foreach (var renderable in scene.Renderables)
        {
            if (!renderable.Entity.Active) continue;
            if (!renderable.Enabled) continue;
            if (renderable.EditorOnly && !isEditorMode) continue;
            renderable.EnsureModelReady();
            if (!renderable.RenderModel.HasValue) continue;
            DrawModelWithShader(renderable.RenderModel.Value, renderable.Entity.Transform.WorldMatrix, renderable.Tint);
        }

        if (isEditorMode)
            DrawGrid(20, 1.0f);

        postSceneRender?.Invoke();

        Raylib.EndMode3D();

        if (renderTarget.HasValue)
            Raylib.EndTextureMode();
    }

    private void DrawModelWithShader(Model model, Matrix4x4 worldMatrix, Color tint)
    {
        if (_shaderLoaded)
        {
            unsafe
            {
                for (int i = 0; i < model.MaterialCount; i++)
                {
                    model.Materials[i].Shader = _lightingShader;
                    model.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Texture = _lightDataTexture;
                    model.Materials[i].Maps[(int)MaterialMapIndex.Emission].Texture = _tileHeaderTexture;
                    model.Materials[i].Maps[(int)MaterialMapIndex.Height].Texture = _tileIndexTexture;
                }
            }
        }

        model.Transform = Matrix4x4.Transpose(worldMatrix);
        Raylib.DrawModel(model, System.Numerics.Vector3.Zero, 1f, tint);
    }

    /// <summary>
    /// Renders a binary selection mask of the specified entities into a render texture, used for outline effects.
    /// </summary>
    /// <param name="scene">The scene containing the entities.</param>
    /// <param name="camera">The camera viewpoint.</param>
    /// <param name="selectedEntities">The entities to highlight.</param>
    /// <param name="renderTarget">The render texture to draw the mask into.</param>
    /// <param name="isEditorMode">When <c>true</c>, editor-only objects participate in depth testing.</param>
    public void RenderSelectionMask(
        Scene.Scene scene,
        Camera3D camera,
        IReadOnlyList<Entity> selectedEntities,
        RenderTexture2D renderTarget,
        bool isEditorMode = true)
    {
        if (!_selectionMaskShaderLoaded || selectedEntities.Count == 0)
        {
            Raylib.BeginTextureMode(renderTarget);
            Raylib.ClearBackground(new Color(0, 0, 0, 0));
            Raylib.EndTextureMode();
            return;
        }

        Raylib.BeginTextureMode(renderTarget);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));
        Raylib.BeginMode3D(camera);

        Rlgl.DrawRenderBatchActive();
        Rlgl.EnableDepthTest();
        Rlgl.EnableDepthMask();
        Rlgl.ColorMask(false, false, false, false);

        foreach (var renderable in scene.Renderables)
        {
            if (!renderable.Entity.Active) continue;
            if (!renderable.Enabled) continue;
            if (renderable.EditorOnly && !isEditorMode) continue;
            renderable.EnsureModelReady();
            if (!renderable.RenderModel.HasValue) continue;
            DrawModelWithShader(renderable.RenderModel.Value, renderable.Entity.Transform.WorldMatrix, renderable.Tint);
        }

        Rlgl.DrawRenderBatchActive();
        Rlgl.ColorMask(true, true, true, true);
        Rlgl.DisableDepthMask();

        foreach (var entity in selectedEntities)
        {
            if (!entity.Active)
                continue;

            var renderable = entity.GetComponent<RenderableComponent>();
            if (renderable == null || !renderable.Enabled)
                continue;

            renderable.EnsureModelReady();
            if (!renderable.RenderModel.HasValue)
                continue;

            var model = renderable.RenderModel.Value;

            unsafe
            {
                for (int i = 0; i < model.MaterialCount; i++)
                {
                    model.Materials[i].Shader = _selectionMaskShader;
                }
            }

            model.Transform = Matrix4x4.Transpose(entity.Transform.WorldMatrix);
            Raylib.DrawModel(model, Vector3.Zero, 1f, Color.White);
        }

        Rlgl.DrawRenderBatchActive();
        Rlgl.EnableDepthMask();
        Raylib.EndMode3D();
        Raylib.EndTextureMode();
    }

    private void UpdateForwardPlusData(Scene.Scene scene, Camera3D camera, int viewportWidth, int viewportHeight, bool isEditorMode)
    {
        EnsureForwardPlusResources(viewportWidth, viewportHeight);
        BuildFrameLights(scene, camera, isEditorMode);
        BuildTileLightLists(camera);
        PackLightBuffer();
        PackTileHeaderBuffer();
        UploadForwardPlusBuffers();
        MaybeLogForwardPlusWarnings();
    }

    private void BindForwardPlusShaderData(int viewportWidth, int viewportHeight)
    {
        // Forward+ sampler textures are bound via material maps in DrawModelWithShader()
        // instead of SetShaderValueTexture (which uses activeTextureId — unreliable
        // because DrawRenderBatchActive clears it before DrawMesh binds textures).

        SetShaderIVec2(_screenSizeLoc, viewportWidth, viewportHeight);
        SetShaderIVec2(_tileCountLoc, _tileCountX, _tileCountY);
        SetShaderIVec2(_lightDataTexSizeLoc, _lightDataTexWidth, _lightDataTexHeight);
        SetShaderIVec2(_tileHeaderTexSizeLoc, _tileHeaderTexWidth, _tileHeaderTexHeight);
        SetShaderIVec2(_tileIndexTexSizeLoc, _tileIndexTexWidth, _tileIndexTexHeight);

        if (_tileSizeLoc >= 0)
            Raylib.SetShaderValue(_lightingShader, _tileSizeLoc, _forwardPlusSettings.TileSize, ShaderUniformDataType.Int);
        if (_totalLightsLoc >= 0)
            Raylib.SetShaderValue(_lightingShader, _totalLightsLoc, _frameLights.Count, ShaderUniformDataType.Int);
    }

    private void BuildFrameLights(Scene.Scene scene, Camera3D camera, bool isEditorMode)
    {
        _frameLights.Clear();
        _pointCandidates.Clear();
        _forwardPlusDroppedTileLights = 0;
        _forwardPlusClippedLights = 0;

        float[] ambient = { 0.15f, 0.15f, 0.15f, 1.0f };
        bool skylightFound = false;
        int eligibleLights = 0;
        int visibleLights = 0;
        int skylightCount = 0;
        int directionalCount = 0;
        int pointCount = 0;

        var cameraPos = camera.Position;
        var lights = scene.Lights;
        _lastSceneLightCount = lights.Count;

        foreach (var light in lights)
        {
            if (!light.Entity.Active) continue;
            if (light.EditorOnly && !isEditorMode) continue;
            if (!light.Enabled) continue;

            visibleLights++;

            if (light.LightType == LightType.Skylight)
            {
                skylightCount++;
                if (!skylightFound)
                {
                    skylightFound = true;
                    var skylightColor = light.LightColor;
                    float intensity = light.Intensity;
                    ambient = new[]
                    {
                        skylightColor.R / 255f * intensity,
                        skylightColor.G / 255f * intensity,
                        skylightColor.B / 255f * intensity,
                        1.0f
                    };
                }
                continue;
            }

            eligibleLights++;

            var packed = CreatePackedLight(light);
            if (packed.Type == PackedLightType.Directional)
            {
                directionalCount++;
                if (_frameLights.Count < _forwardPlusSettings.MaxLights)
                    _frameLights.Add(packed);
                continue;
            }

            pointCount++;
            var offset = packed.Position - cameraPos;
            var distanceSquared = offset.LengthSquared();
            _pointCandidates.Add(new PointLightCandidate(packed, distanceSquared));
        }

        _pointCandidates.Sort(static (a, b) => a.DistanceSquared.CompareTo(b.DistanceSquared));
        foreach (var candidate in _pointCandidates)
        {
            if (_frameLights.Count >= _forwardPlusSettings.MaxLights)
                break;
            _frameLights.Add(candidate.Light);
        }

        _forwardPlusClippedLights = Math.Max(0, eligibleLights - _frameLights.Count);
        _lastVisibleLightCount = visibleLights;
        _lastSkylightCount = skylightCount;
        _lastDirectionalLightCount = directionalCount;
        _lastPointLightCount = pointCount;
        Raylib.SetShaderValue(_lightingShader, _ambientLoc, ambient, ShaderUniformDataType.Vec4);
    }

    private void BuildTileLightLists(Camera3D camera)
    {
        Array.Fill(_tileLightCounts, 0);
        Array.Fill(_tileLightScores, float.PositiveInfinity);
        Array.Fill(_tileIndexBuffer, -1f);

        for (int lightIndex = 0; lightIndex < _frameLights.Count; lightIndex++)
        {
            var light = _frameLights[lightIndex];
            if (light.Type == PackedLightType.Directional)
            {
                for (int tileIndex = 0; tileIndex < _tileCount; tileIndex++)
                    TryInsertTileLight(tileIndex, lightIndex, -1f);
                continue;
            }

            if (!TryProjectPointLight(camera, light.Position, light.Range, out var projected))
                continue;

            for (int ty = projected.MinTileY; ty <= projected.MaxTileY; ty++)
            {
                for (int tx = projected.MinTileX; tx <= projected.MaxTileX; tx++)
                {
                    int tileIndex = ty * _tileCountX + tx;
                    float tileCenterX = tx * _forwardPlusSettings.TileSize + _forwardPlusSettings.TileSize * 0.5f;
                    float tileCenterY = ty * _forwardPlusSettings.TileSize + _forwardPlusSettings.TileSize * 0.5f;
                    float dx = tileCenterX - projected.CenterPixelX;
                    float dy = tileCenterY - projected.CenterPixelY;
                    float score = dx * dx + dy * dy + projected.DepthScore;
                    TryInsertTileLight(tileIndex, lightIndex, score);
                }
            }
        }

        if (_tileCount > 0)
        {
            int sum = 0;
            int peak = 0;
            for (int i = 0; i < _tileCount; i++)
            {
                int count = _tileLightCounts[i];
                sum += count;
                if (count > peak)
                    peak = count;
            }

            _lastAverageLightsPerTile = sum / (float)_tileCount;
            _lastMaxLightsPerTile = peak;
        }
        else
        {
            _lastAverageLightsPerTile = 0f;
            _lastMaxLightsPerTile = 0;
        }

        _lastStatsValid = true;
    }

    private void TryInsertTileLight(int tileIndex, int lightIndex, float score)
    {
        int baseSlot = tileIndex * _forwardPlusSettings.MaxLightsPerTile;
        int count = _tileLightCounts[tileIndex];

        if (count < _forwardPlusSettings.MaxLightsPerTile)
        {
            int slot = baseSlot + count;
            SetTileIndexValue(slot, lightIndex);
            _tileLightScores[slot] = score;
            _tileLightCounts[tileIndex] = count + 1;
            return;
        }

        _forwardPlusDroppedTileLights++;

        int worstSlot = baseSlot;
        float worstScore = _tileLightScores[baseSlot];
        for (int i = 1; i < _forwardPlusSettings.MaxLightsPerTile; i++)
        {
            int slot = baseSlot + i;
            float currentScore = _tileLightScores[slot];
            if (currentScore > worstScore)
            {
                worstScore = currentScore;
                worstSlot = slot;
            }
        }

        if (score >= worstScore)
            return;

        SetTileIndexValue(worstSlot, lightIndex);
        _tileLightScores[worstSlot] = score;
    }

    private void SetTileIndexValue(int slot, int lightIndex)
    {
        int dataOffset = slot * 4;
        _tileIndexBuffer[dataOffset] = lightIndex;
        _tileIndexBuffer[dataOffset + 1] = 0f;
        _tileIndexBuffer[dataOffset + 2] = 0f;
        _tileIndexBuffer[dataOffset + 3] = 0f;
    }

    private void PackLightBuffer()
    {
        Array.Fill(_lightDataBuffer, 0f);

        for (int i = 0; i < _frameLights.Count; i++)
        {
            var light = _frameLights[i];
            int lightStartTexel = i * LightTexelsPerLight;

            WritePackedVec4(_lightDataBuffer, lightStartTexel + 0,
                (int)light.Type, 1f, light.Range, 0f);
            WritePackedVec4(_lightDataBuffer, lightStartTexel + 1,
                light.Position.X, light.Position.Y, light.Position.Z, 0f);
            WritePackedVec4(_lightDataBuffer, lightStartTexel + 2,
                light.Direction.X, light.Direction.Y, light.Direction.Z, 0f);
            WritePackedVec4(_lightDataBuffer, lightStartTexel + 3,
                light.Color.X, light.Color.Y, light.Color.Z, 1f);
        }
    }

    private void PackTileHeaderBuffer()
    {
        for (int tileIndex = 0; tileIndex < _tileCount; tileIndex++)
        {
            int start = tileIndex * _forwardPlusSettings.MaxLightsPerTile;
            int count = _tileLightCounts[tileIndex];
            WritePackedVec4(_tileHeaderBuffer, tileIndex, start, count, 0f, 0f);
        }
    }

    private void UploadForwardPlusBuffers()
    {
        if (_lightDataTexture.Id != 0)
            Raylib.UpdateTexture(_lightDataTexture, _lightDataBuffer);
        if (_tileHeaderTexture.Id != 0)
            Raylib.UpdateTexture(_tileHeaderTexture, _tileHeaderBuffer);
        if (_tileIndexTexture.Id != 0)
            Raylib.UpdateTexture(_tileIndexTexture, _tileIndexBuffer);
    }

    private void EnsureForwardPlusResources(int viewportWidth, int viewportHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0)
            return;

        bool viewportChanged = _viewportWidth != viewportWidth || _viewportHeight != viewportHeight;
        if (viewportChanged)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
        }

        int requiredTileCountX = (_viewportWidth + _forwardPlusSettings.TileSize - 1) / _forwardPlusSettings.TileSize;
        int requiredTileCountY = (_viewportHeight + _forwardPlusSettings.TileSize - 1) / _forwardPlusSettings.TileSize;
        int requiredTileCount = Math.Max(1, requiredTileCountX * requiredTileCountY);

        bool tilesChanged = requiredTileCountX != _tileCountX
                            || requiredTileCountY != _tileCountY
                            || requiredTileCount != _tileCount;

        _tileCountX = requiredTileCountX;
        _tileCountY = requiredTileCountY;
        _tileCount = requiredTileCount;

        int requiredLightEntries = Math.Max(1, _forwardPlusSettings.MaxLights * LightTexelsPerLight);
        int requiredHeaderEntries = Math.Max(1, _tileCount);
        int requiredIndexEntries = Math.Max(1, _tileCount * _forwardPlusSettings.MaxLightsPerTile);

        EnsurePackedTexture(
            ref _lightDataTexture,
            ref _lightDataBuffer,
            ref _lightDataEntries,
            ref _lightDataTexWidth,
            ref _lightDataTexHeight,
            requiredLightEntries);

        bool tileStorageChanged = tilesChanged
                                  || requiredHeaderEntries != _tileHeaderEntries
                                  || requiredIndexEntries != _tileIndexEntries;

        EnsurePackedTexture(
            ref _tileHeaderTexture,
            ref _tileHeaderBuffer,
            ref _tileHeaderEntries,
            ref _tileHeaderTexWidth,
            ref _tileHeaderTexHeight,
            requiredHeaderEntries);

        EnsurePackedTexture(
            ref _tileIndexTexture,
            ref _tileIndexBuffer,
            ref _tileIndexEntries,
            ref _tileIndexTexWidth,
            ref _tileIndexTexHeight,
            requiredIndexEntries);

        if (tileStorageChanged || _tileLightCounts.Length != _tileCount)
        {
            _tileLightCounts = new int[_tileCount];
            _tileLightScores = new float[_tileCount * _forwardPlusSettings.MaxLightsPerTile];
        }
    }

    private void EnsurePackedTexture(
        ref Texture2D texture,
        ref float[] buffer,
        ref int entryCount,
        ref int textureWidth,
        ref int textureHeight,
        int requiredEntries)
    {
        if (requiredEntries <= 0)
            requiredEntries = 1;

        var (requiredWidth, requiredHeight) = ComputePackedTextureSize(requiredEntries);
        bool recreateTexture = texture.Id == 0
                               || requiredWidth != textureWidth
                               || requiredHeight != textureHeight;

        if (recreateTexture)
        {
            if (texture.Id != 0)
                Raylib.UnloadTexture(texture);

            texture = CreateFloatTexture(requiredWidth, requiredHeight);
            textureWidth = requiredWidth;
            textureHeight = requiredHeight;
        }

        int requiredBufferLength = requiredWidth * requiredHeight * 4;
        if (buffer.Length != requiredBufferLength)
            buffer = new float[requiredBufferLength];

        entryCount = requiredEntries;
    }

    private static (int Width, int Height) ComputePackedTextureSize(int entries)
    {
        entries = Math.Max(entries, 1);
        int width = Math.Min(PackedTextureMaxWidth, entries);
        int height = (entries + width - 1) / width;
        return (width, Math.Max(1, height));
    }

    private static unsafe Texture2D CreateFloatTexture(int width, int height)
    {
        var initial = new float[width * height * 4];
        fixed (float* data = initial)
        {
            uint textureId = Rlgl.LoadTexture(data, width, height, PixelFormat.UncompressedR32G32B32A32, 1);
            var texture = new Texture2D
            {
                Id = textureId,
                Width = width,
                Height = height,
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

    private PackedLight CreatePackedLight(LightComponent light)
    {
        var c = light.LightColor;
        float intensity = light.Intensity;
        var color = new Vector3(c.R / 255f * intensity, c.G / 255f * intensity, c.B / 255f * intensity);

        if (light.LightType == LightType.Directional)
        {
            var direction = light.Entity.Transform.Forward;
            if (direction.LengthSquared() > 1e-8f)
                direction = Vector3.Normalize(direction);
            else
                direction = -Vector3.UnitY;

            return new PackedLight(
                PackedLightType.Directional,
                Vector3.Zero,
                direction,
                color,
                0f);
        }

        return new PackedLight(
            PackedLightType.Point,
            light.Entity.Transform.WorldPosition,
            Vector3.Zero,
            color,
            MathF.Max(0f, light.Range));
    }

    private bool TryProjectPointLight(Camera3D camera, Vector3 position, float range, out ProjectedPointLight projected)
    {
        projected = default;
        if (range <= 0f || _tileCountX <= 0 || _tileCountY <= 0)
            return false;

        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        var viewSpacePosition = Vector3.Transform(position, view);

        float aspect = MathF.Max(1e-5f, _viewportWidth / (float)Math.Max(1, _viewportHeight));
        float ndcX;
        float ndcY;
        float radiusNdcX;
        float radiusNdcY;
        float depthForScore;

        if (camera.Projection == CameraProjection.Perspective)
        {
            float depth = -viewSpacePosition.Z;
            if (depth <= -range)
                return false;

            depthForScore = MathF.Max(depth, MinPerspectiveDepth);

            float halfFovRad = MathF.Max(1e-4f, camera.FovY * 0.5f * (MathF.PI / 180f));
            float halfHeight = depthForScore * MathF.Tan(halfFovRad);
            if (halfHeight <= 1e-5f)
                return false;

            float halfWidth = halfHeight * aspect;
            ndcX = viewSpacePosition.X / halfWidth;
            ndcY = viewSpacePosition.Y / halfHeight;
            radiusNdcX = range / halfWidth;
            radiusNdcY = range / halfHeight;
        }
        else
        {
            float halfHeight = MathF.Max(0.01f, camera.FovY * 0.5f);
            float halfWidth = halfHeight * aspect;
            ndcX = viewSpacePosition.X / halfWidth;
            ndcY = viewSpacePosition.Y / halfHeight;
            radiusNdcX = range / halfWidth;
            radiusNdcY = range / halfHeight;
            depthForScore = 1f;
        }

        if (ndcX + radiusNdcX < -1f || ndcX - radiusNdcX > 1f || ndcY + radiusNdcY < -1f || ndcY - radiusNdcY > 1f)
            return false;

        float centerPixelX = (ndcX * 0.5f + 0.5f) * _viewportWidth;
        float centerPixelY = (-ndcY * 0.5f + 0.5f) * _viewportHeight;
        float radiusPixelsX = radiusNdcX * 0.5f * _viewportWidth;
        float radiusPixelsY = radiusNdcY * 0.5f * _viewportHeight;

        float minPixelX = centerPixelX - radiusPixelsX;
        float maxPixelX = centerPixelX + radiusPixelsX;
        float minPixelY = centerPixelY - radiusPixelsY;
        float maxPixelY = centerPixelY + radiusPixelsY;

        if (maxPixelX < 0f || minPixelX > _viewportWidth || maxPixelY < 0f || minPixelY > _viewportHeight)
            return false;

        int clampedMinPixelX = Math.Clamp((int)MathF.Floor(minPixelX), 0, Math.Max(0, _viewportWidth - 1));
        int clampedMaxPixelX = Math.Clamp((int)MathF.Ceiling(maxPixelX), 0, Math.Max(0, _viewportWidth - 1));
        int clampedMinPixelY = Math.Clamp((int)MathF.Floor(minPixelY), 0, Math.Max(0, _viewportHeight - 1));
        int clampedMaxPixelY = Math.Clamp((int)MathF.Ceiling(maxPixelY), 0, Math.Max(0, _viewportHeight - 1));

        if (clampedMinPixelX > clampedMaxPixelX || clampedMinPixelY > clampedMaxPixelY)
            return false;

        projected = new ProjectedPointLight(
            clampedMinPixelX / _forwardPlusSettings.TileSize,
            clampedMaxPixelX / _forwardPlusSettings.TileSize,
            clampedMinPixelY / _forwardPlusSettings.TileSize,
            clampedMaxPixelY / _forwardPlusSettings.TileSize,
            centerPixelX,
            centerPixelY,
            depthForScore * depthForScore * 0.001f);
        return true;
    }

    private void MaybeLogForwardPlusWarnings()
    {
        _frameCounter++;
        if ((_forwardPlusClippedLights <= 0 && _forwardPlusDroppedTileLights <= 0)
            || _frameCounter % ForwardPlusWarningLogIntervalFrames != 0)
            return;

        FrinkyLog.Warning(
            $"Forward+ budget pressure: clippedLights={_forwardPlusClippedLights}, droppedTileLinks={_forwardPlusDroppedTileLights}, " +
            $"maxLights={_forwardPlusSettings.MaxLights}, maxLightsPerTile={_forwardPlusSettings.MaxLightsPerTile}, " +
            $"tiles={_tileCountX}x{_tileCountY}");
    }

    private static void WritePackedVec4(float[] buffer, int texelIndex, float x, float y, float z, float w)
    {
        int dataIndex = texelIndex * 4;
        buffer[dataIndex + 0] = x;
        buffer[dataIndex + 1] = y;
        buffer[dataIndex + 2] = z;
        buffer[dataIndex + 3] = w;
    }

    private void SetShaderIVec2(int location, int x, int y)
    {
        if (location < 0)
            return;

        Span<int> vec2 = stackalloc int[2];
        vec2[0] = x;
        vec2[1] = y;
        Raylib.SetShaderValue(_lightingShader, location, vec2, ShaderUniformDataType.IVec2);
    }

    private void ReleaseForwardPlusTextures()
    {
        if (_lightDataTexture.Id != 0)
            Raylib.UnloadTexture(_lightDataTexture);
        if (_tileHeaderTexture.Id != 0)
            Raylib.UnloadTexture(_tileHeaderTexture);
        if (_tileIndexTexture.Id != 0)
            Raylib.UnloadTexture(_tileIndexTexture);

        _lightDataTexture = default;
        _tileHeaderTexture = default;
        _tileIndexTexture = default;
    }

    private static void DrawGrid(int slices, float spacing)
    {
        Raylib.DrawGrid(slices, spacing);
    }

    private enum PackedLightType
    {
        Directional = 0,
        Point = 1
    }

    private readonly record struct PackedLight(
        PackedLightType Type,
        Vector3 Position,
        Vector3 Direction,
        Vector3 Color,
        float Range);

    private readonly record struct PointLightCandidate(PackedLight Light, float DistanceSquared);

    private readonly record struct ProjectedPointLight(
        int MinTileX,
        int MaxTileX,
        int MinTileY,
        int MaxTileY,
        float CenterPixelX,
        float CenterPixelY,
        float DepthScore);
}
