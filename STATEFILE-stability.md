# Stability Branch Statefile

## Status: Complete

## Issue 1: Undo/Redo causing full scene reload (200ms spike)
- **Status**: Fixed
- **Root cause**: `UndoRedoManager.RestoreSnapshot()` fully deserializes the scene from JSON, creating brand-new component instances. `MeshRendererComponent` with skinned meshes calls `LoadModelUnique()` which always reloads from disk (bypasses AssetManager cache). Post-process effects are also recreated uninitialized, triggering shader recompilation.
- **Fix**: Transfer loaded model instances and initialized post-process effects from the old scene to the restored scene during undo/redo, matching entities by GUID. Old renderables that had their models transferred are cleared so `Invalidate()` becomes a no-op for them. Post-process effects are swapped (old initialized effect replaces new uninitialized one, with deserialized property values copied over).
- **Files changed**:
  - `src/FrinkyEngine.Core/Components/MeshRendererComponent.cs` - Added `HasLoadedModel`, `TransferModelFrom()`
  - `src/FrinkyEngine.Editor/UndoRedoManager.cs` - Added `TransferLoadedAssets()`, `TransferPostProcessEffects()`, `CopyEffectProperties()`

## Issue 2: Unresolved component type cleanup
- **Status**: Fixed
- **Storage mechanism**: `Entity.UnresolvedComponents` (List<ComponentData>) stores raw JSON for component types that couldn't be resolved during deserialization. These are re-serialized on save to avoid data loss.
- **Fix approach**: Two cleanup paths:
  1. Inspector: per-entity UI shows unresolved components with individual "Remove" buttons and "Remove All Unresolved" bulk action
  2. Menu: Edit > Clean Up Unresolved Components for scene-wide cleanup
  Both paths are undoable.
- **Files changed**:
  - `src/FrinkyEngine.Core/ECS/Entity.cs` - Made `UnresolvedComponents` public, added `HasUnresolvedComponents`
  - `src/FrinkyEngine.Editor/Panels/InspectorPanel.cs` - Added `DrawUnresolvedComponents()`
  - `src/FrinkyEngine.Editor/Panels/MenuBar.cs` - Added Edit > Clean Up Unresolved Components menu item
