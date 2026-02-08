# FrinkyEngine

A C#/.NET 8 3D game engine with a Dear ImGui editor, standalone runtime, forward+ rendering, BEPU physics, post-processing, prefabs, and a `dotnet new` template for game projects.

## Features

- **Editor** with viewport, hierarchy, inspector, asset browser, console, and performance panels
- **Runtime** that runs from `.fproject` (dev) or `.fasset` (exported) files
- **Entity-Component** architecture with lifecycle hooks and hot-reloadable game assemblies
- **Forward+ Tiled Lighting** supporting hundreds of lights with directional, point, and skylight types
- **Post-Processing** pipeline with bloom, fog, and SSAO
- **BEPU Physics** with rigidbodies, colliders, and a character controller
- **Prefab System** with `.fprefab` files, override tracking, and drag-and-drop instantiation
- **Entity References** for cross-entity linking that survive serialization and prefab instantiation
- **Scene Serialization** to human-readable `.fscene` JSON
- **Export Pipeline** that packages games into a single `.exe` + `.fasset` archive

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows for `.bat` helper scripts (or use `dotnet` commands directly on any platform)

## Quick Start

1. **Build the solution.**

```powershell
.\build.bat
```
```bash
dotnet build FrinkyEngine.sln -c Release
```

2. **Launch the editor.**

```powershell
.\launch-editor.bat
```
```bash
dotnet run --project src/FrinkyEngine.Editor
```

3. **Open a project.**

```powershell
.\launch-editor.bat ExampleGames\PrefabTestingGround\PrefabTestingGround.fproject
```
```bash
dotnet run --project src/FrinkyEngine.Editor -- ExampleGames/PrefabTestingGround/PrefabTestingGround.fproject
```

4. **Build game scripts** from the editor (`Scripts -> Build Scripts`) or manually:

```bash
dotnet build path/to/MyGame.csproj --configuration Debug
```

5. **Run the runtime.**

```powershell
.\launch-runtime.bat ExampleGames\PrefabTestingGround\PrefabTestingGround.fproject
```
```bash
dotnet run --project src/FrinkyEngine.Runtime -- path/to/MyGame.fproject
```

## Command Cheat Sheet

| Goal | Windows script | Dotnet equivalent |
|---|---|---|
| Build all projects | `.\build.bat [debug]` | `dotnet build FrinkyEngine.sln -c Release` |
| Run editor | `.\launch-editor.bat [Game.fproject]` | `dotnet run --project src/FrinkyEngine.Editor -- [Game.fproject]` |
| Run runtime (dev mode) | `.\launch-runtime.bat Game.fproject` | `dotnet run --project src/FrinkyEngine.Runtime -- Game.fproject` |
| Install template | `.\install-template.bat` | `dotnet new install .\templates\FrinkyEngine.Templates --force` |
| Publish editor | `.\publish-editor.bat [rid] [outDir]` | `dotnet publish src/FrinkyEngine.Editor/FrinkyEngine.Editor.csproj -c Release -r win-x64 --self-contained false -o artifacts/release/editor/win-x64` |
| Publish runtime | `.\publish-runtime.bat [rid] [outDir]` | `dotnet publish src/FrinkyEngine.Runtime/FrinkyEngine.Runtime.csproj -c Release -r win-x64 -p:FrinkyExport=true --self-contained false -o artifacts/release/runtime/win-x64` |
| Package a game | `.\package-game.bat path\to\Game.fproject [outDir] [rid]` | Run build + publish + copy steps manually |
| Local release zips | `.\release-local.bat vX.Y.Z` | Run restore/build/publish/pack/zip steps manually |

## Create a New Game Project

Install the template:

```powershell
.\install-template.bat
```
```bash
dotnet new install ./templates/FrinkyEngine.Templates --force
```

Create a project:

```bash
dotnet new frinky-game -n MyGame
```

Template output:

```text
MyGame/
  MyGame.fproject
  MyGame.csproj
  Assets/
    Scenes/MainScene.fscene
    Scripts/
      RotatorComponent.cs
      CharacterControllerExample.cs
  .gitignore
```

The template `.csproj` includes a placeholder comment for `FrinkyEngine.Core`. Add a `ProjectReference` to your local `src/FrinkyEngine.Core/FrinkyEngine.Core.csproj`.

## Editor Overview

### Panels

| Panel | Description |
|---|---|
| **Viewport** | 3D scene rendering with transform gizmos (translate, rotate, scale) |
| **Hierarchy** | Entity tree with drag-and-drop reordering and parenting |
| **Inspector** | Component editing with custom drawers and reflection fallback |
| **Assets** | File browser with drag-and-drop for models, prefabs, and scenes |
| **Console** | Log stream viewer |
| **Performance** | Frame time and rendering statistics |

