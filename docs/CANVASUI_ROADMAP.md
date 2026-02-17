# CanvasUI — Full Replacement Plan

## Context

FrinkyEngine currently uses an immediate-mode ImGui wrapper (`FrinkyEngine.Core.UI`) for game UI. This is ~3,150 lines in Core + ~9,950 lines in Editor panels. The goal is to replace the **game-facing** UI with a new retained-mode system called **CanvasUI**, heavily inspired by S&box's web-like Panel/CSS/flexbox architecture.

This is a clean break — no migration path, no backwards compatibility. The editor keeps ImGui for now. The old `FrinkyEngine.Core.UI` namespace gets renamed/scoped to editor-only usage later.

---

## Phase 1: Foundation ✅ (Implemented)

Core panel tree, flexbox layout via Yoga, basic rendering, and input.

### 1.1 Namespace + project structure

Created `src/FrinkyEngine.Core/CanvasUI/` with:

```
CanvasUI/
  CanvasUI.cs              — Static entry point (Initialize/Update/Shutdown)
  Panel.cs                 — Base retained-mode panel class
  RootPanel.cs             — Tree root, owns layout/render/input subsystems
  PseudoClassFlags.cs      — [Flags] enum: Hover, Active, Focus, Disabled, Intro, Outro
  Box.cs                   — Computed layout rect struct (X, Y, Width, Height)
  Panels/
    Label.cs               — Text display
    Button.cs              — Clickable, raises OnClick
  Styles/
    StyleSheet.cs           — Per-panel inline style property bag
    ComputedStyle.cs        — Flat resolved style struct
    StyleEnums.cs           — FlexDirection, AlignItems, JustifyContent, Overflow, Display, PositionMode
    Length.cs               — Pixels / Percent / Auto
    Edges.cs                — Top/Right/Bottom/Left shorthand
    StyleResolver.cs        — Resolves inline styles into ComputedStyle
  Layout/
    YogaLayoutEngine.cs     — Syncs styles → Yoga nodes, runs CalculateLayout, reads results
  Rendering/
    CanvasRenderer.cs       — Depth-first tree traversal, draws via Raylib
    DrawCommands.cs         — Filled rect, rounded rect, text, textured quad
    FontManager.cs          — TTF font loading/caching via Raylib.LoadFontEx
    ScissorStack.cs         — Nested clipping rect stack (Rlgl.Scissor)
  Input/
    InputManager.cs         — Hit testing, hover/focus tracking, event dispatch
  Events/
    MouseEvent.cs           — ScreenPos, LocalPos, Button, Target, Handled
    FocusEvent.cs
    KeyboardEvent.cs
```

### 1.2 Yoga dependency

Added `IceReaper.YogaSharp` (1.18.0.3) NuGet package to `FrinkyEngine.Core.csproj`. Wraps Facebook's Yoga C library (used by React Native).

### 1.3 Panel base class

Every UI element is a `Panel`. Key API:
- `Id`, `Classes` (list of string class names)
- `Parent`, `Children` tree links
- `Style` (inline StyleSheet), `ComputedStyle` (resolved)
- `PseudoClasses` flags (set by input manager)
- `YogaNode` handle (internal)
- Events: `OnClick`, `OnMouseOver`, `OnMouseOut`, `OnMouseDown`, `OnMouseUp`, `OnFocus`, `OnBlur`
- Lifecycle: `OnCreated()`, `OnDeleted()`, `Tick()` (per-frame)
- Child management: `AddChild<T>()`, `RemoveChild()`, `DeleteChildren()`

### 1.4 Rendering backend

Uses Raylib drawing API (`DrawRectangleRounded`, `DrawTextEx`, etc.) with Rlgl scissor clipping. Render pipeline:
1. Disable depth test
2. Depth-first traversal of panel tree
3. Per panel: draw background rect → draw border → draw content (text/image) → recurse children
4. Scissor clipping for `Overflow.Hidden`

### 1.5 Engine integration

In `src/FrinkyEngine.Runtime/Program.cs`, within `RunGameLoop`:
- After `UI.Initialize()`: `CanvasUI.Initialize()`
- In the UI profile scope: `CanvasUI.Update(dt, screenW, screenH)`
- At shutdown: `CanvasUI.Shutdown()`

### 1.6 Usage example

