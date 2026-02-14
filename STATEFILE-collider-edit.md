# Collider Edit Mode - State File

## Status: COMPLETE

## What Was Implemented

### Mode Toggle
- Added `ToggleColliderEditMode` to `EditorAction` enum (Keybind.cs)
- Added `IsColliderEditModeEnabled` property + `ToggleColliderEditMode()` method to `EditorApplication`
- Added `ColliderEditMode` to `EditorProjectSettings` for persistence across sessions
- Default keybind: F9
- Whitelisted in play mode via `CanProcessActionInCurrentMode`
- Menu item in View menu with shortcut text

### Collider Visualization
- In collider edit mode, all colliders are rendered with wireframes (forces `PhysicsHitboxDrawMode.All`)
- Selected entity's collider highlighted via existing hitbox color system

### Collider Manipulation
- Created `ColliderEditSystem` using ImGuizmo Scale+Translate in Local mode
- Box colliders: scale handles adjust `Size`, translate moves `Center` offset
- Sphere colliders: scale adjusts uniform `Radius`, translate moves `Center`
- Capsule colliders: scale X/Z adjusts `Radius`, scale Y adjusts `Length`, translate moves `Center`
- All manipulations correctly account for entity world transform (position, rotation, scale)

### Integration
- Normal entity transform gizmo is replaced by collider gizmo while mode is active
- Inspector gizmo handles are hidden in collider edit mode
- Viewport picking guards against collider edit drag state
- Undo batching tracked via `TrackDragUndo` (same pattern as gizmo and inspector gizmo drags)
- Collider property setters call `MarkColliderDirty()` which syncs with physics system

### Documentation
- Updated `docs/editor-guide.md` with Collider Edit Mode section and keyboard shortcut table

## Files Modified
- `src/FrinkyEngine.Editor/Keybind.cs`
- `src/FrinkyEngine.Editor/EditorApplication.cs`
- `src/FrinkyEngine.Editor/EditorProjectSettings.cs`
- `src/FrinkyEngine.Editor/KeybindManager.cs`
- `src/FrinkyEngine.Editor/Panels/MenuBar.cs`
- `src/FrinkyEngine.Editor/Panels/ViewportPanel.cs`
- `docs/editor-guide.md`

## Files Created
- `src/FrinkyEngine.Editor/ColliderEditSystem.cs`
