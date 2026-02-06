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

- **Viewport** -- 3D scene view. Right-click + WASD to fly, QE for up/down, scroll to zoom.
- **Hierarchy** -- Entity tree. Click to select, right-click to create entities.
- **Inspector** -- Edit the selected entity's components. Supports custom drawers for built-in types and reflection-based editing for user-defined components.
- **Console** -- Color-coded log output.
- **Menu Bar** -- File (New/Open/Save Scene, Open Project), Edit, Window, Play/Stop.

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
      ECS/                     # Component base class, Entity
      Components/              # Transform, Camera, MeshRenderer, Light
      Scene/                   # Scene, SceneManager
      Rendering/               # SceneRenderer, FrinkyLog
      Serialization/           # JSON scene serializer, type resolver, converters
      Scripting/               # GameAssemblyLoader (collectible AssemblyLoadContext)
      Assets/                  # AssetManager, ProjectFile
      Input/                   # Static input wrapper

    FrinkyEngine.Editor/       # ImGui editor executable
      Panels/                  # Viewport, Hierarchy, Inspector, Console, MenuBar
      EditorApplication.cs     # Central editor state, Play/Stop mode
      EditorCamera.cs          # Free-fly editor camera

    FrinkyEngine.Runtime/      # Standalone game player executable

  templates/
    FrinkyEngine.Templates/    # dotnet new template pack

  Shaders/
    lighting.vs, lighting.fs   # Basic lighting (ambient + 4 point/directional lights)

  JetBrains_Mono/              # Editor UI font
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

A **Scene** holds entities and maintains quick-access lists for cameras, lights, and renderers. The **SceneRenderer** draws the scene using Raylib with a bundled lighting shader.

### Serialization

Scenes are saved as `.fscene` JSON files. The serializer uses reflection to persist all public read/write properties on components, with custom converters for `Vector3`, `Quaternion`, and `Color`. A `$type` discriminator enables polymorphic component deserialization.

### Game Scripting

Game projects compile to a .NET class library. The engine loads the DLL at runtime via `AssemblyLoadContext`, making all `Component` subclasses available to the serializer and the editor's Add Component menu.

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