```csharp
var hud = CanvasUI.RootPanel.AddChild<Panel>(p => {
    p.Style.FlexDirection = FlexDirection.Row;
    p.Style.Padding = new Edges(12);
});
hud.AddChild<Label>(l => { l.Text = "Health: 100"; });
hud.AddChild<Button>(b => { b.Text = "Menu"; b.OnClick += _ => OpenMenu(); });
```

---

## Phase 2: CSS Styling + More Widgets

### 2.1 CSS parser

Lightweight hand-written tokenizer + parser. Supports:
- Selectors: `.classname`, `#id`, `Panel`, `Label.classname`, `Panel:hover`, `Panel:active`
- Common properties: `background-color`, `color`, `font-size`, `padding`, `margin`, `border-radius`, `border`, `width`, `height`, `flex-direction`, `align-items`, `justify-content`, `gap`, `opacity`, `display`, `position`, `overflow`
- Specificity-based cascading (inline > id > class > type)

Files: `Styles/CssParser.cs`, `Styles/CssTokenizer.cs`, `Styles/Selector.cs`, `Styles/StyleRule.cs`, `Styles/StyleResolver.cs` (extended)

### 2.2 Style resolver

Each frame (or when dirty): collect matching rules per panel → sort by specificity → merge into `ComputedStyle` → inline styles override.

### 2.3 More built-in panels

| Panel | Purpose |
|-------|---------|
| `TextEntry` | Single-line text input with cursor, selection, keyboard handling |
| `Checkbox` | Toggle with `:checked` pseudo-class |
| `Slider` | Range input (horizontal drag) |
| `ProgressBar` | Visual fill bar |
| `ScrollPanel` | Scrollable container, mouse wheel, scroll bar |
| `Image` | Displays a Texture2D/RenderTexture |

### 2.4 Border rendering

Rounded corners via tessellated triangle fans at each corner.

---

## Phase 3: Markup + Data Binding

### 3.1 XML markup format (`.canvas` files)

```xml
<Panel class="hud-root">
    <Label text="{Health}" class="health-text" />
    <Button text="Menu" onclick="OpenMenu" />
</Panel>
```

Parsed at runtime into Panel trees. **Not** Razor — avoids heavy `Microsoft.AspNetCore.Razor.Language` dependency.

### 3.2 Data binding

`{PropertyName}` syntax bound to a context object implementing `INotifyPropertyChanged`.

### 3.3 Hot reload

File watcher on `.canvas` and `.css` files — rebuilds panel tree and re-applies styles during development.

### 3.4 Asset integration

Markup/CSS loaded through `AssetManager` for both dev and exported builds.

---

## Phase 4: Polish + Advanced Features

- **SDF font rendering** — Raylib's built-in SDF support for resolution-independent text
- **Transitions** — CSS-like `transition: opacity 0.3s ease`, `:intro`/`:outro` pseudo-classes for enter/exit animations
- **Transforms** — `translate`, `rotate`, `scale` applied as matrix before render
- **Box shadows** — Blurred rectangle behind panel
- **Gamepad navigation** — D-pad focus traversal
- **Dirty flags** — Skip layout/style recalc when nothing changed
- **Draw batching** — Minimize Rlgl state changes

---

## Phase 5: Migration

1. Port `EngineOverlays` (stats + dev console) to CanvasUI panels
2. Port `DebugDraw.PrintString` to CanvasUI
3. Move old `FrinkyEngine.Core.UI` to editor-only (or delete entirely)
4. Remove `Hexa.NET.ImGui` dependency from `FrinkyEngine.Core.csproj`

---

## Key Files Modified (Phase 1)

| File | Change |
|------|--------|
| `src/FrinkyEngine.Core/FrinkyEngine.Core.csproj` | Added IceReaper.YogaSharp NuGet package |
| `src/FrinkyEngine.Runtime/Program.cs` | Wired CanvasUI.Initialize/Update/Shutdown into game loop |
| `src/FrinkyEngine.Core/CanvasUI/**` | All new files (listed above) |

## Verification

1. **Build** — `dotnet build FrinkyEngine.sln` compiles with no errors ✅
2. **Runtime smoke test** — Launch runtime with a test scene; a game component creates a Panel with a Label and Button; panels render on screen with correct flexbox layout
3. **Input test** — Hover highlights button (pseudo-class), click fires OnClick event
4. **Coexistence** — Old ImGui UI (EngineOverlays) still works alongside CanvasUI in the same frame
