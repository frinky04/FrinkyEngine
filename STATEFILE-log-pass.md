# Log Pass - State File

## Status: In Progress

## Tasks
1. **Error count notification** - Persistent incrementing notification showing error count in editor
2. **Turn Raylib logs off by default** - Add `r_raylib_logs` CVar, default off
3. **On-screen debug text system** - Debug.PrintString overlay, editor-only, no-op in runtime
4. **Console enhancements** - Log search/filter, entry count, timestamps toggle, etc.

## Key Files
- `src/FrinkyEngine.Core/Rendering/FrinkyLog.cs` - Central logging
- `src/FrinkyEngine.Core/Rendering/RaylibLogger.cs` - Raylib trace log redirect
- `src/FrinkyEngine.Core/UI/EngineOverlays.cs` - Console overlays + CVar registration
- `src/FrinkyEngine.Core/UI/Console/ConsoleBackend.cs` - Console command system
- `src/FrinkyEngine.Editor/NotificationManager.cs` - Editor notifications
- `src/FrinkyEngine.Editor/Panels/ConsolePanel.cs` - Editor console log panel
- `src/FrinkyEngine.Editor/EditorApplication.cs` - Editor main app

## Architecture Decisions
- Debug.PrintString: Core API with interface/callback in Core, implementation in Editor
- RaylibLogger: Add enabled flag, controlled by CVar
- Error count: Use NotificationManager persistent notification, subscribe to FrinkyLog.OnLog
