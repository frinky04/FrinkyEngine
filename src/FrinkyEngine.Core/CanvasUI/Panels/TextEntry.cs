using System.Numerics;
using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Events;
using FrinkyEngine.Core.CanvasUI.Rendering;
using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

public class TextEntry : Panel
{
    private string _text = string.Empty;
    private int _cursorPos;
    private int _selectionStart = -1;
    private float _cursorBlinkTimer;
    private bool _cursorVisible = true;

    public string Text
    {
        get => _text;
        set
        {
            var newText = value ?? string.Empty;
            if (MaxLength.HasValue && newText.Length > MaxLength.Value)
                newText = newText[..MaxLength.Value];
            if (_text != newText)
            {
                _text = newText;
                _cursorPos = Math.Min(_cursorPos, _text.Length);
                if (_selectionStart >= 0)
                    _selectionStart = Math.Min(_selectionStart, _text.Length);
                NotifyTextChanged();
            }
        }
    }

    public int CursorPos
    {
        get => _cursorPos;
        set => _cursorPos = Math.Clamp(value, 0, _text.Length);
    }

    public int SelectionStart
    {
        get => _selectionStart;
        set => _selectionStart = value;
    }

    public string Placeholder { get; set; } = string.Empty;
    public int? MaxLength { get; set; }

    public event Action<string>? OnTextChanged;
    public event Action<string>? OnSubmit;

    public override void OnCreated()
    {
        AcceptsFocus = true;

        OnKeyPress += HandleCharInput;
        OnKeyDown += HandleKeyDown;
        OnMouseDown += HandleMouseDown;

        UpdateMeasureFunction();
    }

    public override void Tick(float dt)
    {
        if (PseudoClasses.HasFlag(PseudoClassFlags.Focus))
        {
            _cursorBlinkTimer += dt;
            if (_cursorBlinkTimer >= 0.5f)
            {
                _cursorBlinkTimer -= 0.5f;
                _cursorVisible = !_cursorVisible;
            }
        }
    }

    private void ResetBlink()
    {
        _cursorBlinkTimer = 0f;
        _cursorVisible = true;
    }

    private void NotifyTextChanged()
    {
        OnTextChanged?.Invoke(_text);
        UpdateMeasureFunction();
    }

    private bool HasSelection => _selectionStart >= 0 && _selectionStart != _cursorPos;

    private (int start, int end) GetSelectionRange()
    {
        int s = Math.Min(_selectionStart, _cursorPos);
        int e = Math.Max(_selectionStart, _cursorPos);
        return (s, e);
    }

    private void DeleteSelection()
    {
        if (!HasSelection) return;
        var (start, end) = GetSelectionRange();
        _text = _text.Remove(start, end - start);
        _cursorPos = start;
        _selectionStart = -1;
        NotifyTextChanged();
    }

    private void HandleCharInput(KeyboardEvent e)
    {
        if (e.Character < 32) return;

        ResetBlink();

        if (HasSelection)
            DeleteSelection();

        if (MaxLength.HasValue && _text.Length >= MaxLength.Value)
            return;

        _text = _text.Insert(_cursorPos, e.Character.ToString());
        _cursorPos++;
        NotifyTextChanged();
        e.Handled = true;
    }

    private void HandleKeyDown(KeyboardEvent e)
    {
        ResetBlink();
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);

