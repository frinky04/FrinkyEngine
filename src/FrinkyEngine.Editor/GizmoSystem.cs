using System.Numerics;
using FrinkyEngine.Core.Components;
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

public enum MultiTransformMode
{
    Independent,
    Relative
}

public class GizmoSystem
{
    private const float GizmoScaleFactor = 0.15f;
    private const float PickThresholdFactor = 0.18f;
    private const float ArrowLength = 1f;
    private const float ArrowHeadLength = 0.2f;
    private const float ArrowHeadRadius = 0.07f;
    private const float ShaftRadius = 0.02f;
    private const float RotateRadius = 0.9f;
    private const float RotateLineRadius = 0.015f;
    private const float ScaleCubeSize = 0.1f;
    private const float RelativeScaleFactorMin = 0.01f;
    private const float MinLocalScale = 0.001f;
    private const int CircleSegments = 64;

    public GizmoMode Mode { get; set; } = GizmoMode.Translate;
    public GizmoSpace Space { get; set; } = GizmoSpace.World;
    public MultiTransformMode MultiMode { get; set; } = MultiTransformMode.Independent;
    public bool IsDragging => _isDragging;
    public int HoveredAxis => _hoveredAxis;

    private int _hoveredAxis = -1; // -1=none, 0=X, 1=Y, 2=Z
    private bool _isDragging;

    private readonly List<DragTargetState> _dragTargets = new();
    private Vector3 _dragOrigin;
    private Vector3 _dragPivot;
    private Vector3[] _dragAxes = { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
    private Vector3 _dragStartIntersection;
    private float _dragStartAngle;
    private bool _hasRotateStart;

    private static readonly Color[] AxisColors = { Color.Red, Color.Green, Color.Blue };
    private static readonly Color HoverColor = Color.Yellow;

    public void Draw(Camera3D camera, Entity? selected)
    {
        if (selected == null)
        {
            Draw(camera, Array.Empty<Entity>(), null);
            return;
        }

        Draw(camera, new[] { selected }, selected);
    }

    public void Draw(Camera3D camera, IReadOnlyList<Entity> selected, Entity? primary)
    {
        if (primary == null || selected.Count == 0 || Mode == GizmoMode.None) return;

        var selectedRoots = FilterToSelectionRoots(selected);
        if (selectedRoots.Count == 0) return;

        var origin = GetGizmoOrigin(selectedRoots, primary);
        float distance = Vector3.Distance(camera.Position, origin);
        float gizmoScale = distance * GizmoScaleFactor;
        var axes = GetGizmoAxes(primary);

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
        if (selected == null)
        {
            Update(camera, Array.Empty<Entity>(), null, viewportMousePos, viewportSize);
            return;
        }

        Update(camera, new[] { selected }, selected, viewportMousePos, viewportSize);
    }

    public void Update(
        Camera3D camera,
        IReadOnlyList<Entity> selected,
        Entity? primary,
        Vector2 viewportMousePos,
        Vector2 viewportSize)
    {
        if (primary == null || selected.Count == 0 || Mode == GizmoMode.None)
        {
            ResetInteractionState();
            return;
        }

        var selectedRoots = FilterToSelectionRoots(selected);
        if (selectedRoots.Count == 0)
        {
            ResetInteractionState();
            return;
        }

        var origin = GetGizmoOrigin(selectedRoots, primary);
        float distance = Vector3.Distance(camera.Position, origin);
        float gizmoScale = distance * GizmoScaleFactor;
        var axes = GetGizmoAxes(primary);
        var ray = RaycastUtils.GetViewportRay(camera, viewportMousePos, viewportSize);

        if (_isDragging)
        {
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                _isDragging = false;
                _dragTargets.Clear();
                _hasRotateStart = false;
            }
            else
            {
                ApplyDrag(ray);
            }
            return;
        }

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

        if (_hoveredAxis >= 0 && Raylib.IsMouseButtonPressed(MouseButton.Left))
            BeginDrag(selectedRoots, origin, axes, ray);
    }

    private void BeginDrag(IReadOnlyList<Entity> selectedRoots, Vector3 origin, Vector3[] axes, Ray ray)
    {
        _dragTargets.Clear();
        foreach (var entity in selectedRoots)
            _dragTargets.Add(CaptureTargetState(entity));

        if (_dragTargets.Count == 0)
            return;

        _isDragging = true;
        _dragOrigin = origin;
        _dragAxes = axes;
        _dragPivot = ComputeSelectionCenter(_dragTargets);
        _hasRotateStart = false;

        if (Mode == GizmoMode.Translate || Mode == GizmoMode.Scale)
        {
            _dragStartIntersection = ClosestPointOnAxis(ray, _dragOrigin, _dragAxes[_hoveredAxis]);
            return;
        }

        if (Mode == GizmoMode.Rotate)
        {
            var axis = _dragAxes[_hoveredAxis];
            var hitPoint = RayPlaneIntersection(ray, _dragOrigin, axis);
            if (hitPoint.HasValue)
            {
                var dir = Vector3.Normalize(hitPoint.Value - _dragOrigin);
                _dragStartAngle = AngleOnPlane(dir, axis);
                _hasRotateStart = true;
                return;
            }

            _isDragging = false;
            _dragTargets.Clear();
        }
    }

