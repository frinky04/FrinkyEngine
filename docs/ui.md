# Game UI

Game scripts can build runtime UI through `FrinkyEngine.Core.UI` without directly using ImGui APIs. The wrapper provides an immediate-mode API for HUDs, menus, and in-game interfaces.

## Core Pattern

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

## Available Widgets

### Layout

| Method | Description |
|--------|-------------|
| `Panel(id, options)` | Create a UI panel/window (returns `UiScope`) |
| `Horizontal(id, columns)` | Table layout with column count |
| `Vertical(id)` | Vertical grouping scope |
| `NextCell()` | Advance to next cell in horizontal layout |
| `Spacer(width, height)` | Empty spacer region (default height: 8px) |
| `SameLine(offset, spacing)` | Place next widget on the same line |
| `Separator()` | Draw a horizontal separator line |

### Display

| Method | Description |
|--------|-------------|
| `Text(text, fontPx, style)` | Draw text with optional font size and style |
| `ProgressBar(id, value01, size, overlayText)` | Draw a progress bar (0.0 to 1.0) |
| `Image(image, size, flipY)` | Draw a texture image |

### Input

| Method | Description |
|--------|-------------|
| `Button(id, label, fontPx, size, disabled)` | Clickable button (returns true when clicked) |
| `Checkbox(id, label, ref value, fontPx, disabled)` | Toggle checkbox (returns true when changed) |
| `SliderFloat(id, label, ref value, min, max, fontPx, disabled)` | Float slider (returns true when changed) |

## Panel Options

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

## Styling

`UiStyle` provides per-widget styling:

| Property | Description |
|----------|-------------|
| `TextColor` | Override text color (Vector4 RGBA) |
| `WrapWidth` | Text wrap width in pixels (0 = no wrapping) |
| `Disabled` | Render in disabled visual state |

## Input Capture

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

## Font Sizes

Dynamic font sizes are available per widget — pass pixel values (e.g. `24f`) to the `fontPx` parameter. The default font size is used when `fontPx` is `0`.

## Notes

- The API is immediate mode — call `UI.Draw(...)` every frame you want UI visible
- Runtime and editor Play/Simulate modes use the same game UI path
- Layout scopes (`Panel`, `Horizontal`, `Vertical`) use `using` for automatic cleanup

## See Also

- [UI Roadmap](roadmaps/ui_roadmap.md) — planned UI features
