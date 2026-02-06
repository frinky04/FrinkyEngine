# FrinkyEngine

A 3D game engine framework built with C#, .NET 8, and [Raylib](https://www.raylib.com/). Features a Dear ImGui editor, composition-based entity/component architecture, JSON scene serialization, and a `dotnet new` template for game projects.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

```bash
dotnet build FrinkyEngine.sln
```

## Running the Editor

```bash
dotnet run --project src/FrinkyEngine.Editor
```

The editor opens a 1600x900 window with dockable panels:

- **Viewport** -- 3D scene view with transform gizmos. Right-click + WASD to fly, QE for up/down, scroll to zoom.
- **Hierarchy** -- Entity tree. Click to select, right-click to create entities.
- **Inspector** -- Edit the selected entity's components. Supports custom drawers for built-in types and reflection-based editing for user-defined components.
- **Asset Browser** -- Browse and manage project assets with file-watching for external changes.
- **Console** -- Color-coded log output.
- **Menu Bar** -- File (New/Open/Save Scene, Open Project), Edit, Window, Play/Stop. Configurable keybindings.

## Running the Runtime

```bash
dotnet run --project src/FrinkyEngine.Runtime -- path/to/MyGame.fproject
```

The runtime loads a `.fproject` file, loads the game assembly and default scene, and runs the game loop.

## Creating a Game Project

Install the template pack and scaffold a new project:

```bash
dotnet new install ./templates/FrinkyEngine.Templates
dotnet new frinky-game -n MyGame
```

This creates:

```
MyGame/
  MyGame.csproj          # Class library referencing FrinkyEngine.Core
  MyGame.fproject        # Project file (points to default scene and game DLL)
  Assets/Scenes/
    MainScene.fscene     # Default scene with a camera and directional light
  Scripts/
    RotatorComponent.cs  # Example component that rotates an entity
```

Add a project reference to FrinkyEngine.Core in `MyGame.csproj`, build, then open `MyGame.fproject` in the editor.

## Project Structure

```
FrinkyEngine/
  FrinkyEngine.sln
  Directory.Build.props        # Shared: net8.0, C# 12, nullable, unsafe

  src/
    FrinkyEngine.Core/         # Shared engine library
      ECS/                     # Component base class (with EditorOnly flag), Entity
      Components/              # Transform, Camera, MeshRenderer, Light, Primitives, Materials
      Scene/                   # Scene, SceneManager, ComponentRegistry
      Rendering/               # SceneRenderer, FrinkyLog, MaterialType
      Serialization/           # JSON scene serializer, type resolver, converters
      Scripting/               # GameAssemblyLoader (collectible AssemblyLoadContext)
      Assets/                  # AssetManager, AssetDatabase, ProjectFile
      Input/                   # Static input wrapper
      FrinkyMath.cs            # Shared math utilities

    FrinkyEngine.Editor/       # ImGui editor executable
      Panels/                  # Viewport, Hierarchy, Inspector, Console, AssetBrowser, MenuBar
      EditorApplication.cs     # Central editor state, Play/Stop mode
      EditorCamera.cs          # Free-fly editor camera
      EditorGizmos.cs          # Transform gizmo rendering
      GizmoSystem.cs           # Gizmo interaction logic
      KeybindManager.cs        # Configurable keybindings
      NotificationManager.cs   # Toast notification system
      AssetFileWatcher.cs      # Watches project files for external changes
      ScriptBuilder.cs         # Compiles game scripts
      ScriptCreator.cs         # Creates new script files from templates
      ProjectScaffolder.cs     # Scaffolds new game projects
      ImGuiDockBuilder.cs      # Custom P/Invoke bindings for dock layout

    FrinkyEngine.Runtime/      # Standalone game player executable

  templates/
    FrinkyEngine.Templates/    # dotnet new template pack

  Shaders/
    lighting.vs, lighting.fs   # Basic lighting (ambient + 4 point/directional lights)

  EditorAssets/
    Fonts/
      JetBrains_Mono/          # Editor UI font

  Games/
    Testing/                   # Example game project
```

## Architecture

Entities own a flat list of **Components**. Every entity always has a `TransformComponent` with parent-child hierarchy support. Components have lifecycle methods:

| Method | Called |
|---|---|
| `Awake()` | When the component is added to an entity |
| `Start()` | Once, before the first `Update` |
| `Update(dt)` | Every frame |
| `LateUpdate(dt)` | Every frame, after all `Update` calls |
| `OnEnable()` / `OnDisable()` | When `Enabled` changes |
| `OnDestroy()` | When the component is removed |

Components can be marked `EditorOnly` so they are excluded from runtime builds.

A **Scene** holds entities and maintains quick-access lists for cameras, lights, and renderers. The **SceneRenderer** draws the scene using Raylib with a bundled lighting shader. The renderer supports an editor mode that respects the `EditorOnly` flag.

### Rendering

Built-in renderable components include `MeshRendererComponent` and a set of **primitive shapes** (Cube, Sphere, Cylinder, Plane) that share a common `PrimitiveComponent` base. Materials are configured via `MaterialSlot` with support for different `MaterialType` presets.

### Serialization

Scenes are saved as `.fscene` JSON files. The serializer uses reflection to persist all public read/write properties on components, with custom converters for `Vector3`, `Quaternion`, and `Color`. A `$type` discriminator enables polymorphic component deserialization.

### Game Scripting

Game projects compile to a .NET class library. The engine loads the DLL at runtime via `AssemblyLoadContext`, making all `Component` subclasses available to the serializer and the editor's Add Component menu. The editor can compile game scripts and create new script files from templates.

### Editor Features

- **Gizmos** -- Transform gizmos for manipulating entity position, rotation, and scale in the viewport.
- **Keybindings** -- Configurable keyboard shortcuts stored per-project in `.frinky/keybinds.json`.
- **Notifications** -- Toast notification system for editor feedback.
- **Asset file watching** -- Detects external changes to project files and reloads assets automatically.
- **Project scaffolding** -- Create new game projects from within the editor.

### Play Mode

The editor snapshots the scene to JSON before entering play mode and restores it on stop, so runtime changes don't persist.

## Dependencies

| Project | Packages |
|---|---|
| Core | Raylib-cs 7.0.2 |
| Editor | rlImGui-cs 3.2.0 (brings Raylib-cs + ImGui.NET) |
| Runtime | Raylib-cs 7.0.2 |

## License

Unlicensed. All rights reserved.
