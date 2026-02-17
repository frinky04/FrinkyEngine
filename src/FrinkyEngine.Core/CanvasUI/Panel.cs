using System.Numerics;
using Facebook.Yoga;
using FrinkyEngine.Core.CanvasUI.Events;
using FrinkyEngine.Core.CanvasUI.Styles;

namespace FrinkyEngine.Core.CanvasUI;

public class Panel
{
    private static int _nextId;
    private readonly List<Panel> _children = new();

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

    public bool AcceptsFocus { get; set; }

    public T AddChild<T>(Action<T>? configure = null) where T : Panel, new()
    {
        var child = new T();
        child.Parent = this;
        _children.Add(child);
        YogaNode.AddChild(child.YogaNode);
        configure?.Invoke(child);
        child.OnCreated();
        return child;
    }

    public void AddChild(Panel child)
    {
        if (child.Parent != null)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        _children.Add(child);
        YogaNode.AddChild(child.YogaNode);
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
        Parent?.RemoveChild(this);
        DeleteChildren();
    }

    public void DeleteChildren()
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            child.OnDeleted();
            child.DeleteChildren();
        }
        _children.Clear();
        YogaNode.Clear();
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

    public Panel? FindById(int id)
    {
        if (Id == id) return this;
        foreach (var child in _children)
        {
            var found = child.FindById(id);
            if (found != null) return found;
        }
        return null;
    }
}