    private void ApplyDrag(Ray ray)
    {
        if (_dragTargets.Count == 0 || _hoveredAxis < 0) return;

        switch (Mode)
        {
            case GizmoMode.Translate:
                ApplyTranslateDrag(ray);
                break;
            case GizmoMode.Rotate:
                ApplyRotateDrag(ray);
                break;
            case GizmoMode.Scale:
                ApplyScaleDrag(ray);
                break;
        }
    }

    private void ApplyTranslateDrag(Ray ray)
    {
        var axis = _dragAxes[_hoveredAxis];
        var currentPoint = ClosestPointOnAxis(ray, _dragOrigin, axis);
        float signedDistance = Vector3.Dot(currentPoint - _dragStartIntersection, axis);
        var worldDelta = axis * signedDistance;

        if (MultiMode == MultiTransformMode.Relative)
        {
            ApplyWorldTranslationToTargets(worldDelta);
            return;
        }

        if (Space == GizmoSpace.World)
        {
            ApplyWorldTranslationToTargets(worldDelta);
            return;
        }

        foreach (var target in _dragTargets)
        {
            var targetAxis = GetEntityLocalAxisWorld(target.Entity, _hoveredAxis);
            var localModeDelta = targetAxis * signedDistance;
            ApplyWorldTranslation(target, localModeDelta);
        }
    }

    private void ApplyRotateDrag(Ray ray)
    {
        if (!_hasRotateStart) return;

        var axis = _dragAxes[_hoveredAxis];
        var hitPoint = RayPlaneIntersection(ray, _dragOrigin, axis);
        if (!hitPoint.HasValue) return;

        var dir = Vector3.Normalize(hitPoint.Value - _dragOrigin);
        float currentAngle = AngleOnPlane(dir, axis);
        float deltaAngle = currentAngle - _dragStartAngle;

        if (MathF.Abs(deltaAngle) < 1e-6f)
            return;

        if (MultiMode == MultiTransformMode.Relative)
        {
            var groupRotation = Quaternion.CreateFromAxisAngle(axis, deltaAngle);
            foreach (var target in _dragTargets)
            {
                var rotatedOffset = Vector3.Transform(target.StartWorldPosition - _dragPivot, groupRotation);
                var newWorldPosition = _dragPivot + rotatedOffset;
                var newWorldRotation = Quaternion.Normalize(groupRotation * target.StartWorldRotation);
                SetWorldTransform(target, newWorldPosition, newWorldRotation);
            }

            return;
        }

        if (Space == GizmoSpace.World)
        {
            foreach (var target in _dragTargets)
            {
                if (target.HasParentInverse)
                {
                    var localAxis = Vector3.Normalize(Vector3.TransformNormal(axis, target.ParentInverseWorldMatrix));
                    target.Entity.Transform.LocalRotation = Quaternion.Normalize(
                        Quaternion.CreateFromAxisAngle(localAxis, deltaAngle) * target.StartLocalRotation);
                }
                else
                {
                    target.Entity.Transform.LocalRotation = Quaternion.Normalize(
                        Quaternion.CreateFromAxisAngle(axis, deltaAngle) * target.StartLocalRotation);
                }
            }

            return;
        }

        var localRotateAxis = GetLocalScaleOrRotateAxis(_hoveredAxis);
        foreach (var target in _dragTargets)
        {
            target.Entity.Transform.LocalRotation = Quaternion.Normalize(
                Quaternion.CreateFromAxisAngle(localRotateAxis, deltaAngle) * target.StartLocalRotation);
        }
    }

