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

    public void Render(Scene.Scene scene, Camera3D camera, RenderTexture2D? renderTarget = null)
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
            if (!renderer.Enabled || renderer.LoadedModel == null) continue;

            var model = renderer.LoadedModel.Value;

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

            var pos = renderer.Entity.Transform.WorldPosition;
            Raylib.DrawModel(model, pos, 1f, renderer.Tint);
        }

        DrawGrid(20, 1.0f);

        Raylib.EndMode3D();

        if (renderTarget.HasValue)
            Raylib.EndTextureMode();
    }

    private void UpdateLightUniforms(Scene.Scene scene)
    {
        var lights = scene.Lights;
        for (int i = 0; i < 4; i++)
        {
            if (i < lights.Count && lights[i].Enabled)
            {
                int enabled = 1;
                Raylib.SetShaderValue(_lightingShader, _lightEnabledLoc[i], enabled, ShaderUniformDataType.Int);

                int type = (int)lights[i].LightType;
                Raylib.SetShaderValue(_lightingShader, _lightTypeLoc[i], type, ShaderUniformDataType.Int);

                var pos = lights[i].Entity.Transform.WorldPosition;
                float[] posArr = { pos.X, pos.Y, pos.Z };
                Raylib.SetShaderValue(_lightingShader, _lightPositionLoc[i], posArr, ShaderUniformDataType.Vec3);

                var c = lights[i].LightColor;
                float intensity = lights[i].Intensity;
                float[] colorArr = { c.R / 255f * intensity, c.G / 255f * intensity, c.B / 255f * intensity, c.A / 255f };
                Raylib.SetShaderValue(_lightingShader, _lightColorLoc[i], colorArr, ShaderUniformDataType.Vec4);
            }
            else
            {
                int disabled = 0;
                Raylib.SetShaderValue(_lightingShader, _lightEnabledLoc[i], disabled, ShaderUniformDataType.Int);
            }
        }
    }

    private static void DrawGrid(int slices, float spacing)
    {
        Raylib.DrawGrid(slices, spacing);
    }
}
