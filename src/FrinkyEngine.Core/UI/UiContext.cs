using System.Numerics;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Wrapper API for immediate-mode UI drawing. Game code should use this type instead of raw ImGui.
/// </summary>
public sealed class UiContext
{
    internal UiContext()
    {
    }

    /// <summary>
    /// Begins a panel scope.
    /// </summary>
    /// <param name="id">Unique panel identifier.</param>
    /// <param name="options">Panel layout and behavior options.</param>
    /// <returns>A disposable scope that ends the panel.</returns>
    public UiScope Panel(string id, UiPanelOptions options = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Panel id cannot be empty.", nameof(id));

        if (options.Position.HasValue)
            ImGui.SetNextWindowPos(options.Position.Value, ImGuiCond.Always);
        if (options.Size.HasValue)
            ImGui.SetNextWindowSize(options.Size.Value, ImGuiCond.Always);

        var flags = ImGuiWindowFlags.NoSavedSettings;
        if (!options.HasTitleBar)
            flags |= ImGuiWindowFlags.NoTitleBar;
        if (!options.Movable)
            flags |= ImGuiWindowFlags.NoMove;
        if (!options.Resizable)
            flags |= ImGuiWindowFlags.NoResize;
        if (options.NoBackground)
            flags |= ImGuiWindowFlags.NoBackground;
        if (!options.Scrollbar)
            flags |= ImGuiWindowFlags.NoScrollbar;
        if (options.AutoResize)
            flags |= ImGuiWindowFlags.AlwaysAutoResize;

        bool visible = ImGui.Begin(id, flags);
        return new UiScope(static () => ImGui.End(), visible);
    }

