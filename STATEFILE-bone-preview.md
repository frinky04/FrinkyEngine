# Bone Preview Feature - State File

## Status: Complete

## Plan
1. Add `ToggleBonePreview` to `EditorAction` enum
2. Add `IsBonePreviewEnabled` property + `ToggleBonePreview()` to `EditorApplication`
3. Add `ShowBonePreview` to `EditorProjectSettings` (persist, clone, normalize, default)
4. Register keybind (F9) and action in `EditorApplication.RegisterKeybindActions()`
5. Whitelist `ToggleBonePreview` in `KeybindManager.CanProcessActionInCurrentMode()`
6. Add menu item in `MenuBar` under View menu
7. Draw bones in `EditorGizmos` (new `DrawBones` method)
8. Call bone drawing from `ViewportPanel` render callback
9. Add bone hierarchy tree in inspector via `ComponentDrawerRegistry` or `DrawReflection` hook
10. Update docs
11. Build and fix errors

## Files to Modify
- `src/FrinkyEngine.Editor/Keybind.cs` - EditorAction enum
- `src/FrinkyEngine.Editor/EditorApplication.cs` - property, toggle, register
- `src/FrinkyEngine.Editor/EditorProjectSettings.cs` - ShowBonePreview
- `src/FrinkyEngine.Editor/KeybindManager.cs` - default keybind, whitelist
- `src/FrinkyEngine.Editor/Panels/MenuBar.cs` - View menu item
- `src/FrinkyEngine.Editor/EditorGizmos.cs` - DrawBones method
- `src/FrinkyEngine.Editor/Panels/ViewportPanel.cs` - call DrawBones
- `src/FrinkyEngine.Editor/Panels/ComponentDrawerRegistry.cs` - bone hierarchy tree
