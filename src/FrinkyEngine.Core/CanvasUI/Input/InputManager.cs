using System.Numerics;
using FrinkyEngine.Core.CanvasUI.Events;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Input;

internal class InputManager
{
    private Panel? _hoveredPanel;
    private Panel? _focusedPanel;
    private Panel? _activePanel;

    public Vector2 MousePosition { get; internal set; }

    public void Reset()
    {
        _hoveredPanel = null;
        _focusedPanel = null;
        _activePanel = null;
    }

    public void ProcessInput(RootPanel root)
    {
        float mx = MousePosition.X;
        float my = MousePosition.Y;
        var mousePos = MousePosition;

        // Hit test
        var hit = HitTest(root, mx, my);

        // Hover tracking
        if (hit != _hoveredPanel)
        {
            if (_hoveredPanel != null)
            {
                _hoveredPanel.PseudoClasses &= ~PseudoClassFlags.Hover;
                _hoveredPanel.RaiseMouseOut(new MouseEvent { ScreenPos = mousePos, Target = _hoveredPanel });
            }

            _hoveredPanel = hit;

            if (_hoveredPanel != null)
            {
                _hoveredPanel.PseudoClasses |= PseudoClassFlags.Hover;
                _hoveredPanel.RaiseMouseOver(new MouseEvent { ScreenPos = mousePos, Target = _hoveredPanel });
            }
        }

        // Mouse down
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            _activePanel = _hoveredPanel;
            if (_activePanel != null)
            {
                _activePanel.PseudoClasses |= PseudoClassFlags.Active;
                var localPos = new Vector2(mx - _activePanel.Box.X, my - _activePanel.Box.Y);
                _activePanel.RaiseMouseDown(new MouseEvent
                {
                    ScreenPos = mousePos, LocalPos = localPos,
                    Button = MouseButton.Left, Target = _activePanel
                });
            }

            // Focus management
            var focusTarget = _hoveredPanel?.AcceptsFocus == true ? _hoveredPanel : null;
            if (focusTarget != _focusedPanel)
            {
                if (_focusedPanel != null)
                {
                    _focusedPanel.PseudoClasses &= ~PseudoClassFlags.Focus;
                    _focusedPanel.RaiseBlur(new FocusEvent { Target = _focusedPanel, RelatedTarget = focusTarget });
                }
                _focusedPanel = focusTarget;
                if (_focusedPanel != null)
                {
                    _focusedPanel.PseudoClasses |= PseudoClassFlags.Focus;
                    _focusedPanel.RaiseFocus(new FocusEvent { Target = _focusedPanel });
                }
            }
        }

        // Mouse up
        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            if (_activePanel != null)
            {
                _activePanel.PseudoClasses &= ~PseudoClassFlags.Active;
                var localPos = new Vector2(mx - _activePanel.Box.X, my - _activePanel.Box.Y);
                _activePanel.RaiseMouseUp(new MouseEvent
                {
                    ScreenPos = mousePos, LocalPos = localPos,
                    Button = MouseButton.Left, Target = _activePanel
                });

                // Click if released on same panel
                if (_activePanel == _hoveredPanel)
                {
                    _activePanel.RaiseClick(new MouseEvent
                    {
                        ScreenPos = mousePos, LocalPos = localPos,
                        Button = MouseButton.Left, Target = _activePanel
                    });
                }

                _activePanel = null;
            }
        }

        // Mouse wheel — bubble up ancestors until handled
        var wheelDelta = Raylib.GetMouseWheelMoveV();
        if (wheelDelta.X != 0 || wheelDelta.Y != 0)
        {
            var target = _hoveredPanel ?? _focusedPanel;
            while (target != null)
            {
                target.RaiseMouseWheel(wheelDelta);
                target = target.Parent;
            }
        }

        // Keyboard input — dispatch to focused panel
        if (_focusedPanel != null)
        {
            // Key presses (special keys)
            int key = Raylib.GetKeyPressed();
            while (key != 0)
            {
                _focusedPanel.RaiseKeyDown(new KeyboardEvent
                {
                    Key = (KeyboardKey)key,
                    Target = _focusedPanel
                });
                key = Raylib.GetKeyPressed();
            }

            // Character input (text characters)
            int ch = Raylib.GetCharPressed();
            while (ch != 0)
            {
                _focusedPanel.RaiseKeyPress(new KeyboardEvent
                {
                    Character = (char)ch,
                    Target = _focusedPanel
                });
                ch = Raylib.GetCharPressed();
            }
        }
    }

    private static Panel? HitTest(Panel panel, float x, float y)
    {
        if (panel.ComputedStyle.Display == Styles.Display.None) return null;

        // Test children in reverse order (front to back)
        for (int i = panel.Children.Count - 1; i >= 0; i--)
        {
            var hit = HitTest(panel.Children[i], x, y);
            if (hit != null) return hit;
        }

        // Test self (skip root panel which is the full screen)
        if (panel is not RootPanel && panel.Box.Contains(x, y))
            return panel;

        return null;
    }
}