    /// <summary>
    /// Begins a horizontal layout with the specified column count.
    /// </summary>
    /// <param name="id">Unique layout identifier.</param>
    /// <param name="columns">Number of columns.</param>
    /// <returns>A disposable scope that ends the horizontal layout.</returns>
    public UiScope Horizontal(string id, int columns)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Layout id cannot be empty.", nameof(id));
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns), "Column count must be greater than zero.");

        bool opened = ImGui.BeginTable(id, columns, ImGuiTableFlags.SizingStretchProp);
        return new UiScope(opened ? static () => ImGui.EndTable() : null, opened);
    }

    /// <summary>
    /// Begins a vertical grouping scope.
    /// </summary>
    /// <param name="id">Unique group identifier.</param>
    /// <returns>A disposable scope that ends the group.</returns>
    public UiScope Vertical(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Group id cannot be empty.", nameof(id));

        ImGui.PushID(id);
        ImGui.BeginGroup();
        return new UiScope(static () =>
        {
            ImGui.EndGroup();
            ImGui.PopID();
        }, true);
    }

    /// <summary>
    /// Advances to the next cell in a horizontal table layout.
    /// </summary>
    public void NextCell()
    {
        ImGui.TableNextColumn();
    }

    /// <summary>
    /// Emits an empty spacer region.
    /// </summary>
    /// <param name="width">Spacer width in pixels.</param>
    /// <param name="height">Spacer height in pixels.</param>
    public void Spacer(float width = 0f, float height = 8f)
    {
        ImGui.Dummy(new Vector2(MathF.Max(0f, width), MathF.Max(0f, height)));
    }

    /// <summary>
    /// Places the next widget on the same line.
    /// </summary>
    /// <param name="offsetFromStartX">Optional absolute offset from line start.</param>
    /// <param name="spacing">Optional spacing override.</param>
    public void SameLine(float offsetFromStartX = 0f, float spacing = -1f)
    {
        ImGui.SameLine(offsetFromStartX, spacing);
    }

    /// <summary>
    /// Draws a separator line.
    /// </summary>
    public void Separator()
    {
        ImGui.Separator();
    }

    /// <summary>
    /// Draws plain text.
    /// </summary>
    /// <param name="text">Text content.</param>
    /// <param name="fontPx">Optional font size in pixels. Values less than or equal to zero use current style defaults.</param>
    /// <param name="style">Optional style overrides.</param>
    public void Text(string text, float fontPx = 0f, UiStyle style = default)
    {
        using var fontScope = new FontScope(fontPx);
        using var styleScope = new StyleScope(style);
        ImGui.TextUnformatted(text ?? string.Empty);
    }

    /// <summary>
    /// Draws a clickable button.
    /// </summary>
    /// <param name="id">Stable widget identifier.</param>
    /// <param name="label">Visible label.</param>
    /// <param name="fontPx">Optional font size in pixels.</param>
    /// <param name="size">Optional button size.</param>
    /// <param name="disabled">When <c>true</c>, button is disabled.</param>
    /// <returns><c>true</c> when the button is clicked this frame.</returns>
    public bool Button(string id, string label, float fontPx = 0f, Vector2? size = null, bool disabled = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Widget id cannot be empty.", nameof(id));

        ImGui.PushID(id);
        using var fontScope = new FontScope(fontPx);
        if (disabled)
            ImGui.BeginDisabled();

        bool clicked = size.HasValue
            ? ImGui.Button(label, size.Value)
            : ImGui.Button(label);

        if (disabled)
            ImGui.EndDisabled();
        ImGui.PopID();
        return clicked;
    }

    /// <summary>
    /// Draws a checkbox and edits the provided value.
    /// </summary>
    /// <param name="id">Stable widget identifier.</param>
    /// <param name="label">Visible label.</param>
    /// <param name="value">Value to edit.</param>
    /// <param name="fontPx">Optional font size in pixels.</param>
    /// <param name="disabled">When <c>true</c>, checkbox is disabled.</param>
    /// <returns><c>true</c> when the value changed this frame.</returns>
    public bool Checkbox(string id, string label, ref bool value, float fontPx = 0f, bool disabled = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Widget id cannot be empty.", nameof(id));

        ImGui.PushID(id);
        using var fontScope = new FontScope(fontPx);
        if (disabled)
            ImGui.BeginDisabled();

        bool changed = ImGui.Checkbox(label, ref value);

        if (disabled)
            ImGui.EndDisabled();
        ImGui.PopID();
        return changed;
    }

    /// <summary>
    /// Draws a float slider and edits the provided value.
    /// </summary>
    /// <param name="id">Stable widget identifier.</param>
    /// <param name="label">Visible label.</param>
    /// <param name="value">Value to edit.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="fontPx">Optional font size in pixels.</param>
    /// <param name="disabled">When <c>true</c>, slider is disabled.</param>
    /// <returns><c>true</c> when the value changed this frame.</returns>
    public bool SliderFloat(string id, string label, ref float value, float min, float max, float fontPx = 0f, bool disabled = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Widget id cannot be empty.", nameof(id));

        ImGui.PushID(id);
        using var fontScope = new FontScope(fontPx);
        if (disabled)
            ImGui.BeginDisabled();

        bool changed = ImGui.SliderFloat(label, ref value, min, max);

        if (disabled)
            ImGui.EndDisabled();
        ImGui.PopID();
        return changed;
    }

    /// <summary>
    /// Draws a progress bar.
    /// </summary>
    /// <param name="id">Stable widget identifier.</param>
    /// <param name="value01">Progress value in [0, 1].</param>
    /// <param name="size">Optional size override.</param>
    /// <param name="overlayText">Optional overlay text.</param>
    public void ProgressBar(string id, float value01, Vector2? size = null, string? overlayText = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Widget id cannot be empty.", nameof(id));

        ImGui.PushID(id);
        var clamped = Math.Clamp(value01, 0f, 1f);
        ImGui.ProgressBar(clamped, size ?? new Vector2(-1f, 0f), overlayText);
        ImGui.PopID();
    }

    /// <summary>
    /// Draws an image.
    /// </summary>
    /// <param name="image">Image handle to draw.</param>
    /// <param name="size">Image size in pixels.</param>
    /// <param name="flipY">When <c>true</c>, flips image vertically (useful for render textures).</param>
    public unsafe void Image(UiImageHandle image, Vector2 size, bool flipY = false)
    {
        if (!image.IsValid || size.X <= 0f || size.Y <= 0f)
            return;

        var uv0 = Vector2.Zero;
        var uv1 = flipY ? new Vector2(1f, -1f) : Vector2.One;
        ImGui.Image(new ImTextureRef(null, new ImTextureID(image.TextureId)), size, uv0, uv1);
    }

    private readonly struct FontScope : IDisposable
    {
        private readonly bool _active;

        public FontScope(float fontPx)
        {
            if (fontPx > 0f)
            {
                ImGui.PushFont(ImGui.GetFont(), fontPx);
                _active = true;
            }
        }

        public void Dispose()
        {
            if (_active)
                ImGui.PopFont();
        }
    }

    private readonly struct StyleScope : IDisposable
    {
        private readonly bool _hasTextColor;
        private readonly bool _hasWrap;
        private readonly bool _disabled;

        public StyleScope(UiStyle style)
        {
            _hasTextColor = style.TextColor.HasValue;
            _hasWrap = style.WrapWidth > 0f;
            _disabled = style.Disabled;

            if (_hasTextColor)
                ImGui.PushStyleColor(ImGuiCol.Text, style.TextColor!.Value);
            if (_hasWrap)
                ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + style.WrapWidth);
            if (_disabled)
                ImGui.BeginDisabled();
        }

        public void Dispose()
        {
            if (_disabled)
                ImGui.EndDisabled();
            if (_hasWrap)
                ImGui.PopTextWrapPos();
            if (_hasTextColor)
                ImGui.PopStyleColor();
        }
    }
}