### Play and Simulate Modes

- **Play** (`Ctrl+P`) — runs gameplay from the scene's main camera, locks scene editing
- **Simulate** (`Ctrl+Shift+P`) — runs gameplay while keeping the editor camera and tools available
- The editor snapshots the scene before entering either mode and restores it on stop

## Architecture

FrinkyEngine uses a **composition-based entity model**. An `Entity` holds a flat list of `Component` instances. Every entity always has a `TransformComponent` (cannot be removed).

### Component Lifecycle

```
Awake → Start → Update / LateUpdate → OnDestroy
                 OnEnable / OnDisable
```

### Built-in Components

#### Rendering

| Component | Key Properties |
|---|---|
| `CameraComponent` | `FieldOfView` (60), `NearPlane` (0.1), `FarPlane` (1000), `Projection` (Perspective/Orthographic), `ClearColor`, `IsMain` |
| `LightComponent` | `LightType` (Directional/Point/Skylight), `LightColor`, `Intensity` (1.0), `Range` (10.0) |
| `MeshRendererComponent` | `ModelPath`, `MaterialSlots`, `Tint`, `EditorOnly` |
| `PostProcessStackComponent` | `PostProcessingEnabled` (true), `Effects` (list of effects) |

#### Primitives

| Component | Key Properties |
|---|---|
| `CubePrimitive` | `Width` (1), `Height` (1), `Depth` (1) |
| `SpherePrimitive` | `Radius` (0.5), `Rings` (16), `Slices` (16) |
| `PlanePrimitive` | `Width` (10), `Depth` (10), `ResolutionX` (1), `ResolutionZ` (1) |
| `CylinderPrimitive` | `Radius` (0.5), `Height` (2), `Slices` (16) |

#### Physics

| Component | Key Properties |
|---|---|
| `RigidbodyComponent` | `MotionType` (Dynamic/Kinematic/Static), `Mass` (1.0), `LinearDamping` (0.03), `AngularDamping` (0.03), `ContinuousDetection`, axis locks |
| `BoxColliderComponent` | `Size` (1,1,1), `Center`, `IsTrigger` |
| `SphereColliderComponent` | `Radius` (0.5), `Center`, `IsTrigger` |
| `CapsuleColliderComponent` | `Radius` (0.5), `Length` (1.0), `Center`, `IsTrigger` |
| `CharacterControllerComponent` | `MoveSpeed` (4), `JumpVelocity` (6), `MaxSlopeDegrees` (45), air control settings |

#### Input

| Component | Key Properties |
|---|---|
| `SimplePlayerInputComponent` | Movement keys, mouse look, `CameraEntity` (EntityReference), `UseCharacterController` |

### Materials

Each `MeshRendererComponent` has a list of `MaterialSlot` entries with three material types:

| Type | Description |
|---|---|
| `SolidColor` | Flat color (default) |
| `Textured` | Albedo texture mapped with UV coordinates |
| `TriplanarTexture` | Texture projected along world/local axes, configurable scale and blend sharpness |

## Rendering

The renderer uses **forward+ tiled lighting** with Raylib and GLSL shaders.

- **Tile size**, **max lights**, and **max lights per tile** are configurable in `project_settings.json`
- Default limits: 256 lights total, 64 per tile, 16px tile size
- **Light types**: Directional (parallel rays), Point (radial falloff), Skylight (ambient hemisphere)
- **Shading**: Phong specular with per-fragment lighting
- **Model formats**: OBJ, GLTF, GLB (loaded via Raylib)

## Post-Processing

Add a `PostProcessStackComponent` to a camera entity to enable post-processing. Effects are processed in order and can be individually toggled.

### Built-in Effects

**Bloom** — multi-pass threshold/downsample/upsample glow
| Property | Default |
|---|---|
| `Threshold` | 1.0 |
| `SoftKnee` | 0.5 |
| `Intensity` | 1.0 |
| `Iterations` | 5 |

**Fog** — distance-based atmospheric fog (requires depth)
| Property | Default |
|---|---|
| `FogColor` | (180, 190, 200) |
| `FogStart` | 10 |
| `FogEnd` | 100 |
| `Density` | 0.02 |
| `Mode` | Linear / Exponential / ExponentialSquared |

**Ambient Occlusion (SSAO)** — screen-space ambient occlusion with bilateral blur (requires depth)
| Property | Default |
|---|---|
| `Radius` | 20.0 |
| `Intensity` | 1.0 |
| `Bias` | 1.0 |
| `SampleCount` | 64 |
| `BlurSize` | 16 |

