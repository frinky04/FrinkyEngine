# Agent Instructions

## Build & Run

| Command | Description |
|---------|-------------|
| `dotnet build FrinkyEngine.sln` | Build all projects |
| `dotnet run --project src/FrinkyEngine.Editor` | Run editor |
| `dotnet run --project src/FrinkyEngine.Editor -- path/to/Game.fproject` | Open specific project |
| `dotnet run --project src/FrinkyEngine.Runtime -- path/to/Game.fproject` | Run game |

No tests exist.

## Commit Attribution

AI commits MUST include:
```
Co-Authored-By: (the agent model's name and attribution byline)
```

## Project Layout

- `src/FrinkyEngine.Core/` — shared engine library (entities, components, scene, serialization, rendering)
- `src/FrinkyEngine.Editor/` — ImGui desktop editor
- `src/FrinkyEngine.Runtime/` — standalone game player
- `templates/FrinkyEngine.Templates/` — `dotnet new` template pack

## Key Conventions

- .NET 8, C# 12, nullable enabled, `AllowUnsafeBlocks=true`
- File-scoped namespaces
- Components in `Components/`, panels in `Panels/`, serialization in `Serialization/`
- Scenes: `.fscene`, Projects: `.fproject` (both JSON)
- Public Core API types/methods get `<summary>` XML docs; Editor/Runtime internals do not
- Update `README.md` when adding features or changing behavior

## API Pitfalls

- **Raylib-cs `Shader.Locs`** is a pointer — use `unsafe`, cache locations in `int` fields
- **Hexa.NET.ImGui** — namespace `Hexa.NET.ImGui`, use `ImGuiP` for docking APIs, many overloads need `(string?)null` casts, images use `ImTextureRef`
- **Hexa.NET.ImGui.Widgets** — `ComboEnumHelper<T>.Combo()` / `ComboEnumHelper.Combo()` for enum combos, `MessageBoxes.Show()` / `MessageBoxes.Draw()` for modal dialogs
- **RlImGui** (custom `RlImGui.cs`) — call `Rlgl.DrawRenderBatchActive()` after each draw command in `End()`
- **Raylib cursor** — `DisableCursor()`/`EnableCursor()` re-center mouse; only call on state transitions
