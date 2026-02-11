# Troubleshooting

## Runtime says "Failed to load scene"

- Check `.fproject` `assetsPath` and `defaultScene` fields
- Confirm the scene file exists under `<project>/Assets/...`
- Verify `defaultScene` is relative to `assetsPath`, not the project root

## Custom component does not appear in Add Component

- Build scripts first: `Scripts -> Build Scripts` (`Ctrl+B`)
- Confirm `.fproject` `gameAssembly` path matches the built DLL location
- Ensure your component class is `public` and extends `Component`
- Check the Console panel for assembly load errors

## Component was skipped while opening a scene or prefab

- Symptom: the Console shows a warning like `Skipped component '...' ... data preserved`
- Cause: the component type could not be instantiated (commonly missing a public parameterless constructor)
- Fix:
  1. Add a public parameterless constructor to the component
  2. Move initialization logic into `Awake`/`Start` instead of constructor parameters
  3. Rebuild scripts (`Ctrl+B`) and reopen the scene

## Runtime does not start in exported mode

- Confirm the `.fasset` file is in the same folder as the executable
- The `.fasset` filename must match the `OutputName` from `project_settings.json`

## Missing shader or black render output

- Use publish/build outputs that include the `Shaders/` folder content
- If running from source, ensure the shader copy step in the build completed
- Check the Console panel for shader compilation errors

## VS Code C# extension shows errors after launching from editor

- The editor's .NET runtime sets `DOTNET_*` and `MSBUILD*` environment variables that can interfere with VS Code's C# Dev Kit
- The editor strips these when launching VS Code, but if you launched VS Code separately, restart it from the editor (`Ctrl+Shift+V`)

## Physics objects fall through the floor

- Ensure the floor entity has a collider component (e.g. `BoxColliderComponent`)
- Ensure the floor entity has a `RigidbodyComponent` with `MotionType = Static`
- Check that collider sizes are appropriate for the mesh

## Character controller does not move

- Verify all three required components are on the entity:
  1. `RigidbodyComponent` with `MotionType = Dynamic`
  2. `CapsuleColliderComponent` (must be the first enabled collider)
  3. `CharacterControllerComponent`
- If using `SimplePlayerInputComponent`, ensure it is on the same entity
- Check that the viewport/game window has focus

## Audio does not play

- Verify the sound file path is correct and the file exists in the assets folder
- Check the Console panel for audio loading warnings
- Confirm the relevant mixer bus volume is not zero (check `project_settings.json` audio settings)
- For 3D audio, ensure an `AudioListenerComponent` exists in the scene (or a main camera as fallback)

## Hot-reload does not pick up script changes

- Use `Scripts -> Build Scripts` (`Ctrl+B`) to trigger a rebuild
- Check the Console panel for build errors
- Ensure `.fproject` `gameProject` points to the correct `.csproj`
