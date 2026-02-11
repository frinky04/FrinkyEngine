# Editor Guide

The FrinkyEngine editor is an ImGui-based desktop application for building and testing scenes.

## Panels

| Panel | Description |
|-------|-------------|
| **Viewport** | 3D scene rendering with transform gizmos (translate, rotate, scale) |
| **Hierarchy** | Entity tree with drag-and-drop reordering and parenting |
| **Inspector** | Component editing with attribute-driven reflection (engine and script components use the same inspector pipeline) |
| **Assets** | File browser with drag-and-drop for models, prefabs, and scenes |
| **Console** | Log stream viewer |
| **Performance** | Frame time and rendering statistics, including an `Ignore Editor` toggle for runtime-estimate CPU views |

## Transform Gizmos

- **1** — Translate mode
- **2** — Rotate mode
- **3** — Scale mode
- **X** — Toggle world/local space

## Entity Management

- **Create**: right-click in the hierarchy or use the `Entity` menu
- **Rename**: select an entity and press `F2`
- **Delete**: select an entity and press `Delete`
- **Duplicate**: `Ctrl+D`
- **Reparent**: drag entities in the hierarchy to reorder or nest them
- **Select all**: `Ctrl+A`
- **Deselect**: `Escape`

## Play and Simulate Modes

- **Play** (`F5`) — runs gameplay from the scene's main camera, locks scene editing
- **Simulate** (`Alt+F5`) — runs gameplay while keeping the editor camera and tools available
- The editor snapshots the scene before entering either mode and restores it on stop
- **Shift+F1** — toggle cursor lock during play mode

## Asset Browser

- Browse project files with filtering and search
- Drag-and-drop models (`.obj`, `.gltf`, `.glb`), prefabs (`.fprefab`), and scenes (`.fscene`) into the viewport or hierarchy
- Tags and type filters for quick asset lookup

## Building Scripts

Build game assemblies from the editor with `Scripts -> Build Scripts` (`Ctrl+B`). The editor hot-reloads the assembly without restarting.

## Physics Hitbox Preview

Press `F8` to toggle a wireframe overlay showing collider shapes in the viewport.

## Stats Overlay and Developer Console

- **F3** — cycle stats overlay modes: None → FPS + MS → Advanced Stats → Most Verbose Stats
- **\`** (Grave) — toggle the developer console
  - `help` lists registered commands and cvars
  - `Tab` cycles suggestions, `Enter` accepts and executes
  - `Up/Down` navigates command history

## Keyboard Shortcuts

### File

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New Scene |
| Ctrl+O | Open Scene |
| Ctrl+S | Save Scene |
| Ctrl+Shift+S | Save Scene As |
| Ctrl+Shift+N | New Project |

### Edit

| Shortcut | Action |
|----------|--------|
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Delete | Delete Entity |
| Ctrl+D | Duplicate Entity |
| F2 | Rename Entity |
| Ctrl+A | Select All |
| Escape | Deselect |

### Build and Runtime

| Shortcut | Action |
|----------|--------|
| Ctrl+B | Build Scripts |
| F5 | Play / Stop |
| Alt+F5 | Simulate / Stop |
| Shift+F1 | Toggle Play Mode Cursor Lock |

### Gizmos

| Shortcut | Action |
|----------|--------|
| 1 | Translate Mode |
| 2 | Rotate Mode |
| 3 | Scale Mode |
| X | Toggle World/Local Space |

### View

| Shortcut | Action |
|----------|--------|
| G | Toggle Game View |
| F | Frame Selected (focus camera on selection) |
| F3 | Cycle Stats Overlay |
| F8 | Toggle Physics Hitbox Preview |

### Navigation

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Focus Hierarchy Search |
| Right Arrow | Expand Selection (Hierarchy) |
| Left Arrow | Collapse Selection (Hierarchy) |

### Project and Tools

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+O | Open Assets Folder |
| Ctrl+Shift+V | Open Project in VS Code |
| Ctrl+Shift+E | Export Game |

### Prefabs

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+M | Create Prefab from Selection |
| Ctrl+Alt+P | Apply Prefab |
| Ctrl+Alt+R | Revert Prefab |
| Ctrl+Shift+U | Make Unique Prefab |
| Ctrl+Alt+K | Unpack Prefab |

### Camera Controls (Viewport)

| Input | Action |
|-------|--------|
| Right Mouse Button (hold) | Free Look |
| W/A/S/D (while right-mouse held) | Move Camera |
| Left Shift (while right-mouse held) | 2.5x Camera Speed |
| Mouse Scroll | Zoom (2x speed) |

All keybinds are customizable per-project in `.frinky/keybinds.json`.
