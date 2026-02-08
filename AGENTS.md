- Think in first principles, be direct, adapt to context. Skip "great question" fluff. Verifiable facts over platitudes.
- Always cite every source you used
- Banned phrases: emdashes, watery language, "it's not about X, it's about Y", here's the kicker
- Humanize all your output
- Reason at 100% max ultimate power, think step by step
- Self-critique every response: rate 1-10, fix weaknesses, iterate. User sees only final version.
- Useful over polite. When wrong, say so and show better.

# Repository Guidelines

## Project Structure & Module Organization
The solution is `FrinkyEngine.sln` with three main projects in `src/`:
- `FrinkyEngine.Core/` shared engine library (ECS-style components, scene/serialization, rendering).
- `FrinkyEngine.Editor/` ImGui-based editor app.
- `FrinkyEngine.Runtime/` standalone game player.

Supporting directories:
- `templates/FrinkyEngine.Templates/` `dotnet new` template pack.
- `Shaders/` engine shaders copied to output.
- `EditorAssets/` editor fonts and assets.
- `Games/` sample game projects.

Scene files use `.fscene` and project files use `.fproject` (JSON).

## Build, Test, and Development Commands
- `dotnet build FrinkyEngine.sln` — build all projects.
- `dotnet run --project src/FrinkyEngine.Editor` — run the editor.
- `dotnet run --project src/FrinkyEngine.Editor -- path/to/Game.fproject` — open a specific project.
- `dotnet run --project src/FrinkyEngine.Runtime -- path/to/Game.fproject` — run a game project.
- `dotnet new install ./templates/FrinkyEngine.Templates` — install templates.
- `dotnet new frinky-game -n MyGame` — scaffold a game project.

## Coding Style & Naming Conventions
- C# 12, nullable enabled, implicit usings, unsafe allowed (see `Directory.Build.props`).
- Indentation is 4 spaces; use file-scoped namespaces as in existing files.
- Use PascalCase for types/methods/properties; camelCase for locals/fields.
- Place components in `Components/`, editor panels in `Panels/`, and serialization helpers in `Serialization/`.
- No formatter/linter is configured; match the existing style and keep changes minimal and consistent.
- Public methods and types in Core should have `<summary>` XML doc comments. These are the primary API documentation for users building games with the standalone editor. Editor/Runtime internals do not need XML docs.

## Testing Guidelines
There are currently no automated tests or test projects. Validate changes by running:
- Editor flows in `FrinkyEngine.Editor` (viewport, hierarchy, inspector).
- Runtime loading of a `.fproject`.
If you add tests, document the framework and how to run them in your PR.

## Commit & Pull Request Guidelines
Commit history uses short, imperative summaries (e.g., `Add gizmo system`, `Fix dark meshes`). Follow that style and keep messages focused.

For PRs, include:
- What changed and why.
- How you tested (commands and manual steps).
- Screenshots or short clips for editor UI changes.

## Configuration & Assets
Keep shader changes in `Shaders/` and ensure they're copied to output. Editor fonts/assets should live under `EditorAssets/` so content globbing picks them up.

## Documentation
When adding new features, components, systems, or changing existing behavior, update `README.md` to reflect those changes. The README is the primary user-facing documentation for the engine.