When no post-processing stack is active, rendering goes direct-to-screen with zero overhead.

## Physics

Physics is powered by [BepuPhysics 2](https://github.com/bepu/bepuphysics2).

### Rigidbodies

Add a `RigidbodyComponent` to give an entity physics behavior:

- **Dynamic** — fully simulated, affected by forces and collisions
- **Kinematic** — moves via transform, pushes dynamic bodies but is not affected by forces
- **Static** — immovable, used for terrain and walls

Kinematic stability notes:
- Contact-driving velocity is derived from consecutive kinematic target poses (continuity-aware), not from arbitrary pose snaps.
- Kinematic linear and angular contact velocities are safety-clamped to avoid extreme one-frame impulses.
- Large discontinuities (for example sudden large rotation jumps) are treated as teleport-style corrections for that step, with kinematic velocity suppressed.

### Colliders

- `BoxColliderComponent` — axis-aligned box
- `SphereColliderComponent` — sphere
- `CapsuleColliderComponent` — capsule (used for characters)

All colliders support `Center` offset and `IsTrigger` mode.

### Character Controller

A dynamic character controller backed by BEPU support constraints. Minimum setup on one entity:

1. `RigidbodyComponent` with `MotionType = Dynamic`
2. `CapsuleColliderComponent` (must be the first enabled collider)
3. `CharacterControllerComponent`

Script-side input methods:

- `AddMovementInput(direction)` / `Jump()` — Unreal-style
- `SetMoveInput(Vector2)` — direct planar input
- `MoveAndSlide(desiredVelocity, requestJump)` — Godot-style convenience

Or use `SimplePlayerInputComponent` for built-in WASD + mouse look with configurable keys.

## Prefabs

Prefabs store reusable entity hierarchies as `.fprefab` JSON files.

### Workflow

1. **Create**: right-click an entity in the hierarchy and select "Create Prefab", or use `PrefabSerializer.CreateFromEntity()`
2. **Instantiate**: drag a `.fprefab` from the asset browser into the viewport or hierarchy
3. **Override**: modify properties on a prefab instance — changes are tracked as overrides against the prefab source
4. **Update**: changes to the source `.fprefab` propagate to all instances (overridden properties are preserved)

Prefabs use **stable IDs** to track entities across edits and support nested hierarchies with automatic EntityReference remapping on instantiation.

## Entity References

`EntityReference` is a struct for linking one entity to another. It stores the target entity's GUID and resolves at runtime.

```csharp
public class MyComponent : Component
{
    public EntityReference Target { get; set; }

    public override void Update(float dt)
    {
        var entity = Target.Resolve(Scene);
        if (entity != null)
        {
            // use the referenced entity
        }
    }
}
```

- Serialized as a GUID string in scene/prefab JSON
- Survives duplication (remapped for co-duplicated entities)
- Survives prefab instantiation (remapped via stable ID mapping)
- Drag an entity from the hierarchy onto an EntityReference field in the inspector to assign it

## Scripting

### Custom Components

Create gameplay by writing classes that extend `Component`:

```csharp
using FrinkyEngine.Core.ECS;

public class RotatorComponent : Component
{
    public float Speed { get; set; } = 90f;

    public override void Update(float dt)
    {
        Transform.LocalRotation *= Quaternion.CreateFromAxisAngle(
            Vector3.UnitY, float.DegreesToRadians(Speed * dt));
    }
}
```

### Game Assemblies

- Game code compiles to a separate DLL referenced via `.fproject` `gameAssembly`
- Build scripts from the editor with `Scripts -> Build Scripts`
- The engine discovers custom components via `ComponentTypeResolver` reflection
- **Hot-reload**: assemblies load through collectible `AssemblyLoadContext` — rebuild and reload without restarting the editor

### Auto-Serialization

Public read/write properties on components are automatically serialized to JSON. Supported types include `float`, `int`, `bool`, `string`, `Vector2`, `Vector3`, `Quaternion`, `Color`, enums, and `EntityReference`.

## Runtime Modes

### Dev Mode (`.fproject`)

Use when iterating from source or build outputs.

```bash
dotnet run --project src/FrinkyEngine.Runtime -- path/to/MyGame.fproject
```

- Loads `.fproject` and resolves `assetsPath`, `defaultScene`, and `gameAssembly`
- Applies runtime settings from `project_settings.json`
- Runs the scene loop from the scene's main camera

### Exported Mode (`.fasset`)

Use for packaged game distribution.

- Runtime looks for a `.fasset` file next to the executable
- Extracts the archive to a temp folder, loads `manifest.json`, assets, shaders, and game assembly
- Runs the startup scene from the archive
- Cleans up on exit

If no `.fproject` argument and no `.fasset` is found, the runtime prints usage help.

## Export and Packaging

### Editor Export (`File -> Export Game...`)

1. Builds game scripts in `Release`
2. Publishes runtime (`win-x64`, `FrinkyExport=true` for no console window)
3. Packs assets, shaders, manifest, and game assembly into `.fasset`
4. Outputs `<OutputName>.exe` + `<OutputName>.fasset`

`OutputName` and `BuildVersion` come from `project_settings.json` build settings.

### Script Packaging (`package-game.bat`)

```powershell
.\package-game.bat path\to\Game.fproject [outDir] [rid]
```

Builds the game assembly, publishes the runtime (framework-dependent), copies project files, and creates a `Play.bat` launcher. Default RID: `win-x64`.

### Local Release (`release-local.bat`)

```powershell
.\release-local.bat v0.1.0
```

Validates the version tag, builds the solution with warnings-as-errors, publishes editor and runtime, packs the template NuGet, and creates zip artifacts in `artifacts/release/`. Supports `--patch` for auto-incrementing the version from git tags.

## Project and Settings Files

### `.fproject`

```json
{
  "projectName": "MyGame",
  "defaultScene": "Scenes/MainScene.fscene",
  "assetsPath": "Assets",
  "gameAssembly": "bin/Debug/net8.0/MyGame.dll",
  "gameProject": "MyGame.csproj"
}
```

`defaultScene` is relative to `assetsPath`. The editor script build uses `Debug`, so `gameAssembly` points to `bin/Debug`.

### `project_settings.json`

```json
{
  "project": {
    "version": "0.1.0",
    "author": "",
    "company": "",
    "description": ""
  },
  "runtime": {
    "targetFps": 120,
    "vSync": true,
    "windowTitle": "MyGame",
    "windowWidth": 1280,
    "windowHeight": 720,
    "resizable": true,
    "fullscreen": false,
    "startMaximized": false,
    "startupSceneOverride": "",
    "forwardPlusTileSize": 16,
    "forwardPlusMaxLights": 256,
    "forwardPlusMaxLightsPerTile": 64
  },
  "build": {
    "outputName": "MyGame",
    "buildVersion": "0.1.0"
  }
}
```

### `.frinky/editor_settings.json` and `.frinky/keybinds.json`

Per-project editor preferences and keybinds. `keybinds.json` is auto-created with defaults when a project opens.

## Repository Layout

```text
FrinkyEngine/
  src/
    FrinkyEngine.Core/        # Engine library: ECS, rendering, physics, serialization
    FrinkyEngine.Editor/      # Dear ImGui editor application
    FrinkyEngine.Runtime/     # Standalone game player
  templates/
    FrinkyEngine.Templates/   # dotnet new template pack
  Shaders/                    # GLSL shaders (copied to output)
  EditorAssets/               # Editor fonts and icons
  ExampleGames/
    PrefabTestingGround/      # Example project with prefab usage
  artifacts/                  # Publish and build outputs
  *.bat                       # Build, launch, publish, and packaging scripts
  FrinkyEngine.sln
  Directory.Build.props       # Global build settings (.NET 8, C# 12, unsafe)
```

## Example Games

### PrefabTestingGround

A test project demonstrating the prefab system, entity references, and post-processing. Located at `ExampleGames/PrefabTestingGround/`.

Open it in the editor:

```bash
dotnet run --project src/FrinkyEngine.Editor -- ExampleGames/PrefabTestingGround/PrefabTestingGround.fproject
```

## Troubleshooting

- **Runtime says "Failed to load scene"**
  - Check `.fproject` `assetsPath` and `defaultScene`
  - Confirm the scene file exists under `<project>/Assets/...`

- **Custom component does not appear in Add Component**
  - Build scripts first (`Scripts -> Build Scripts`)
  - Confirm `.fproject` `gameAssembly` path matches the built DLL

- **Runtime does not start in exported mode**
  - Confirm `.fasset` is in the same folder as the executable

- **Missing shader or black render output**
  - Use publish/build outputs that include the `Shaders/` folder content

## Dependencies

| Project | Package | Version |
|---|---|---|
| Core | `BepuPhysics` | 2.4.0 |
| Core | `Raylib-cs` | 7.0.2 |
| Editor | `NativeFileDialogSharp` | 0.5.0 |
| Editor | `rlImGui-cs` | 3.2.0 |
| Runtime | `Raylib-cs` | 7.0.2 |

Global: .NET 8, C# 12, nullable enabled, `AllowUnsafeBlocks=true`.

## License

Unlicensed. All rights reserved.
