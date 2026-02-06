using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public static class EditorGizmos
{
    private static readonly Color CameraGizmoColor = new(200, 200, 200, 255);
    private static readonly Color DirectionalLightColor = new(255, 220, 50, 255);
    private static readonly Color SelectionHighlightColor = new(255, 170, 0, 255);

    public static void DrawAll(Core.Scene.Scene scene, Camera3D editorCamera)
    {
        foreach (var cam in scene.Cameras)
        {
            if (!cam.Entity.Active || !cam.Enabled) continue;
            DrawCameraGizmo(cam, editorCamera);
        }

        foreach (var light in scene.Lights)
        {
            if (!light.Entity.Active || !light.Enabled) continue;
            if (light.LightType == LightType.Directional)
                DrawDirectionalLightGizmo(light);
            else if (light.LightType == LightType.Point)
                DrawPointLightGizmo(light);
        }
    }

    private static void DrawCameraGizmo(CameraComponent cam, Camera3D editorCamera)
    {
        var pos = cam.Entity.Transform.WorldPosition;
        var forward = cam.Entity.Transform.Forward;
        var right = cam.Entity.Transform.Right;
        var up = cam.Entity.Transform.Up;

        float distance = Vector3.Distance(editorCamera.Position, pos);
        float scale = Math.Clamp(distance * 0.05f, 0.3f, 2f);

        float nearDist = 0.5f * scale;
        float farDist = 2.0f * scale;
        float fovRad = cam.FieldOfView * MathF.PI / 180f;
        float aspect = 16f / 9f;

        float nearH = MathF.Tan(fovRad * 0.5f) * nearDist;
        float nearW = nearH * aspect;
        float farH = MathF.Tan(fovRad * 0.5f) * farDist;
        float farW = farH * aspect;

        // Near plane corners
        var nc = pos + forward * nearDist;
        var ntl = nc + up * nearH - right * nearW;
        var ntr = nc + up * nearH + right * nearW;
        var nbl = nc - up * nearH - right * nearW;
        var nbr = nc - up * nearH + right * nearW;

        // Far plane corners
        var fc = pos + forward * farDist;
        var ftl = fc + up * farH - right * farW;
        var ftr = fc + up * farH + right * farW;
        var fbl = fc - up * farH - right * farW;
        var fbr = fc - up * farH + right * farW;

        // Near rectangle
        Raylib.DrawLine3D(ntl, ntr, CameraGizmoColor);
        Raylib.DrawLine3D(ntr, nbr, CameraGizmoColor);
        Raylib.DrawLine3D(nbr, nbl, CameraGizmoColor);
        Raylib.DrawLine3D(nbl, ntl, CameraGizmoColor);

        // Far rectangle
        Raylib.DrawLine3D(ftl, ftr, CameraGizmoColor);
        Raylib.DrawLine3D(ftr, fbr, CameraGizmoColor);
        Raylib.DrawLine3D(fbr, fbl, CameraGizmoColor);
        Raylib.DrawLine3D(fbl, ftl, CameraGizmoColor);

        // Connecting edges
        Raylib.DrawLine3D(ntl, ftl, CameraGizmoColor);
        Raylib.DrawLine3D(ntr, ftr, CameraGizmoColor);
        Raylib.DrawLine3D(nbl, fbl, CameraGizmoColor);
        Raylib.DrawLine3D(nbr, fbr, CameraGizmoColor);

        // Origin to near plane corners
        Raylib.DrawLine3D(pos, ntl, CameraGizmoColor);
        Raylib.DrawLine3D(pos, ntr, CameraGizmoColor);
        Raylib.DrawLine3D(pos, nbl, CameraGizmoColor);
        Raylib.DrawLine3D(pos, nbr, CameraGizmoColor);
    }

    private static void DrawDirectionalLightGizmo(LightComponent light)
    {
        var pos = light.Entity.Transform.WorldPosition;
        var forward = light.Entity.Transform.Forward;

        float arrowLength = 2.0f;
        var end = pos + forward * arrowLength;

        // Main arrow line
        Raylib.DrawLine3D(pos, end, DirectionalLightColor);

        // Arrowhead lines
        var right = light.Entity.Transform.Right;
        var up = light.Entity.Transform.Up;
        float headSize = 0.3f;
        var headBase = pos + forward * (arrowLength - headSize);

        Raylib.DrawLine3D(end, headBase + right * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase - right * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase + up * headSize * 0.5f, DirectionalLightColor);
        Raylib.DrawLine3D(end, headBase - up * headSize * 0.5f, DirectionalLightColor);

        // Small rays emanating from origin
        float rayLen = 0.5f;
        float offset = 0.4f;
        var rayPositions = new[]
        {
            pos + right * offset,
            pos - right * offset,
            pos + up * offset,
            pos - up * offset
        };
        foreach (var rp in rayPositions)
        {
            Raylib.DrawLine3D(rp, rp + forward * rayLen, DirectionalLightColor);
        }
    }

    private static void DrawPointLightGizmo(LightComponent light)
    {
        var pos = light.Entity.Transform.WorldPosition;
        var color = new Color(light.LightColor.R, light.LightColor.G, light.LightColor.B, (byte)128);
        Raylib.DrawSphereWires(pos, light.Range, 8, 8, color);
    }

    public static void DrawSelectionFallbackHighlight(Entity? selected)
    {
        if (selected == null || !selected.Active) return;

        var renderable = selected.GetComponent<RenderableComponent>();
        if (renderable != null && renderable.Enabled) return;

        // Non-renderable entities (cameras, lights): draw a small wireframe cube
        var camera = selected.GetComponent<CameraComponent>();
        var light = selected.GetComponent<LightComponent>();
        bool hasVisual = (camera != null && camera.Enabled)
                      || (light != null && light.Enabled);
        if (hasVisual)
        {
            var pos = selected.Transform.WorldPosition;
            var halfExt = new Vector3(0.5f);
            Raylib.DrawBoundingBox(
                new BoundingBox(pos - halfExt, pos + halfExt),
                SelectionHighlightColor);
        }
    }
}
