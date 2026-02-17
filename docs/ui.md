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
| `OnMouseWheel` | Mouse wheel scrolled over this panel (bubbles up to ancestors) |
| `OnFocus` | Panel received keyboard focus |
| `OnBlur` | Panel lost keyboard focus |
| `OnKeyDown` | Key pressed while panel has focus (receives `KeyboardEvent` with `Key`) |
| `OnKeyPress` | Text character typed while panel has focus (receives `KeyboardEvent` with `Character`) |

#### Classes

Add string class names to a panel with `AddClass()`, `RemoveClass()`, `ToggleClass()`, and check with `HasClass()`. Classes are used for CSS selector matching (see [CSS Styling](#css-styling) below).

#### Custom Panels

Subclass `Panel` to create reusable UI elements. Override these methods:

| Method | Description |
|--------|-------------|
| `OnCreated()` | Called after the panel is added to the tree — set up children and defaults here |
| `OnDeleted()` | Called when the panel is removed — clean up subscriptions here |
| `Tick(dt)` | Called every frame — use for animations or per-frame updates |
| `RenderContent(box, style, alpha)` | Override to draw custom content (text, images, shapes) |

Set `AcceptsFocus = true` in `OnCreated()` if your panel needs keyboard focus.

### Built-in Panels

#### Label

Displays text. Set the `Text` property.

```csharp
var label = parent.AddChild<Label>(l => l.Text = "Score: 0");
label.Text = "Score: 100"; // update later
```

#### Button

Clickable panel with centered text. Accepts focus by default.

```csharp
var btn = parent.AddChild<Button>(b => b.Text = "Start Game");
btn.OnClick += _ => StartGame();
```

#### ProgressBar

Horizontal bar showing a 0–1 value. Draws a track and a fill — no children needed.

| Property | Default | Description |
|----------|---------|-------------|
| `Value` | `0` | Fill amount, clamped 0–1 |
| `TrackColor` | dark gray | Background track color (nullable — set `null` to use default) |
| `FillColor` | green | Fill color (nullable) |

```csharp
var hp = parent.AddChild<ProgressBar>(p =>
{
    p.AddClass("health-bar");
    p.Value = 0.75f;
    p.FillColor = new Color(74, 222, 128, 255);
});

// Update each frame
hp.Value = currentHealth / maxHealth;
```

Style the size with CSS or inline:

```css
ProgressBar.health-bar { width: 200px; height: 8px; border-radius: 4px; }
```

#### Checkbox

Toggle with an optional text label. Accepts focus by default. Toggles the `:checked` pseudo-class.

| Property | Default | Description |
|----------|---------|-------------|
| `Checked` | `false` | Current toggle state |
| `Text` | `""` | Label text displayed next to the checkbox |

| Event | Description |
|-------|-------------|
| `OnChanged` | Fires with the new `bool` state when toggled |

```csharp
var mute = parent.AddChild<Checkbox>(c =>
{
    c.Text = "Mute Audio";
    c.Checked = false;
});
mute.OnChanged += isMuted => AudioManager.SetMuted(isMuted);
```

Style the checked state with CSS:

```css
Checkbox { color: #aaa; font-size: 16px; }
Checkbox:checked { color: #4ade80; }
```

#### Slider

Horizontal range slider with drag and keyboard arrow support. Accepts focus by default.

| Property | Default | Description |
|----------|---------|-------------|
| `Value` | `0` | Normalized position, clamped 0–1 |
| `Min` | `0` | Minimum of the mapped range |
| `Max` | `1` | Maximum of the mapped range |
| `Step` | `0.05` | Increment for keyboard arrows |
| `MappedValue` | (read-only) | `Min + Value * (Max - Min)` |

| Event | Description |
|-------|-------------|
| `OnChanged` | Fires with the mapped value on drag or keyboard change |

```csharp
var volume = parent.AddChild<Slider>(s =>
{
    s.Min = 0f;
    s.Max = 100f;
    s.Value = 0.8f; // 80%
});
volume.OnChanged += v => AudioManager.SetVolume(v);
```

Drag the thumb or use Left/Right arrow keys when focused.

#### TextEntry

Single-line text input with cursor, selection, clipboard, and placeholder support. Accepts focus by default.

| Property | Default | Description |
|----------|---------|-------------|
| `Text` | `""` | Current text content |
| `Placeholder` | `""` | Gray hint text shown when empty |
| `MaxLength` | `null` | Optional character limit |
| `CursorPos` | `0` | Current cursor position |

| Event | Description |
|-------|-------------|
| `OnTextChanged` | Fires with the new text on every edit |
| `OnSubmit` | Fires with the text when Enter is pressed |

```csharp
var nameField = parent.AddChild<TextEntry>(t =>
{
    t.Placeholder = "Enter player name...";
    t.MaxLength = 20;
});
nameField.OnSubmit += name => SetPlayerName(name);
```

Keyboard support: typing, Backspace, Delete, Home, End, arrow keys (with Shift for selection), Ctrl+A/C/V/X for clipboard, Enter to submit.

#### ScrollPanel

Scrollable container. Children that extend beyond the panel's height are clipped and can be scrolled into view with the mouse wheel. A subtle scrollbar indicator appears when content overflows.

| Property | Default | Description |
|----------|---------|-------------|
| `ScrollOffsetY` | `0` | Current vertical scroll position in pixels |
| `ScrollSpeed` | `30` | Pixels scrolled per mouse wheel tick |

```csharp
var scroll = parent.AddChild<ScrollPanel>(s =>
{
    s.Style.Height = 300;
});

// Add more content than fits
for (int i = 0; i < 20; i++)
    scroll.AddChild<Label>(l => l.Text = $"Item {i}");
```

`ScrollPanel` automatically sets `Overflow = Hidden` and clamps the scroll offset to the content bounds.

#### Image

Displays a Raylib `Texture2D`. Reports texture dimensions as intrinsic size for layout.

| Property | Default | Description |
|----------|---------|-------------|
| `Texture` | `null` | The texture to display |
| `Tint` | white | Color tint applied to the texture |

```csharp
var icon = parent.AddChild<Image>(img =>
{
    img.Texture = myTexture;
    img.Style.Width = 64;
    img.Style.Height = 64;
});
```

If no explicit size is set, the panel sizes itself to the texture dimensions.

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

### CSS Styling

Style panels with CSS strings instead of (or alongside) inline C# style properties. Load one or more stylesheets, and they apply automatically to all panels via selector matching.

```csharp
CanvasUI.LoadStyleSheet(@"
    .hud { flex-direction: row; padding: 12px; gap: 8px; }
    Label { color: white; font-size: 20px; }
    Button { background-color: #3366cc; padding: 8px 16px; border-radius: 4px; }
    Button:hover { background-color: #4477dd; }
");
```

Inline `panel.Style` properties always win over CSS rules — use CSS for defaults and class-based theming, and inline styles for one-off overrides.

#### Selectors

| Selector | Example | Matches |
|----------|---------|---------|
| Type | `Label` | All `Label` panels |
| Class | `.hud` | Panels with `AddClass("hud")` |
| Pseudo-class | `:hover`, `:active`, `:focus`, `:disabled`, `:checked` | Panels in that interaction state |
| Universal | `*` | All panels |
| Descendant | `.hud Label` | Labels anywhere inside a `.hud` panel |
| Child | `.hud > Label` | Labels that are direct children of `.hud` |
| Combined | `Button.primary:hover` | Hovered buttons with class `primary` |

Multiple selectors can share a rule with commas: `Label, Button { color: white; }`

#### Supported Properties

All properties from the [Layout](#layout-properties) and [Visual](#visual-properties) tables are supported in CSS using kebab-case names:

- **Layout**: `flex-direction`, `justify-content`, `align-items`, `align-self`, `display`, `position`, `overflow`, `width`, `height`, `min-width`, `min-height`, `max-width`, `max-height`, `flex-grow`, `flex-shrink`, `flex-basis`, `gap`, `top`, `right`, `bottom`, `left`
- **Spacing**: `padding`, `padding-top`, `padding-right`, `padding-bottom`, `padding-left`, `margin` (and sides)
- **Visual**: `background-color`, `color`, `border-color`, `border-width`, `border-radius`, `font-size`, `opacity`
- **Shorthand**: `border` (e.g. `border: 2px #ff0000`)

#### Color Values

Colors can be specified as:

```css
color: white;               /* named color */
color: #ff0000;             /* hex RGB */
color: #ff000080;           /* hex RGBA */
color: #f00;                /* short hex */
color: rgb(255, 0, 0);      /* rgb() function */
color: rgba(255, 0, 0, 0.5); /* rgba() with 0-1 alpha */
```

#### Length Values

```css
width: 200px;   /* pixels */
width: 50%;     /* percentage of parent */
width: auto;    /* auto-size */
width: 200;     /* unitless = pixels */
```

#### Specificity

When multiple rules match the same panel, more specific selectors win. Specificity is calculated as:

1. Number of class selectors + pseudo-class selectors
2. Number of type selectors

Higher specificity overrides lower. Equal specificity uses source order (later rules win). Inline `panel.Style` always takes highest priority.

#### Managing Stylesheets

| Method | Description |
|--------|-------------|
| `CanvasUI.LoadStyleSheet(css)` | Parse and add CSS rules (can call multiple times to layer stylesheets) |
| `CanvasUI.ClearStyleSheets()` | Remove all loaded CSS rules |

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