    private void ApplyScaleDrag(Ray ray)
    {
        var axis = _dragAxes[_hoveredAxis];
        var currentPoint = ClosestPointOnAxis(ray, _dragOrigin, axis);
        float signedDistance = Vector3.Dot(currentPoint - _dragStartIntersection, axis);
        var scaleAxis = GetLocalScaleOrRotateAxis(_hoveredAxis);

        if (MultiMode == MultiTransformMode.Relative)
        {
            float scaleFactor = MathF.Max(RelativeScaleFactorMin, 1f + signedDistance);
            foreach (var target in _dragTargets)
            {
                var offset = target.StartWorldPosition - _dragPivot;
                var parallel = Vector3.Dot(offset, axis) * axis;
                var perpendicular = offset - parallel;
                var newWorldPosition = _dragPivot + perpendicular + parallel * scaleFactor;
                target.Entity.Transform.LocalPosition = WorldToLocalPosition(target, newWorldPosition);

                // In relative mode, derive each target's local scale direction from the gizmo world axis
                // so rotated selections scale consistently instead of using one fixed local component.
                var projectedLocalDir = GetProjectedLocalScaleDirection(target, axis, scaleAxis);
                var localScaleFactor = Vector3.One + projectedLocalDir * signedDistance;
                var scaledLocal = new Vector3(
                    target.StartLocalScale.X * MathF.Max(RelativeScaleFactorMin, localScaleFactor.X),
                    target.StartLocalScale.Y * MathF.Max(RelativeScaleFactorMin, localScaleFactor.Y),
                    target.StartLocalScale.Z * MathF.Max(RelativeScaleFactorMin, localScaleFactor.Z));
                target.Entity.Transform.LocalScale = ClampLocalScale(scaledLocal);
            }

            return;
        }

        foreach (var target in _dragTargets)
        {
            target.Entity.Transform.LocalScale = ClampLocalScale(target.StartLocalScale + scaleAxis * signedDistance);
        }
    }

    private void ApplyWorldTranslationToTargets(Vector3 worldDelta)
    {
        foreach (var target in _dragTargets)
            ApplyWorldTranslation(target, worldDelta);
    }

    private static void ApplyWorldTranslation(DragTargetState target, Vector3 worldDelta)
    {
        if (target.HasParentInverse)
        {
            var localDelta = Vector3.TransformNormal(worldDelta, target.ParentInverseWorldMatrix);
            target.Entity.Transform.LocalPosition = target.StartLocalPosition + localDelta;
        }
        else
        {
            target.Entity.Transform.LocalPosition = target.StartLocalPosition + worldDelta;
        }
    }

    private static Vector3 WorldToLocalPosition(DragTargetState target, Vector3 worldPosition)
    {
        if (!target.HasParentInverse)
            return worldPosition;
        return Vector3.Transform(worldPosition, target.ParentInverseWorldMatrix);
    }

    private static Quaternion WorldToLocalRotation(DragTargetState target, Quaternion worldRotation)
    {
        if (target.Parent == null)
            return worldRotation;

        var parentWorld = target.Parent.WorldRotation;
        var parentInverse = Quaternion.Inverse(parentWorld);
        return Quaternion.Normalize(parentInverse * worldRotation);
    }

    private static void SetWorldTransform(DragTargetState target, Vector3 worldPosition, Quaternion worldRotation)
    {
        target.Entity.Transform.LocalPosition = WorldToLocalPosition(target, worldPosition);
        target.Entity.Transform.LocalRotation = WorldToLocalRotation(target, worldRotation);
    }

    private Vector3 GetGizmoOrigin(IReadOnlyList<Entity> selectedRoots, Entity primary)
    {
        if (selectedRoots.Count > 1 && MultiMode == MultiTransformMode.Relative)
        {
            Vector3 sum = Vector3.Zero;
            foreach (var entity in selectedRoots)
                sum += entity.Transform.WorldPosition;
            return sum / selectedRoots.Count;
        }

        return primary.Transform.WorldPosition;
    }

    private Vector3[] GetGizmoAxes(Entity primary)
    {
        if (Space == GizmoSpace.Local)
        {
            return
            [
                primary.Transform.Right,
                primary.Transform.Up,
                -primary.Transform.Forward
            ];
        }

        return [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];
    }

    private static Vector3 GetEntityLocalAxisWorld(Entity entity, int axisIndex)
    {
        return axisIndex switch
        {
            0 => entity.Transform.Right,
            1 => entity.Transform.Up,
            2 => -entity.Transform.Forward,
            _ => Vector3.Zero
        };
    }

    private static Vector3 GetLocalScaleOrRotateAxis(int axisIndex)
    {
        return axisIndex switch
        {
            0 => Vector3.UnitX,
            1 => Vector3.UnitY,
            2 => Vector3.UnitZ,
            _ => Vector3.Zero
        };
    }

