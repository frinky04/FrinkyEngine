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

    public void NotifyPanelRemoved(Panel panel)
    {
        if (_hoveredPanel != null && Panel.IsSameOrDescendant(_hoveredPanel, panel))
        {
            _hoveredPanel.PseudoClasses &= ~PseudoClassFlags.Hover;
            _hoveredPanel = null;
        }

        if (_activePanel != null && Panel.IsSameOrDescendant(_activePanel, panel))
        {
            _activePanel.PseudoClasses &= ~PseudoClassFlags.Active;
            _activePanel = null;
        }

        if (_focusedPanel != null && Panel.IsSameOrDescendant(_focusedPanel, panel))
        {
            var blurred = _focusedPanel;
            blurred.PseudoClasses &= ~PseudoClassFlags.Focus;
            _focusedPanel = null;
            blurred.RaiseBlur(new FocusEvent
            {
                Target = blurred,
                RelatedTarget = null
            });
        }
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
                var wheelEvent = new MouseWheelEvent
                {
                    Delta = wheelDelta,
                    Target = target
                };
                target.RaiseMouseWheel(wheelEvent);
                if (wheelEvent.Handled)
                    break;
                target = target.Parent;
            }
        }

        // Keyboard input — dispatch to focused panel
        if (_focusedPanel != null)
        {
            // Key presses (special keys)
            int key = Raylib.GetKeyPressed();
            while (key != 0 && _focusedPanel != null)
            {
                var focused = _focusedPanel;
                focused.RaiseKeyDown(new KeyboardEvent
                {
                    Key = (KeyboardKey)key,
                    Target = focused
                });
                key = Raylib.GetKeyPressed();
            }

            // Character input (text characters)
            int ch = Raylib.GetCharPressed();
            while (ch != 0 && _focusedPanel != null)
            {
                var focused = _focusedPanel;
                focused.RaiseKeyPress(new KeyboardEvent
                {
                    Character = (char)ch,
                    Target = focused
                });
                ch = Raylib.GetCharPressed();
            }
        }
    }

    private static Panel? HitTest(Panel panel, float x, float y, Box? inheritedClip = null)
    {
        if (panel.ComputedStyle.Display == Styles.Display.None) return null;

        Box? effectiveClip = inheritedClip;
        if (panel.ComputedStyle.Overflow == Styles.Overflow.Hidden)
        {
            effectiveClip = effectiveClip.HasValue
                ? Box.Intersect(effectiveClip.Value, panel.Box)
                : panel.Box;
        }

        if (effectiveClip.HasValue)
        {
            var clip = effectiveClip.Value;
            if (clip.Width <= 0 || clip.Height <= 0 || !clip.Contains(x, y))
                return null;
        }

        // Test children in reverse order (front to back)
        for (int i = panel.Children.Count - 1; i >= 0; i--)
        {
            var hit = HitTest(panel.Children[i], x, y, effectiveClip);
            if (hit != null) return hit;
        }

        // Test self (skip root panel which is the full screen)
        if (panel is not RootPanel && panel.Box.Contains(x, y))
            return panel;

        return null;
    }
}