        switch (e.Key)
        {
            case KeyboardKey.Backspace:
                if (HasSelection)
                    DeleteSelection();
                else if (_cursorPos > 0)
                {
                    _text = _text.Remove(_cursorPos - 1, 1);
                    _cursorPos--;
                    NotifyTextChanged();
                }
                e.Handled = true;
                break;

            case KeyboardKey.Delete:
                if (HasSelection)
                    DeleteSelection();
                else if (_cursorPos < _text.Length)
                {
                    _text = _text.Remove(_cursorPos, 1);
                    NotifyTextChanged();
                }
                e.Handled = true;
                break;

            case KeyboardKey.Left:
                if (shift && _selectionStart < 0) _selectionStart = _cursorPos;
                if (!shift && HasSelection) { _cursorPos = GetSelectionRange().start; _selectionStart = -1; }
                else if (_cursorPos > 0) _cursorPos--;
                if (!shift) _selectionStart = -1;
                e.Handled = true;
                break;

            case KeyboardKey.Right:
                if (shift && _selectionStart < 0) _selectionStart = _cursorPos;
                if (!shift && HasSelection) { _cursorPos = GetSelectionRange().end; _selectionStart = -1; }
                else if (_cursorPos < _text.Length) _cursorPos++;
                if (!shift) _selectionStart = -1;
                e.Handled = true;
                break;

            case KeyboardKey.Home:
                if (shift && _selectionStart < 0) _selectionStart = _cursorPos;
                _cursorPos = 0;
                if (!shift) _selectionStart = -1;
                e.Handled = true;
                break;

            case KeyboardKey.End:
                if (shift && _selectionStart < 0) _selectionStart = _cursorPos;
                _cursorPos = _text.Length;
                if (!shift) _selectionStart = -1;
                e.Handled = true;
                break;

            case KeyboardKey.Enter:
                OnSubmit?.Invoke(_text);
                e.Handled = true;
                break;

            case KeyboardKey.A when ctrl:
                _selectionStart = 0;
                _cursorPos = _text.Length;
                e.Handled = true;
                break;

            case KeyboardKey.C when ctrl:
                if (HasSelection)
                {
                    var (s, en) = GetSelectionRange();
                    Raylib.SetClipboardText(_text[s..en]);
                }
                e.Handled = true;
                break;

            case KeyboardKey.V when ctrl:
                {
                    var clip = Raylib.GetClipboardText_();
                    if (!string.IsNullOrEmpty(clip))
                    {
                        if (HasSelection) DeleteSelection();
                        _text = _text.Insert(_cursorPos, clip);
                        _cursorPos += clip.Length;
                        if (MaxLength.HasValue && _text.Length > MaxLength.Value)
                        {
                            _text = _text[..MaxLength.Value];
                            _cursorPos = Math.Min(_cursorPos, _text.Length);
                        }
                        NotifyTextChanged();
                    }
                }
                e.Handled = true;
                break;

            case KeyboardKey.X when ctrl:
                if (HasSelection)
                {
                    var (s, en) = GetSelectionRange();
                    Raylib.SetClipboardText(_text[s..en]);
                    DeleteSelection();
                }
                e.Handled = true;
                break;
        }
    }

    private void HandleMouseDown(MouseEvent e)
    {
        ResetBlink();
        _selectionStart = -1;

        var renderer = CanvasRenderer.Current;
        if (renderer == null) return;

        float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;
        float padL = YogaNode.LayoutPaddingLeft;
        float clickX = e.LocalPos.X - padL;

        // Find cursor position from click X
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i <= _text.Length; i++)
        {
            float charX = DrawCommands.MeasureText(_text[..i], fontSize, renderer.FontManager.DefaultFont).X;
            float dist = MathF.Abs(charX - clickX);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        _cursorPos = best;
    }

    public override void RenderContent(Box box, ComputedStyle style, byte alpha)
    {
        var renderer = CanvasRenderer.Current;
        if (renderer == null) return;

        float fontSize = style.FontSize;
        float padL = YogaNode.LayoutPaddingLeft;
        float padT = YogaNode.LayoutPaddingTop;
        float textY = box.Y + padT + (box.Height - padT * 2 - fontSize) * 0.5f;
        float textX = box.X + padL;
        var font = renderer.FontManager.DefaultFont;

        // Selection highlight
        if (HasSelection)
        {
            var (s, e) = GetSelectionRange();
            float selStartX = textX + DrawCommands.MeasureText(_text[..s], fontSize, font).X;
            float selEndX = textX + DrawCommands.MeasureText(_text[..e], fontSize, font).X;
            var selColor = new Color(60, 120, 200, 140);
            DrawCommands.RoundedRect(selStartX, textY, selEndX - selStartX, fontSize, 0f,
                CanvasRenderer.AlphaBlend(selColor, alpha));
        }

        // Text or placeholder
        if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(Placeholder))
        {
            var placeholderColor = new Color(120, 120, 140, 180);
            DrawCommands.Text(Placeholder, textX, textY, fontSize,
                CanvasRenderer.AlphaBlend(placeholderColor, alpha), font);
        }
        else if (!string.IsNullOrEmpty(_text))
        {
            DrawCommands.Text(_text, textX, textY, fontSize,
                CanvasRenderer.AlphaBlend(style.Color, alpha), font);
        }

        // Cursor
        bool focused = PseudoClasses.HasFlag(PseudoClassFlags.Focus);
        if (focused && _cursorVisible)
        {
            float cursorX = textX + DrawCommands.MeasureText(_text[.._cursorPos], fontSize, font).X;
            var cursorColor = CanvasRenderer.AlphaBlend(style.Color, alpha);
            Raylib.DrawLineEx(new Vector2(cursorX, textY), new Vector2(cursorX, textY + fontSize), 1.5f, cursorColor);
        }
    }

    private void UpdateMeasureFunction()
    {
        YogaNode.SetMeasureFunction((node, width, widthMode, height, heightMode) =>
        {
            float fontSize = ComputedStyle.FontSize > 0 ? ComputedStyle.FontSize : 16f;
            string measureText = !string.IsNullOrEmpty(_text) ? _text : Placeholder;
            float textWidth = string.IsNullOrEmpty(measureText) ? fontSize * 5 : measureText.Length * fontSize * 0.6f;
            float textHeight = fontSize;

            float w = widthMode == YogaMeasureMode.Exactly ? width
                    : widthMode == YogaMeasureMode.AtMost ? MathF.Min(textWidth, width)
                    : textWidth;

            float h = heightMode == YogaMeasureMode.Exactly ? height
                    : heightMode == YogaMeasureMode.AtMost ? MathF.Min(textHeight, height)
                    : textHeight;

            return MeasureOutput.Make(w, h);
        });
    }
}
