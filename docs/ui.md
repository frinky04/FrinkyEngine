# Game UI

FrinkyEngine provides two UI systems for game scripts:

- **CanvasUI** (`FrinkyEngine.Core.CanvasUI`) — retained-mode panels with flexbox layout. Recommended for new game UI.
- **Immediate-mode UI** (`FrinkyEngine.Core.UI`) — the existing ImGui wrapper. Still functional; will eventually be scoped to editor-only usage.

Both systems can be used in the same project.

---

## CanvasUI (Retained-Mode)

CanvasUI lets you build UI by creating panels, setting styles, and responding to events. Layout follows CSS flexbox rules — if you've used CSS, the properties will feel familiar.

### Getting Started

Create panels in `Start()` and update them in `Update()`:

```csharp
using FrinkyEngine.Core.CanvasUI;
using FrinkyEngine.Core.CanvasUI.Panels;
using FrinkyEngine.Core.CanvasUI.Styles;
using FrinkyEngine.Core.ECS;

public class HudComponent : Component
{
    private Label _healthLabel;

    public override void Start()
    {
        var hud = CanvasUI.RootPanel.AddChild<Panel>(p =>
        {
            p.Style.FlexDirection = FlexDirection.Row;
            p.Style.Padding = new Edges(12);
            p.Style.Gap = 8;
        });

        _healthLabel = hud.AddChild<Label>(l =>
        {
            l.Text = "Health: 100";
            l.Style.Color = new Raylib_cs.Color(0, 255, 0, 255);
            l.Style.FontSize = 20f;
        });

        hud.AddChild<Button>(b =>
        {
            b.Text = "Menu";
            b.OnClick += _ => OpenMenu();
        });
    }

    public override void Update(float dt)
    {
        _healthLabel.Text = $"Health: {GetHealth()}";
    }

    private int GetHealth() => 100;
    private void OpenMenu() { }
}
```

### Panel

`Panel` is the base class for all CanvasUI elements. Use `AddChild<T>()` to build a tree of panels from `CanvasUI.RootPanel`.

#### Child Management

| Method | Description |
|--------|-------------|
| `AddChild<T>(configure?)` | Create and add a typed child panel with optional initializer |
| `AddChild(panel)` | Add an existing panel (re-parents if it already has a parent) |
| `RemoveChild(panel)` | Remove a child from this panel |
| `Delete()` | Remove this panel from its parent and delete all children |
| `DeleteChildren()` | Delete all children recursively |

#### Events

| Event | Description |
|-------|-------------|
| `OnClick` | Mouse click (down + up on same panel) |
| `OnMouseOver` | Mouse entered panel bounds |
| `OnMouseOut` | Mouse left panel bounds |
| `OnMouseDown` | Mouse button pressed on panel |
| `OnMouseUp` | Mouse button released on panel |
| `OnFocus` | Panel received keyboard focus |
| `OnBlur` | Panel lost keyboard focus |

#### Classes

Add string class names to a panel with `AddClass()`, `RemoveClass()`, `ToggleClass()`, and check with `HasClass()`. Classes are used for selector matching when CSS styling is added in a future update.

#### Custom Panels

Subclass `Panel` to create reusable UI elements. Override these methods:

| Method | Description |
|--------|-------------|
| `OnCreated()` | Called after the panel is added to the tree — set up children and defaults here |
| `OnDeleted()` | Called when the panel is removed — clean up subscriptions here |
| `Tick(dt)` | Called every frame — use for animations or per-frame updates |

Set `AcceptsFocus = true` in `OnCreated()` if your panel needs keyboard focus.

### Built-in Panels

| Panel | Description |
|-------|-------------|
| `Label` | Displays text. Set the `Text` property. |
| `Button` | Clickable panel with centered text. Set `Text` and subscribe to `OnClick`. Comes with default styling (dark background, rounded corners, padding). |

### Styling

Set properties on `panel.Style` to control layout and appearance. All style properties are optional — unset properties use sensible defaults (e.g., column direction, stretch alignment, white text, 16px font).

#### Layout Properties

| Property | Default | Description |
|----------|---------|-------------|
| `FlexDirection` | `Column` | Main axis direction (`Row`, `Column`, `RowReverse`, `ColumnReverse`) |
| `JustifyContent` | `FlexStart` | Main axis alignment |
| `AlignItems` | `Stretch` | Cross axis alignment |
| `AlignSelf` | `Auto` | Override parent's `AlignItems` for this panel |
| `Display` | `Flex` | `Flex` or `None` (hidden) |
| `Position` | `Relative` | `Relative` or `Absolute` |
| `Overflow` | `Visible` | `Visible`, `Hidden`, or `Scroll` |
| `Width` / `Height` | `Auto` | Panel dimensions |
| `MinWidth` / `MinHeight` | `Auto` | Minimum dimensions |
| `MaxWidth` / `MaxHeight` | `Auto` | Maximum dimensions |
| `FlexGrow` | `0` | How much to grow to fill available space |
| `FlexShrink` | `1` | How much to shrink when space is tight |
| `FlexBasis` | `Auto` | Initial size before flex grow/shrink |
| `Gap` | `0` | Space between children (pixels) |
| `Padding` | `0` | Inner spacing |
| `Margin` | `0` | Outer spacing |

#### Visual Properties

| Property | Default | Description |
|----------|---------|-------------|
| `BackgroundColor` | transparent | Panel background |
| `Color` | white | Text / foreground color |
| `BorderColor` | transparent | Border color |
| `BorderWidth` | `0` | Border thickness in pixels |
| `BorderRadius` | `0` | Corner radius in pixels |
| `FontSize` | `16` | Text size in pixels |
| `Opacity` | `1` | Overall opacity (0-1) |

#### Length Values

