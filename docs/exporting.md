# Exporting & Packaging

FrinkyEngine supports two runtime modes and provides tools for packaging games for distribution.

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

## Editor Export

Use `File -> Export Game...` (`Ctrl+Shift+E`) from the editor:

1. Builds game scripts in `Release`
2. Publishes runtime (`win-x64`, `FrinkyExport=true` for no console window)
3. Packs assets, shaders, manifest, and game assembly into `.fasset`
4. Outputs `<OutputName>.exe` + `<OutputName>.fasset`

`OutputName` and `BuildVersion` come from `project_settings.json` build settings (see [Project Settings](project-settings.md)).

## Script Packaging (`package-game.bat`)

```powershell
.\package-game.bat path\to\Game.fproject [outDir] [rid]
```

Builds the game assembly, publishes the runtime (framework-dependent), copies project files, and creates a `Play.bat` launcher. Default RID: `win-x64`.

## Local Release (`release-local.bat`)

```powershell
.\release-local.bat v0.1.0
```

Validates the version tag, builds the solution with warnings-as-errors, publishes editor and runtime, packs the template NuGet, and creates zip artifacts in `artifacts/release/`. Supports `--patch` for auto-incrementing the version from git tags.

## Runtime Overlay Controls

Available in both standalone runtime and editor Play/Simulate mode:

- **F3** — cycle stats overlay: None → FPS + MS → Advanced Stats → Most Verbose Stats
- **\`** (Grave) — toggle developer console
  - `help` — list registered commands and cvars
  - `Tab` — cycle suggestions, `Enter` — execute
  - `Up/Down` — navigate command history

### Console CVars

| CVar | Values | Description |
|------|--------|-------------|
| `r_postprocess` | `0` / `1` | Toggle post-processing |
