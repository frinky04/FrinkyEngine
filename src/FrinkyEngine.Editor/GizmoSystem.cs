using System.Numerics;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public enum GizmoMode
{
    None,
    Translate,
    Rotate,
    Scale
}

public enum GizmoSpace
{
    World,
    Local
}

public class GizmoSystem
{
    private const float GizmoScaleFactor = 0.15f;
    private const float PickThresholdFactor = 0.12f;
    private const float ArrowLength = 1f;
    private const float ArrowHeadLength = 0.2f;
    private const float ArrowHeadRadius = 0.06f;
    private const float RotateRadius = 0.9f;
    private const float ScaleCubeSize = 0.08f;
    private const int CircleSegments = 64;

    public GizmoMode Mode { get; set; } = GizmoMode.Translate;
    public GizmoSpace Space { get; set; } = GizmoSpace.World;

    private int _hoveredAxis = -1; // -1=none, 0=X, 1=Y, 2=Z
    private bool _isDragging;
    private Vector3 _dragStartPosition;
    private Quaternion _dragStartRotation;
    private Vector3 _dragStartScale;
    private Vector3 _lastIntersection;
    private float _dragStartAngle;

    private static readonly Color[] AxisColors = { Color.Red, Color.Green, Color.Blue };
    private static readonly Color HoverColor = Color.Yellow;

    public void Draw(Camera3D camera, Entity? selected)
    {
        if (selected == null || Mode == GizmoMode.None) return;

        var origin = selected.Transform.WorldPosition;
        float distance = Vector3.Distance(camera.Position, origin);
        float gizmoScale = distance * GizmoScaleFactor;

        var axes = GetAxes(selected);

        Rlgl.DrawRenderBatchActive();
        Rlgl.DisableDepthTest();

        switch (Mode)
        {
            case GizmoMode.Translate:
                DrawTranslateGizmo(origin, axes, gizmoScale);
                break;
            case GizmoMode.Rotate:
                DrawRotateGizmo(origin, axes, gizmoScale);
                break;
            case GizmoMode.Scale:
                DrawScaleGizmo(origin, axes, gizmoScale);
                break;
        }

        Rlgl.DrawRenderBatchActive();
        Rlgl.EnableDepthTest();
    }

    public void Update(Camera3D camera, Entity? selected, Vector2 viewportMousePos, Vector2 viewportSize)
    {
        if (selected == null || Mode == GizmoMode.None)
        {
            _hoveredAxis = -1;
            _isDragging = false;
            return;
        }

        var origin = selected.Transform.WorldPosition;
        float distance = Vector3.Distance(camera.Position, origin);
        float gizmoScale = distance * GizmoScaleFactor;
        var axes = GetAxes(selected);

        var ray = GetViewportRay(camera, viewportMousePos, viewportSize);

        if (_isDragging)
        {
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                _isDragging = false;
            }
            else
            {
                ApplyDrag(selected, ray, origin, axes, gizmoScale);
            }
            return;
        }

        // Hover detection
        _hoveredAxis = -1;
        switch (Mode)
        {
            case GizmoMode.Translate:
            case GizmoMode.Scale:
                _hoveredAxis = PickLinearAxis(ray, origin, axes, gizmoScale);
                break;
            case GizmoMode.Rotate:
                _hoveredAxis = PickRotateAxis(ray, origin, axes, gizmoScale);
                break;
        }