Dimensions accept `Length` values:

```csharp
p.Style.Width = 200;                    // pixels (implicit from float/int)
p.Style.Width = Length.Px(200);          // pixels (explicit)
p.Style.Width = Length.Pct(50);          // 50% of parent
p.Style.Width = Length.Auto;             // auto-size
```

#### Edges (Padding / Margin)

```csharp
p.Style.Padding = new Edges(12);                // all sides
p.Style.Padding = new Edges(8, 16);             // vertical, horizontal
p.Style.Padding = new Edges(4, 8, 12, 16);      // top, right, bottom, left
```

---

## Immediate-Mode UI (Legacy)

The existing ImGui wrapper provides an immediate-mode API for HUDs, menus, and in-game interfaces. It remains functional but is expected to be scoped to editor-only in a future release.

### Core Pattern

Call `UI.Draw()` every frame you want UI visible:

```csharp
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.UI;

public class HudComponent : Component
{
    private float _health01 = 0.75f;
    private bool _showDebug;

    public override void Update(float dt)
    {
        UI.Draw(ctx =>
        {
            using var panel = ctx.Panel("HUD", new UiPanelOptions
            {
                Position = new System.Numerics.Vector2(16, 16),
                Size = new System.Numerics.Vector2(320, 120),
                HasTitleBar = false,
                Movable = false,
                Resizable = false
            });

            if (!panel.IsVisible)
                return;

            ctx.Text("Player HUD", 24f);
            ctx.ProgressBar("health", _health01, overlayText: "Health");
            ctx.Checkbox("debug_toggle", "Show Debug", ref _showDebug);
        });
    }
}
```

### Available Widgets

#### Layout

| Method | Description |
|--------|-------------|
| `Panel(id, options)` | Create a UI panel/window (returns `UiScope`) |
| `Horizontal(id, columns)` | Table layout with column count |
| `Vertical(id)` | Vertical grouping scope |
| `NextCell()` | Advance to next cell in horizontal layout |
| `Spacer(width, height)` | Empty spacer region (default height: 8px) |
| `SameLine(offset, spacing)` | Place next widget on the same line |
| `Separator()` | Draw a horizontal separator line |

#### Display

| Method | Description |
|--------|-------------|
| `Text(text, fontPx, style)` | Draw text with optional font size and style |
| `ProgressBar(id, value01, size, overlayText)` | Draw a progress bar (0.0 to 1.0) |
| `Image(image, size, flipY)` | Draw a texture image |

#### Input

| Method | Description |
|--------|-------------|
| `Button(id, label, fontPx, size, disabled)` | Clickable button (returns true when clicked) |
| `Checkbox(id, label, ref value, fontPx, disabled)` | Toggle checkbox (returns true when changed) |
| `SliderFloat(id, label, ref value, min, max, fontPx, disabled)` | Float slider (returns true when changed) |

### Panel Options

`UiPanelOptions` controls panel appearance and behavior:

| Property | Default | Description |
|----------|---------|-------------|
| `Position` | null | Panel position in pixels |
| `Size` | null | Panel size in pixels |
| `HasTitleBar` | false | Show a title bar |
| `Movable` | false | Allow dragging to move |
| `Resizable` | false | Allow resize handles |
| `NoBackground` | false | Hide panel background |
| `AutoResize` | false | Auto-size to fit contents |
| `Scrollbar` | false | Show scrollbar on overflow |

### Styling

`UiStyle` provides per-widget styling:

| Property | Description |
|----------|-------------|
| `TextColor` | Override text color (Vector4 RGBA) |
| `WrapWidth` | Text wrap width in pixels (0 = no wrapping) |
| `Disabled` | Render in disabled visual state |

### Input Capture

`UI.InputCapture` reports whether UI is consuming input, so your game can avoid processing input that the UI is handling:

```csharp
if (!UI.InputCapture.WantsMouse)
{
    // Process game mouse input
}

if (!UI.InputCapture.WantsKeyboard)
{
    // Process game keyboard input
}
```

| Property | Description |
|----------|-------------|
| `WantsMouse` | UI is consuming mouse input |
| `WantsKeyboard` | UI is consuming keyboard input |
| `WantsTextInput` | UI is actively receiving text input |
| `Any` | Any capture flag is active |

### Font Sizes

Pass pixel values (e.g. `24f`) to the `fontPx` parameter on any widget. The default font size is used when `fontPx` is `0`.

---

## Debug Screen Text

`DebugDraw.PrintString()` displays temporary on-screen messages, similar to Unreal Engine's Print String node. Messages appear as a text overlay in the top-left of the viewport and automatically disappear after their duration expires.

```csharp
using FrinkyEngine.Core.Rendering;

// Simple message (green, 5 seconds)
DebugDraw.PrintString("Hello world!");

// Custom color and duration
DebugDraw.PrintString("Health low!", 3f, new System.Numerics.Vector4(1, 0.3f, 0.3f, 1));

// Keyed message — replaces existing entry with the same key
DebugDraw.PrintString($"FPS: {fps}", 0.5f, key: "fps_counter");
```

| Parameter | Default | Description |
|-----------|---------|-------------|
| `message` | required | Text to display |
| `duration` | `5f` | Seconds to show the message |
| `color` | green | RGBA `Vector4` (0-1 per channel) |
| `key` | `null` | If set, replaces any existing message with the same key |

**Editor-only**: Debug text renders in the editor viewport overlay. In runtime builds, `DebugDraw.PrintString` is a no-op. The `debug_print` console command can also display debug text.

## See Also

- [CanvasUI Roadmap](CANVASUI_ROADMAP.md) — phased plan for the new retained-mode UI system
- [UI Roadmap](roadmaps/ui_roadmap.md) — legacy ImGui UI plans
