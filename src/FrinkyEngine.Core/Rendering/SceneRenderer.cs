using System.Numerics;
using FrinkyEngine.Core.Components;
using Raylib_cs;

namespace FrinkyEngine.Core.Rendering;

public class SceneRenderer
{
    private Shader _lightingShader;
    private bool _shaderLoaded;
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
    }

    public void UnloadShader()
    {
        if (_shaderLoaded)
        {
            Raylib.UnloadShader(_lightingShader);
            _shaderLoaded = false;
        }
    }

    public void Render(Scene.Scene scene, Camera3D camera, RenderTexture2D? renderTarget = null, Action? postSceneRender = null)
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

            UpdateLightUniforms(scene);
        }

        foreach (var renderer in scene.Renderers)
        {
            if (!renderer.Enabled) continue;
            if (!renderer.LoadedModel.HasValue)
                renderer.EnsureModelLoaded();
            if (!renderer.LoadedModel.HasValue) continue;
            DrawModelWithShader(renderer.LoadedModel.Value, renderer.Entity.Transform.WorldMatrix, renderer.Tint);
        }

        foreach (var primitive in scene.Primitives)
        {
            if (!primitive.Enabled) continue;
            if (!primitive.GeneratedModel.HasValue)
                primitive.RebuildModel();
            if (!primitive.GeneratedModel.HasValue) continue;
            DrawModelWithShader(primitive.GeneratedModel.Value, primitive.Entity.Transform.WorldMatrix, primitive.Tint);
        }

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

    private void UpdateLightUniforms(Scene.Scene scene)
    {
        var lights = scene.Lights;

        // Find first enabled Skylight for ambient, otherwise use default
        float[] ambient = { 0.15f, 0.15f, 0.15f, 1.0f };
        foreach (var light in lights)
        {
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