        // Start drag
        if (_hoveredAxis >= 0 && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            _isDragging = true;
            _dragStartPosition = selected.Transform.LocalPosition;
            _dragStartRotation = selected.Transform.LocalRotation;
            _dragStartScale = selected.Transform.LocalScale;

            if (Mode == GizmoMode.Translate || Mode == GizmoMode.Scale)
            {
                _lastIntersection = ClosestPointOnAxis(ray, origin, axes[_hoveredAxis]);
            }
            else if (Mode == GizmoMode.Rotate)
            {
                var axis = axes[_hoveredAxis];
                var hitPoint = RayPlaneIntersection(ray, origin, axis);
                if (hitPoint.HasValue)
                {
                    var dir = Vector3.Normalize(hitPoint.Value - origin);
                    _dragStartAngle = AngleOnPlane(dir, axis);
                }
            }
        }
    }

    private void ApplyDrag(Entity selected, Ray ray, Vector3 origin, Vector3[] axes, float gizmoScale)
    {
        switch (Mode)
        {
            case GizmoMode.Translate:
                ApplyTranslateDrag(selected, ray, origin, axes);
                break;
            case GizmoMode.Rotate:
                ApplyRotateDrag(selected, ray, origin, axes);
                break;
            case GizmoMode.Scale:
                ApplyScaleDrag(selected, ray, origin, axes);
                break;
        }
    }

    private void ApplyTranslateDrag(Entity selected, Ray ray, Vector3 origin, Vector3[] axes)
    {
        var axis = axes[_hoveredAxis];
        var currentPoint = ClosestPointOnAxis(ray, origin, axis);
        var delta = currentPoint - _lastIntersection;

        var worldDelta = delta;
        if (selected.Transform.Parent != null)
        {
            var parentWorld = selected.Transform.Parent.WorldMatrix;
            if (Matrix4x4.Invert(parentWorld, out var parentInverse))
            {
                worldDelta = Vector3.TransformNormal(delta, parentInverse);
            }
        }

        selected.Transform.LocalPosition = _dragStartPosition + worldDelta;
        _dragStartPosition = selected.Transform.LocalPosition;
        _lastIntersection = currentPoint;
    }

    private void ApplyRotateDrag(Entity selected, Ray ray, Vector3 origin, Vector3[] axes)
    {
        var axis = axes[_hoveredAxis];
        var hitPoint = RayPlaneIntersection(ray, origin, axis);
        if (!hitPoint.HasValue) return;

        var dir = Vector3.Normalize(hitPoint.Value - origin);
        float currentAngle = AngleOnPlane(dir, axis);
        float deltaAngle = currentAngle - _dragStartAngle;

        // Convert world axis to local axis for rotation
        var localAxis = axis;
        if (Space == GizmoSpace.World && selected.Transform.Parent != null)
        {
            var parentWorld = selected.Transform.Parent.WorldMatrix;
            if (Matrix4x4.Invert(parentWorld, out var parentInverse))
            {
                localAxis = Vector3.Normalize(Vector3.TransformNormal(axis, parentInverse));
            }
        }
        else if (Space == GizmoSpace.World)
        {
            // World axis applied to local rotation: need to account for current rotation
            // deltaRotation in world space, convert to local
            var worldRot = Quaternion.CreateFromAxisAngle(axis, deltaAngle);
            selected.Transform.LocalRotation = Quaternion.Normalize(worldRot * _dragStartRotation);
            _dragStartAngle = currentAngle;
            _dragStartRotation = selected.Transform.LocalRotation;
            return;
        }

        selected.Transform.LocalRotation = Quaternion.Normalize(
            Quaternion.CreateFromAxisAngle(localAxis, deltaAngle) * _dragStartRotation);

        _dragStartAngle = currentAngle;
        _dragStartRotation = selected.Transform.LocalRotation;
    }

    private void ApplyScaleDrag(Entity selected, Ray ray, Vector3 origin, Vector3[] axes)
    {
        var axis = axes[_hoveredAxis];
        var currentPoint = ClosestPointOnAxis(ray, origin, axis);
        var delta = currentPoint - _lastIntersection;

        float signedDist = Vector3.Dot(delta, axis);

        // Scale along the local axis direction
        var scaleAxis = _hoveredAxis switch
        {
            0 => Vector3.UnitX,
            1 => Vector3.UnitY,
            2 => Vector3.UnitZ,
            _ => Vector3.Zero
        };

        selected.Transform.LocalScale = _dragStartScale + scaleAxis * signedDist;
        _lastIntersection = currentPoint;
        _dragStartScale = selected.Transform.LocalScale;
    }

    private Vector3[] GetAxes(Entity entity)
    {
        if (Space == GizmoSpace.Local)
        {
            return new[]
            {
                entity.Transform.Right,
                entity.Transform.Up,
                -entity.Transform.Forward
            };
        }
        return new[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
    }

    private void DrawTranslateGizmo(Vector3 origin, Vector3[] axes, float scale)
    {
        for (int i = 0; i < 3; i++)
        {
            var color = (_hoveredAxis == i) ? HoverColor : AxisColors[i];
            var end = origin + axes[i] * ArrowLength * scale;
            var headEnd = origin + axes[i] * (ArrowLength + ArrowHeadLength) * scale;

            Raylib.DrawLine3D(origin, end, color);
            Raylib.DrawCylinderEx(end, headEnd, ArrowHeadRadius * scale, 0f, 8, color);
        }
    }

    private void DrawRotateGizmo(Vector3 origin, Vector3[] axes, float scale)
    {
        float radius = RotateRadius * scale;

        for (int i = 0; i < 3; i++)
        {
            var color = (_hoveredAxis == i) ? HoverColor : AxisColors[i];
            var axis = axes[i];

            // Build two perpendicular vectors to the axis
            var perp1 = GetPerpendicular(axis);
            var perp2 = Vector3.Cross(axis, perp1);

            Vector3 prevPoint = origin + perp1 * radius;
            for (int j = 1; j <= CircleSegments; j++)
            {
                float angle = j * MathF.PI * 2f / CircleSegments;
                var point = origin + (perp1 * MathF.Cos(angle) + perp2 * MathF.Sin(angle)) * radius;
                Raylib.DrawLine3D(prevPoint, point, color);
                prevPoint = point;
            }
        }
    }

    private void DrawScaleGizmo(Vector3 origin, Vector3[] axes, float scale)
    {
        for (int i = 0; i < 3; i++)
        {
            var color = (_hoveredAxis == i) ? HoverColor : AxisColors[i];
            var end = origin + axes[i] * ArrowLength * scale;

            Raylib.DrawLine3D(origin, end, color);

            var cubeSize = new Vector3(ScaleCubeSize * scale);
            Raylib.DrawCubeV(end, cubeSize, color);
        }
    }

    private int PickLinearAxis(Ray ray, Vector3 origin, Vector3[] axes, float scale)
    {
        float threshold = scale * PickThresholdFactor;
        float bestDist = threshold;
        int bestAxis = -1;

        for (int i = 0; i < 3; i++)
        {
            var axisEnd = origin + axes[i] * (ArrowLength + ArrowHeadLength) * scale;
            float dist = RaySegmentDistance(ray, origin, axisEnd);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestAxis = i;
            }
        }

        return bestAxis;
    }

    private int PickRotateAxis(Ray ray, Vector3 origin, Vector3[] axes, float scale)
    {
        float radius = RotateRadius * scale;
        float threshold = scale * PickThresholdFactor;
        float bestDelta = threshold;
        int bestAxis = -1;

        for (int i = 0; i < 3; i++)
        {
            var hitPoint = RayPlaneIntersection(ray, origin, axes[i]);
            if (!hitPoint.HasValue) continue;

            float distFromCenter = Vector3.Distance(hitPoint.Value, origin);
            float delta = MathF.Abs(distFromCenter - radius);
            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestAxis = i;
            }
        }

        return bestAxis;
    }

    private static Ray GetViewportRay(Camera3D camera, Vector2 mousePos, Vector2 viewportSize)
    {
        float ndcX = 2f * mousePos.X / viewportSize.X - 1f;
        float ndcY = 1f - 2f * mousePos.Y / viewportSize.Y;

        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        float fovRad = camera.FovY * MathF.PI / 180f;
        float aspect = viewportSize.X / viewportSize.Y;
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspect, 0.1f, 1000f);

        var vp = view * proj;
        Matrix4x4.Invert(vp, out var vpInverse);

        var nearPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 0f, 1f), vpInverse);
        nearPoint /= nearPoint.W;

        var farPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 1f, 1f), vpInverse);
        farPoint /= farPoint.W;

        var origin = new Vector3(nearPoint.X, nearPoint.Y, nearPoint.Z);
        var direction = Vector3.Normalize(
            new Vector3(farPoint.X, farPoint.Y, farPoint.Z) - origin);

        return new Ray(origin, direction);
    }

    private static Vector3 ClosestPointOnAxis(Ray ray, Vector3 axisOrigin, Vector3 axisDir)
    {
        // Closest point on axisRay to mouseRay
        var w = axisOrigin - ray.Position;
        float a = Vector3.Dot(axisDir, axisDir);
        float b = Vector3.Dot(axisDir, ray.Direction);
        float c = Vector3.Dot(ray.Direction, ray.Direction);
        float d = Vector3.Dot(axisDir, w);
        float e = Vector3.Dot(ray.Direction, w);

        float denom = a * c - b * b;
        if (MathF.Abs(denom) < 1e-8f)
            return axisOrigin;

        float t = (b * e - c * d) / denom;
        return axisOrigin + axisDir * t;
    }

    private static Vector3? RayPlaneIntersection(Ray ray, Vector3 planePoint, Vector3 planeNormal)
    {
        float denom = Vector3.Dot(planeNormal, ray.Direction);
        if (MathF.Abs(denom) < 1e-6f) return null;

        float t = Vector3.Dot(planePoint - ray.Position, planeNormal) / denom;
        if (t < 0) return null;

        return ray.Position + ray.Direction * t;
    }

    private static float RaySegmentDistance(Ray ray, Vector3 segStart, Vector3 segEnd)
    {
        var segDir = segEnd - segStart;
        float segLen = segDir.Length();
        if (segLen < 1e-6f) return Vector3.Distance(segStart, ray.Position);
        segDir /= segLen;

        var w = segStart - ray.Position;
        float a = Vector3.Dot(segDir, segDir);
        float b = Vector3.Dot(segDir, ray.Direction);
        float c = Vector3.Dot(ray.Direction, ray.Direction);
        float d = Vector3.Dot(segDir, w);
        float e = Vector3.Dot(ray.Direction, w);

        float denom = a * c - b * b;
        float tSeg, tRay;

        if (MathF.Abs(denom) < 1e-8f)
        {
            tSeg = 0f;
            tRay = e / c;
        }
        else
        {
            tSeg = (b * e - c * d) / denom;
            tRay = (a * e - b * d) / denom;
        }

        tSeg = Math.Clamp(tSeg, 0f, segLen);
        tRay = MathF.Max(tRay, 0f);

        var closestOnSeg = segStart + segDir * tSeg;
        var closestOnRay = ray.Position + ray.Direction * tRay;

        return Vector3.Distance(closestOnSeg, closestOnRay);
    }

    private static float AngleOnPlane(Vector3 direction, Vector3 planeNormal)
    {
        var perp1 = GetPerpendicular(planeNormal);
        var perp2 = Vector3.Cross(planeNormal, perp1);
        return MathF.Atan2(Vector3.Dot(direction, perp2), Vector3.Dot(direction, perp1));
    }

    private static Vector3 GetPerpendicular(Vector3 v)
    {
        var abs = new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
        var cross = abs.X < abs.Y
            ? (abs.X < abs.Z ? Vector3.UnitX : Vector3.UnitZ)
            : (abs.Y < abs.Z ? Vector3.UnitY : Vector3.UnitZ);
        return Vector3.Normalize(Vector3.Cross(v, cross));
    }
}
