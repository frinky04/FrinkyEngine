# FrinkyEngine

FrinkyEngine is a C#/.NET 8 3D engine with a Dear ImGui editor, a standalone runtime, JSON scene files, and a `dotnet new` template for game projects.

## What You Get

- ECS-style component workflow for gameplay code
- Editor with viewport, hierarchy, inspector, console, assets, and play mode
- Runtime that can run from either `.fproject` (dev mode) or `.fasset` (export mode)
- Scene serialization to `.fscene` JSON
- Project template and scripts for build, publish, packaging, and local release

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows for `.bat` helper scripts
- Optional: use direct `dotnet` commands if you do not want to use scripts

## Quick Start (5 Steps)

1. Build the solution.

```powershell
.\build.bat
```

```bash
dotnet build FrinkyEngine.sln -c Release
```

2. Launch the editor.

```powershell
.\launch-editor.bat
```

```bash
dotnet run --project src/FrinkyEngine.Editor
```

3. Open a project, for example:

```powershell
.\launch-editor.bat Games\Testing\Testing.fproject
```

```bash
dotnet run --project src/FrinkyEngine.Editor -- Games/Testing/Testing.fproject
```

4. Build game scripts from the editor (`Scripts -> Build Scripts`) or:

```bash
dotnet build path/to/MyGame.csproj --configuration Debug
```

5. Run the runtime with a project file.

```powershell
.\launch-runtime.bat Games\Testing\Testing.fproject
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

Create project:

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
    Scripts/RotatorComponent.cs
```

Important:
- The template `MyGame.csproj` includes a placeholder comment for `FrinkyEngine.Core`.
- Add a `ProjectReference` to your local `src/FrinkyEngine.Core/FrinkyEngine.Core.csproj` if needed.

## Runtime Modes

### Dev Mode (`.fproject`)

Use when iterating in-editor and running from source/build outputs.

```bash
dotnet run --project src/FrinkyEngine.Runtime -- path/to/MyGame.fproject
```

Behavior:
- Loads `.fproject`
- Resolves `assetsPath` and `defaultScene`
- Loads `gameAssembly` if configured
- Runs the scene loop

### Exported Mode (`.fasset`)

Use for packaged game distribution.

Behavior:
- Runtime checks for a `.fasset` file in the executable directory
- Extracts archive to a temp folder
- Loads `manifest.json`, assets, shaders, and optional game assembly
- Runs the startup scene from the archive

If no `.fproject` argument and no `.fasset` is found next to the executable, runtime prints usage help.

## Export and Packaging Workflows

### Editor Export (`File -> Export Game...`)

The editor export pipeline:
1. Builds game scripts in `Release`
2. Publishes runtime (`win-x64`, self-contained, single-file settings)
3. Packs assets, shaders, manifest, and game assembly dependencies into `.fasset`
4. Writes output as:
   - `<OutputName>.exe`
   - `<OutputName>.fasset`

`OutputName` and `BuildVersion` come from `project_settings.json` build settings.

### Script Packaging (`package-game.bat`)

```powershell
.\package-game.bat path\to\Game.fproject
```

What it does:
1. Builds game project in `Release`
2. Publishes runtime framework-dependent (`--self-contained false`)
3. Copies `.fproject`, game DLL, and game content folders
4. Creates `Play.bat` launcher

### Local Release (`release-local.bat`)

```powershell
.\release-local.bat v0.1.0
```

What it does:
1. Validates version tag format (`vMAJOR.MINOR.PATCH`)
2. Restores and builds solution with warnings as errors
3. Publishes editor and runtime (`win-x64`)
4. Packs template NuGet package
5. Creates editor/runtime zip artifacts in `artifacts/release`

## Project and Settings Files

### `.fproject`

Defines project identity and where runtime/editor load content from.

```json
{
  "projectName": "MyGame",
  "defaultScene": "Scenes/MainScene.fscene",
  "assetsPath": "Assets",
  "gameAssembly": "bin/Debug/net8.0/MyGame.dll",
  "gameProject": "MyGame.csproj"
}
```

Notes:
- `defaultScene` is relative to `assetsPath`.
- Editor script build uses `Debug` by default, so template `gameAssembly` points to `bin/Debug`.

### `project_settings.json`

Stored at the project root. Controls metadata, runtime launch settings, and build output naming.

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

Per-project editor preferences and shortcuts.

`editor_settings.json` (minimal example):

```json
{
  "targetFps": 120,
  "vSync": false
}
```

`keybinds.json` is auto-created with defaults when a project opens.

## Editor Overview

Main panels:
- **Viewport**: scene rendering + transform gizmos
- **Hierarchy**: entity tree and scene filtering
- **Inspector**: component editing for selected entity
- **Assets**: asset browser with file watcher updates
- **Console**: log stream
- **Performance**: optional performance panel

Runtime preview modes:
- **Play** (`Ctrl+P`): runs gameplay from the scene's main camera and locks scene editing.
- **Simulate** (`Ctrl+Shift+P`): runs gameplay while keeping editor camera/tools available (respects `Game View` toggle for overlays).
- Editor snapshots the current scene before entering either runtime mode.
- Runtime and scene edits made during Play/Simulate are restored on Stop.

## Architecture Snapshot

- `Entity` has a list of `Component` instances
- Each entity always has a `TransformComponent`
- Component lifecycle includes `Awake`, `Start`, `Update`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`
- Scenes (`.fscene`) are JSON and use `$type` for component polymorphism
- Rendering uses Raylib and bundled shader content in `Shaders/`
- Game assemblies load through `AssemblyLoadContext` to discover custom components

## Repository Layout

```text
FrinkyEngine/
  src/
    FrinkyEngine.Core/      # Engine core, ECS, scene, serialization, assets, rendering
    FrinkyEngine.Editor/    # Editor app and panels
    FrinkyEngine.Runtime/   # Standalone runtime app
  templates/
    FrinkyEngine.Templates/ # dotnet new template pack
  Shaders/                  # Runtime/editor shader sources copied to output
  EditorAssets/             # Editor fonts and icon assets
  Games/                    # Sample or local game projects
  artifacts/                # Publish/build outputs
```

## Troubleshooting

- Runtime says "Failed to load scene"
  - Check `.fproject` `assetsPath` and `defaultScene`.
  - Confirm the scene file exists under `<project>/Assets/...`.

- Custom script component does not appear in Add Component
  - Build scripts first (`Scripts -> Build Scripts`).
  - Confirm `.fproject` `gameAssembly` path matches built DLL.

- Runtime does not start in exported mode
  - Confirm `.fasset` is in the same folder as the executable.

- Missing shader or black render output
  - Use publish/build outputs that include the `Shaders/` folder content.

## Dependencies

| Project | Package references |
|---|---|
| `FrinkyEngine.Core` | `Raylib-cs` `7.0.2` |
| `FrinkyEngine.Editor` | `NativeFileDialogSharp` `0.5.0`, `rlImGui-cs` `3.2.0` |
| `FrinkyEngine.Runtime` | `Raylib-cs` `7.0.2` |

## License

Unlicensed. All rights reserved.
