using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering;

public class SceneRenderer
{
    private Shader _lightingShader;
    private bool _shaderLoaded;
    private Shader _selectionMaskShader;
    private bool _selectionMaskShaderLoaded;
    private int _ambientLoc;
    private int _viewPosLoc;

    private readonly int[] _lightEnabledLoc = new int[4];
    private readonly int[] _lightTypeLoc = new int[4];
    private readonly int[] _lightPositionLoc = new int[4];
    private readonly int[] _lightColorLoc = new int[4];

    public void LoadShader(string vsPath, string fsPath)
    {
        _lightingShader = Raylib.LoadShader(vsPath, fsPath);
        _viewPosLoc = Raylib.GetShaderLocation(_lightingShader, "viewPos");

        _ambientLoc = Raylib.GetShaderLocation(_lightingShader, "ambient");
        float[] ambient = { 0.15f, 0.15f, 0.15f, 1.0f };
        Raylib.SetShaderValue(_lightingShader, _ambientLoc, ambient, ShaderUniformDataType.Vec4);

        for (int i = 0; i < 4; i++)
        {
            _lightEnabledLoc[i] = Raylib.GetShaderLocation(_lightingShader, $"lights[{i}].enabled");
            _lightTypeLoc[i] = Raylib.GetShaderLocation(_lightingShader, $"lights[{i}].type");
            _lightPositionLoc[i] = Raylib.GetShaderLocation(_lightingShader, $"lights[{i}].position");
            _lightColorLoc[i] = Raylib.GetShaderLocation(_lightingShader, $"lights[{i}].color");
        }

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

    public void UnloadShader()
    {
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

            UpdateLightUniforms(scene, isEditorMode);
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
                }
            }
        }

        model.Transform = Matrix4x4.Transpose(worldMatrix);
        Raylib.DrawModel(model, System.Numerics.Vector3.Zero, 1f, tint);
    }

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

    private void UpdateLightUniforms(Scene.Scene scene, bool isEditorMode = true)
    {
        var lights = scene.Lights;

        // Find first enabled Skylight for ambient, otherwise use default
        float[] ambient = { 0.15f, 0.15f, 0.15f, 1.0f };
        foreach (var light in lights)
        {
            if (!light.Entity.Active) continue;
            if (light.EditorOnly && !isEditorMode) continue;
            if (light.Enabled && light.LightType == Components.LightType.Skylight)
            {
                var c = light.LightColor;
                float intensity = light.Intensity;
                ambient = new[] { c.R / 255f * intensity, c.G / 255f * intensity, c.B / 255f * intensity, 1.0f };
                break;
            }
        }
        Raylib.SetShaderValue(_lightingShader, _ambientLoc, ambient, ShaderUniformDataType.Vec4);

        // Fill shader light slots (0-3) with non-Skylight lights only
        int slot = 0;
        foreach (var light in lights)
        {
            if (slot >= 4) break;
            if (!light.Entity.Active) continue;
            if (light.EditorOnly && !isEditorMode) continue;
            if (!light.Enabled || light.LightType == Components.LightType.Skylight) continue;

            int enabled = 1;
            Raylib.SetShaderValue(_lightingShader, _lightEnabledLoc[slot], enabled, ShaderUniformDataType.Int);

            int type = (int)light.LightType;
            Raylib.SetShaderValue(_lightingShader, _lightTypeLoc[slot], type, ShaderUniformDataType.Int);

            // Directional lights use forward vector; point lights use world position
            var posVec = light.LightType == Components.LightType.Directional
                ? light.Entity.Transform.Forward
                : light.Entity.Transform.WorldPosition;
            float[] posArr = { posVec.X, posVec.Y, posVec.Z };
            Raylib.SetShaderValue(_lightingShader, _lightPositionLoc[slot], posArr, ShaderUniformDataType.Vec3);

            var c = light.LightColor;
            float intensity = light.Intensity;
            float[] colorArr = { c.R / 255f * intensity, c.G / 255f * intensity, c.B / 255f * intensity, c.A / 255f };
            Raylib.SetShaderValue(_lightingShader, _lightColorLoc[slot], colorArr, ShaderUniformDataType.Vec4);

            slot++;
        }

        // Disable remaining unused slots
        for (int i = slot; i < 4; i++)
        {
            int disabled = 0;
            Raylib.SetShaderValue(_lightingShader, _lightEnabledLoc[i], disabled, ShaderUniformDataType.Int);
        }
    }

    private static void DrawGrid(int slices, float spacing)
    {
        Raylib.DrawGrid(slices, spacing);
    }
}
