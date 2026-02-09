using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using Hexa.NET.ImGuizmo;
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
    private const float MinLocalScale = 0.001f;

    public GizmoMode Mode { get; set; } = GizmoMode.Translate;
    public GizmoSpace Space { get; set; } = GizmoSpace.World;
    public MultiTransformMode MultiMode { get; set; } = MultiTransformMode.Independent;
    public bool IsDragging => ImGuizmo.IsUsing();
    public int HoveredAxis => ImGuizmo.IsOver() ? 0 : -1;

    public bool SnapTranslation;
    public bool SnapRotation;
    public bool SnapScale;
    public float TranslationSnapValue = 1.0f;
    public float RotationSnapValue = 15.0f;
    public float ScaleSnapValue = 0.25f;

    public static readonly float[] TranslationSnapPresets = { 0.1f, 0.25f, 0.5f, 1.0f, 2.0f, 5.0f, 10.0f };
    public static readonly float[] RotationSnapPresets = { 5f, 10f, 15f, 30f, 45f, 90f };
    public static readonly float[] ScaleSnapPresets = { 0.05f, 0.1f, 0.25f, 0.5f, 1.0f };

    // Track previous-frame dragging so we can detect transitions for delta application
    private bool _wasDragging;
    // Snapshot of each entity's transforms at drag start, for delta-based application
    private readonly List<DragStartSnapshot> _dragSnapshots = new();

    public unsafe void DrawAndUpdate(
        Camera3D camera,
        IReadOnlyList<Entity> selected,
        Entity? primary,
        Vector2 viewportScreenPos,
        Vector2 viewportSize)
    {
        if (primary == null || selected.Count == 0 || Mode == GizmoMode.None)
        {
            _wasDragging = false;
            _dragSnapshots.Clear();
            return;
        }

        var selectedRoots = FilterToSelectionRoots(selected);
        if (selectedRoots.Count == 0)
        {
            _wasDragging = false;
            _dragSnapshots.Clear();
            return;
        }

        // Configure ImGuizmo for this viewport
        ImGuizmo.SetRect(viewportScreenPos.X, viewportScreenPos.Y, viewportSize.X, viewportSize.Y);
        ImGuizmo.SetOrthographic(false);
        ImGuizmo.SetDrawlist();
        ImGuizmo.Enable(true);

        // Build view and projection matrices
        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        float aspect = viewportSize.X / viewportSize.Y;
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FovY * FrinkyEngine.Core.FrinkyMath.Deg2Rad, aspect, 0.01f, 1000f);

        // Map GizmoMode to ImGuizmo operation
        var op = Mode switch
        {
            GizmoMode.Translate => ImGuizmoOperation.Translate,
            GizmoMode.Rotate => ImGuizmoOperation.Rotate,
            GizmoMode.Scale => ImGuizmoOperation.Scale,
            _ => ImGuizmoOperation.Translate
        };

        // Map GizmoSpace to ImGuizmo mode; Scale always forces Local
        var mode = (Space == GizmoSpace.Local || Mode == GizmoMode.Scale)
            ? ImGuizmoMode.Local
            : ImGuizmoMode.World;

        // Build snap values
        float* snapPtr = null;
        float snapX = 0, snapY = 0, snapZ = 0;
        if (Mode == GizmoMode.Translate && IsSnapActive(SnapTranslation))
        {
            snapX = snapY = snapZ = TranslationSnapValue;
            snapPtr = &snapX;
        }
        else if (Mode == GizmoMode.Rotate && IsSnapActive(SnapRotation))
        {
            snapX = snapY = snapZ = RotationSnapValue;
            snapPtr = &snapX;
        }
        else if (Mode == GizmoMode.Scale && IsSnapActive(SnapScale))
        {
            snapX = snapY = snapZ = ScaleSnapValue;
            snapPtr = &snapX;
        }

        // Build the gizmo object matrix
        Matrix4x4 objectMatrix;
        bool isMulti = selectedRoots.Count > 1;
        bool isRelative = isMulti && MultiMode == MultiTransformMode.Relative;

        if (isRelative)
        {
            var center = ComputeSelectionCenter(selectedRoots);
            objectMatrix = Matrix4x4.CreateTranslation(center);
        }
        else
        {
            objectMatrix = primary.Transform.WorldMatrix;
        }

        // Call Manipulate — use pointer overload so we can pass snapPtr (which may be null)
        var deltaMatrix = Matrix4x4.Identity;
        bool changed = ImGuizmo.Manipulate(
            (float*)&view, (float*)&proj, op, mode,
            (float*)&objectMatrix, (float*)&deltaMatrix,
            snapPtr, (float*)null, (float*)null);

        // Detect drag start: transition from not-dragging to dragging
        bool currentlyDragging = ImGuizmo.IsUsing();
        if (currentlyDragging && !_wasDragging)
        {
            // Drag just started — capture snapshots
            _dragSnapshots.Clear();
            foreach (var entity in selectedRoots)
            {
                _dragSnapshots.Add(new DragStartSnapshot
                {
                    Entity = entity,
                    StartWorldPosition = entity.Transform.WorldPosition,
                    StartWorldRotation = entity.Transform.WorldRotation,
                    StartLocalScale = entity.Transform.LocalScale,
                });
            }
        }

        if (!currentlyDragging)
        {
            _dragSnapshots.Clear();
        }

        _wasDragging = currentlyDragging;

        // Apply the result
        if (changed)
        {
            if (!isMulti)
            {
                // Single entity: apply the full object matrix directly
                ApplySingleEntity(selectedRoots[0], objectMatrix);
            }
            else
            {
                // Multi-entity: apply delta to each entity
                ApplyMultiEntityDelta(selectedRoots, deltaMatrix, isRelative);
            }
        }
    }

    private static void ApplySingleEntity(Entity entity, Matrix4x4 newWorldMatrix)
    {
        var parent = entity.Transform.Parent;
        Matrix4x4 localMatrix;

        if (parent != null)
        {
            // Compute new local matrix by removing parent's world transform
            if (Matrix4x4.Invert(parent.WorldMatrix, out var parentInverse))
                localMatrix = newWorldMatrix * parentInverse;
            else
                localMatrix = newWorldMatrix;
        }
        else
        {
            localMatrix = newWorldMatrix;
        }

        if (Matrix4x4.Decompose(localMatrix, out var scale, out var rotation, out var translation))
        {
            entity.Transform.LocalPosition = translation;
            entity.Transform.LocalRotation = Quaternion.Normalize(rotation);
            entity.Transform.LocalScale = ClampLocalScale(scale);
        }
    }

    private void ApplyMultiEntityDelta(IReadOnlyList<Entity> roots, Matrix4x4 deltaMatrix, bool isRelative)
    {
        if (!Matrix4x4.Decompose(deltaMatrix, out var deltaScale, out var deltaRotation, out var deltaTranslation))
            return;

        // The delta translation is the full translation component of the delta matrix
        var translation = new Vector3(deltaMatrix.M41, deltaMatrix.M42, deltaMatrix.M43);

        switch (Mode)
        {
            case GizmoMode.Translate:
                foreach (var entity in roots)
                    entity.Transform.WorldPosition += translation;
                break;

            case GizmoMode.Rotate:
                if (isRelative)
                {
                    // Rotate all entities around the group pivot
                    var pivot = ComputeSelectionCenter(roots);
                    foreach (var entity in roots)
                    {
                        var offset = entity.Transform.WorldPosition - pivot;
                        var rotatedOffset = Vector3.Transform(offset, deltaRotation);
                        entity.Transform.WorldPosition = pivot + rotatedOffset;
                        entity.Transform.WorldRotation = Quaternion.Normalize(
                            deltaRotation * entity.Transform.WorldRotation);
                    }
                }
                else
                {
                    // Independent: each entity rotates from its own origin
                    foreach (var entity in roots)
                    {
                        if (Space == GizmoSpace.World)
                        {
                            entity.Transform.WorldRotation = Quaternion.Normalize(
                                deltaRotation * entity.Transform.WorldRotation);
                        }
                        else
                        {
                            entity.Transform.LocalRotation = Quaternion.Normalize(
                                entity.Transform.LocalRotation * deltaRotation);
                        }
                    }
                }
                break;

            case GizmoMode.Scale:
                foreach (var entity in roots)
                {
                    var newScale = new Vector3(
                        entity.Transform.LocalScale.X * deltaScale.X,
                        entity.Transform.LocalScale.Y * deltaScale.Y,
                        entity.Transform.LocalScale.Z * deltaScale.Z);
                    entity.Transform.LocalScale = ClampLocalScale(newScale);
                }

                if (isRelative)
                {
                    // Also scale positions relative to pivot
                    var pivot = ComputeSelectionCenter(roots);
                    foreach (var snap in _dragSnapshots)
                    {
                        var offset = snap.StartWorldPosition - pivot;
                        var scaledOffset = new Vector3(
                            offset.X * deltaScale.X,
                            offset.Y * deltaScale.Y,
                            offset.Z * deltaScale.Z);
                        snap.Entity.Transform.WorldPosition = pivot + scaledOffset;
                    }
                }
                break;
        }
    }

    private static Vector3 ComputeSelectionCenter(IReadOnlyList<Entity> entities)
    {
        if (entities.Count == 0) return Vector3.Zero;

        var sum = Vector3.Zero;
        foreach (var entity in entities)
            sum += entity.Transform.WorldPosition;
        return sum / entities.Count;
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

    private static Vector3 ClampLocalScale(Vector3 value)
    {
        return new Vector3(
            MathF.Max(MinLocalScale, value.X),
            MathF.Max(MinLocalScale, value.Y),
            MathF.Max(MinLocalScale, value.Z));
    }

    private static bool IsSnapActive(bool snapEnabled)
    {
        bool ctrlHeld = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
        return snapEnabled ^ ctrlHeld;
    }

    private sealed class DragStartSnapshot
    {
        public required Entity Entity { get; init; }
        public required Vector3 StartWorldPosition { get; init; }
        public required Quaternion StartWorldRotation { get; init; }
        public required Vector3 StartLocalScale { get; init; }
    }
}
