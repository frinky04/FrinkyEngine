using System.Numerics;
using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Events;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI;

public class Panel
{
    private static int _nextId;
    private readonly List<Panel> _children = new();
    private bool _isCreated;

    public int Id { get; } = Interlocked.Increment(ref _nextId);
    public List<string> Classes { get; } = new();

    public Panel? Parent { get; internal set; }
    public IReadOnlyList<Panel> Children => _children;

    public StyleSheet Style { get; } = new();
    public ComputedStyle ComputedStyle;
    public PseudoClassFlags PseudoClasses { get; set; }

    internal YogaNode YogaNode { get; } = new();
    public Box Box { get; internal set; }

    // Events
    public event Action<MouseEvent>? OnClick;
    public event Action<MouseEvent>? OnMouseOver;
    public event Action<MouseEvent>? OnMouseOut;
    public event Action<MouseEvent>? OnMouseDown;
    public event Action<MouseEvent>? OnMouseUp;
    public event Action<FocusEvent>? OnFocus;
    public event Action<FocusEvent>? OnBlur;
    public event Action<KeyboardEvent>? OnKeyDown;
    public event Action<KeyboardEvent>? OnKeyPress;
    public event Action<Vector2>? OnMouseWheel;

    public bool AcceptsFocus { get; set; }
    public float ScrollOffsetY { get; set; }

    public T AddChild<T>(Action<T>? configure = null) where T : Panel, new()
    {
        var child = new T();
        AttachChildInternal(child);
        configure?.Invoke(child);
        EnsureCreated(child);
        return child;
    }

    public void AddChild(Panel child)
    {
        AttachChildInternal(child);
        EnsureCreated(child);
    }

    public void RemoveChild(Panel child)
    {
        if (!_children.Remove(child)) return;
        YogaNode.RemoveChild(child.YogaNode);
        child.Parent = null;
    }

    public void Delete()
    {
        OnDeleted();
        _isCreated = false;
        Parent?.RemoveChild(this);
        DeleteChildren();
    }

    public void DeleteChildren()
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            child.OnDeleted();
            child._isCreated = false;
            child.DeleteChildren();
        }
        _children.Clear();
        YogaNode.Clear();
    }

    private void AttachChildInternal(Panel child)
    {
        if (child.Parent != null)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        _children.Add(child);
        YogaNode.AddChild(child.YogaNode);
    }

    private static void EnsureCreated(Panel child)
    {
        if (child._isCreated) return;
        child.OnCreated();
        child._isCreated = true;
    }

    public bool HasClass(string className) => Classes.Contains(className);

    public void AddClass(string className)
    {
        if (!Classes.Contains(className))
            Classes.Add(className);
    }

    public void RemoveClass(string className) => Classes.Remove(className);

    public void ToggleClass(string className)
    {
        if (Classes.Contains(className))
            Classes.Remove(className);
        else
            Classes.Add(className);
    }

    // Rendering — override in subclasses to draw content (text, images, etc.)
    public virtual void RenderContent(Box box, ComputedStyle style, byte alpha) { }

    // Lifecycle
    public virtual void OnCreated() { }
    public virtual void OnDeleted() { }
    public virtual void Tick(float dt) { }

    // Internal event dispatchers
    internal void RaiseClick(MouseEvent e) => OnClick?.Invoke(e);
    internal void RaiseMouseOver(MouseEvent e) => OnMouseOver?.Invoke(e);
    internal void RaiseMouseOut(MouseEvent e) => OnMouseOut?.Invoke(e);
    internal void RaiseMouseDown(MouseEvent e) => OnMouseDown?.Invoke(e);
    internal void RaiseMouseUp(MouseEvent e) => OnMouseUp?.Invoke(e);
    internal void RaiseFocus(FocusEvent e) => OnFocus?.Invoke(e);
    internal void RaiseBlur(FocusEvent e) => OnBlur?.Invoke(e);
    internal void RaiseKeyDown(KeyboardEvent e) => OnKeyDown?.Invoke(e);
    internal void RaiseKeyPress(KeyboardEvent e) => OnKeyPress?.Invoke(e);
    internal void RaiseMouseWheel(Vector2 delta) => OnMouseWheel?.Invoke(delta);
}