    private static DragTargetState CaptureTargetState(Entity entity)
    {
        var transform = entity.Transform;
        var parent = transform.Parent;
        var hasParentInverse = false;
        var parentInverseWorld = Matrix4x4.Identity;

        if (parent != null)
        {
            var parentWorld = parent.WorldMatrix;
            if (Matrix4x4.Invert(parentWorld, out parentInverseWorld))
                hasParentInverse = true;
        }

        var state = new DragTargetState
        {
            Entity = entity,
            Parent = parent,
            StartLocalPosition = transform.LocalPosition,
            StartLocalRotation = transform.LocalRotation,
            StartLocalScale = transform.LocalScale,
            StartWorldPosition = transform.WorldPosition,
            StartWorldRotation = transform.WorldRotation,
            ParentInverseWorldMatrix = parentInverseWorld,
            HasParentInverse = hasParentInverse
        };

        return state;
    }

    private static Vector3 ComputeSelectionCenter(IReadOnlyList<DragTargetState> targets)
    {
        if (targets.Count == 0) return Vector3.Zero;

        Vector3 sum = Vector3.Zero;
        foreach (var target in targets)
            sum += target.StartWorldPosition;
        return sum / targets.Count;
    }

    private static List<Entity> FilterToSelectionRoots(IReadOnlyList<Entity> selected)
    {
        var selectedIds = new HashSet<Guid>(selected.Select(e => e.Id));
        var roots = new List<Entity>(selected.Count);
        foreach (var entity in selected)
        {
            if (HasSelectedAncestor(entity, selectedIds))
                continue;
            roots.Add(entity);
        }

        return roots;
    }

    private static bool HasSelectedAncestor(Entity entity, HashSet<Guid> selectedIds)
    {
        var current = entity.Transform.Parent;
        while (current != null)
        {
            if (selectedIds.Contains(current.Entity.Id))
                return true;
            current = current.Parent;
        }

        return false;
    }

    private static Vector3 GetProjectedLocalScaleDirection(DragTargetState target, Vector3 worldAxis, Vector3 fallbackLocalAxis)
    {
        var inverseWorldRotation = Quaternion.Inverse(target.StartWorldRotation);
        var localAxis = Vector3.Transform(worldAxis, inverseWorldRotation);
        var direction = new Vector3(MathF.Abs(localAxis.X), MathF.Abs(localAxis.Y), MathF.Abs(localAxis.Z));

        if (direction.LengthSquared() < 1e-6f)
            return fallbackLocalAxis;

        return Vector3.Normalize(direction);
    }

    private static Vector3 ClampLocalScale(Vector3 value)
    {
        return new Vector3(
            MathF.Max(MinLocalScale, value.X),
            MathF.Max(MinLocalScale, value.Y),
            MathF.Max(MinLocalScale, value.Z));
    }

    private void ResetInteractionState()
    {
        _hoveredAxis = -1;
        _isDragging = false;
        _dragTargets.Clear();
        _hasRotateStart = false;
    }

    private void DrawTranslateGizmo(Vector3 origin, Vector3[] axes, float scale)
    {
        for (int i = 0; i < 3; i++)
        {
            var color = (_hoveredAxis == i) ? HoverColor : AxisColors[i];
            var end = origin + axes[i] * ArrowLength * scale;
            var headEnd = origin + axes[i] * (ArrowLength + ArrowHeadLength) * scale;

            float r = ShaftRadius * scale;
            Raylib.DrawCylinderEx(origin, end, r, r, 6, color);
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

            var perp1 = GetPerpendicular(axis);
            var perp2 = Vector3.Cross(axis, perp1);

            float r = RotateLineRadius * scale;
            Vector3 prevPoint = origin + perp1 * radius;
            for (int j = 1; j <= CircleSegments; j++)
            {
                float angle = j * MathF.PI * 2f / CircleSegments;
                var point = origin + (perp1 * MathF.Cos(angle) + perp2 * MathF.Sin(angle)) * radius;
                Raylib.DrawCylinderEx(prevPoint, point, r, r, 4, color);
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

            float r = ShaftRadius * scale;
            Raylib.DrawCylinderEx(origin, end, r, r, 6, color);

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

    private static Vector3 ClosestPointOnAxis(Ray ray, Vector3 axisOrigin, Vector3 axisDir)
    {
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
        float tSeg;
        float tRay;

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

    private sealed class DragTargetState
    {
        public required Entity Entity { get; init; }
        public required Vector3 StartLocalPosition { get; init; }
        public required Quaternion StartLocalRotation { get; init; }
        public required Vector3 StartLocalScale { get; init; }
        public required Vector3 StartWorldPosition { get; init; }
        public required Quaternion StartWorldRotation { get; init; }
        public required TransformComponent? Parent { get; init; }
        public Matrix4x4 ParentInverseWorldMatrix { get; init; }
        public bool HasParentInverse { get; init; }
    }
}
