# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build entire solution
dotnet build FrinkyEngine.sln

# Run editor (optionally pass a .fproject path)
dotnet run --project src/FrinkyEngine.Editor
dotnet run --project src/FrinkyEngine.Editor -- path/to/Game.fproject

# Run runtime (requires .fproject argument)
dotnet run --project src/FrinkyEngine.Runtime -- path/to/Game.fproject

# Install and use game template
dotnet new install ./templates/FrinkyEngine.Templates
dotnet new frinky-game -n MyGame
```

There are no tests in this project.

## Architecture

**Solution layout**: Three projects in `src/` — Core (shared library), Editor (ImGui desktop app), Runtime (standalone game player) — plus a `dotnet new` template pack in `templates/`.

**Global build settings** are in `Directory.Build.props`: .NET 8, C# 12, nullable enabled, `AllowUnsafeBlocks=true` (required for Raylib pointer APIs).

### Core (FrinkyEngine.Core)

The engine uses **composition-based entities**, not strict ECS. An `Entity` holds a flat list of `Component` instances. Every entity always has a `TransformComponent` (cannot be removed). Components have Unity-style lifecycle: `Awake` → `Start` → `Update`/`LateUpdate` → `OnDestroy`.

**Scene** holds entities and maintains quick-access lists for cameras, lights, and renderers (auto-registered via `OnComponentAdded`/`OnComponentRemoved`). **SceneManager** is a singleton that owns the active scene and handles load/save.

**Serialization** uses `System.Text.Json` with reflection over public read/write properties. Components are discriminated by `$type`. Custom `JsonConverter<T>` implementations exist for `Vector3`, `Quaternion`, and `Color`. **ComponentTypeResolver** maps type names to `Type` objects via reflection and supports registering external assemblies.

**SceneRenderer** draws with Raylib using `Shaders/lighting.vs` and `lighting.fs`. Shader uniform locations are cached in `int` fields — do NOT access `Shader.Locs` directly (it's a pointer requiring unsafe). Supports up to 4 lights (directional/point) with Phong specular.

**GameAssemblyLoader** loads game DLLs via collectible `AssemblyLoadContext` for hot-reload. After loading, it registers the assembly with `ComponentTypeResolver`.

**AssetManager** and **ProjectFile** are singletons for resource loading and `.fproject` configuration.

### Editor (FrinkyEngine.Editor)

Entry point in `Program.cs` sets up Raylib (1600x900, MSAA4x) and rlImGui with docking.

**EditorApplication** is the central singleton managing scene state, editor camera, panels, and play mode. Play mode snapshots the scene to JSON and restores on stop.

**EditorCamera** is a free-fly camera (right-click + WASD/QE). Important: `DisableCursor()`/`EnableCursor()` must only be called on state transitions, not every frame.

**Panels**: ViewportPanel (renders to RenderTexture2D, displays via `rlImGui.ImageRenderTexture()`), HierarchyPanel (entity tree), InspectorPanel (component editing), ConsolePanel (log viewer), MenuBar (file ops, play/stop).

**ComponentDrawerRegistry** maps component types to custom ImGui drawing functions. Fallback is `DrawReflection()` which handles common types (float, int, bool, string, Vector2/3, Quaternion as Euler, Color, enums) via reflection.

**ImGuiDockBuilder** contains custom P/Invoke bindings to `cimgui.dll` for dock layout functions not exposed by ImGui.NET. Use `"cimgui"` as the DllImport library name with `CallingConvention.Cdecl`.

### Runtime (FrinkyEngine.Runtime)

Minimal standalone player. Loads `.fproject`, initializes AssetManager, loads game assembly and default scene, runs the game loop using the scene's MainCamera.

## Key API Pitfalls

- **Raylib-cs 7.0.2**: `Shader.Locs` is a pointer — must use `unsafe` context. Prefer storing locations in int fields.
- **rlImGui-cs 3.2.0**: namespace is `rlImGui_cs`, static class is `rlImGui`. Methods: `Setup(darkTheme, docking)`, `Begin(dt)`, `End()`, `Shutdown()`, `ImageRenderTexture()`, `ImageRenderTextureFit()`.
- **ImGui.NET 1.91.6.1**: `DockSpaceOverViewport` takes `(uint id, ImGuiViewportPtr, ...)` — pass `0` for default ID.
- **Raylib cursor**: `DisableCursor()`/`EnableCursor()` re-centers the mouse each call. Only call on state transitions.
- **Template .csproj**: needs `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>` to avoid compiling content/ .cs files.

## File Conventions

- Scenes: `.fscene` (JSON), Projects: `.fproject` (JSON)
- Shaders in `Shaders/` are copied to output via Content build items
- Editor font lives in `EditorAssets/Fonts/JetBrains_Mono/` (copied to output via the `EditorAssets` Content glob)
- Components go in `Components/`, panels in `Panels/`, serialization converters in `Serialization/`
