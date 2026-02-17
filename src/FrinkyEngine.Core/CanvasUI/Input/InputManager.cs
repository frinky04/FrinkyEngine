using System.Numerics;
using FrinkyEngine.Core.CanvasUI.Events;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Input;

internal class InputManager
{
    private Panel? _hoveredPanel;
    private Panel? _focusedPanel;
    private Panel? _activePanel;

    public Panel? HoveredPanel => _hoveredPanel;
    public Panel? FocusedPanel => _focusedPanel;

    public bool WantsMouse => _hoveredPanel != null;
    public bool WantsKeyboard => _focusedPanel != null;

    public void ProcessInput(RootPanel root, Vector2? mouseOverride = null)
    {
        var rawPos = mouseOverride ?? new Vector2(Raylib.GetMouseX(), Raylib.GetMouseY());
        float mx = rawPos.X;
        float my = rawPos.Y;
        var mousePos = rawPos;

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
