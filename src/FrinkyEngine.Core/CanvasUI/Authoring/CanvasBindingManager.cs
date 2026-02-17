using System.ComponentModel;
using System.Reflection;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal sealed class CanvasBindingManager
{
    private readonly List<PropertyBinding> _propertyBindings = new();
    private readonly List<ContextBinding> _contextBindings = new();
    private readonly Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> _subscriptions = new();
    private bool _bindingsDirty = true;
    private bool _subscriptionsDirty = true;
    private bool _isUpdating;

    private static readonly Dictionary<(Type Type, string Name), PropertyInfo?> SourcePropertyCache = new();

    public void Clear()
    {
        foreach (var (source, handler) in _subscriptions)
            source.PropertyChanged -= handler;
        _subscriptions.Clear();
        _propertyBindings.Clear();
        _contextBindings.Clear();
        _bindingsDirty = true;
        _subscriptionsDirty = true;
        ClearSourcePropertyCache();
    }

    internal static void ClearSourcePropertyCache()
    {
        SourcePropertyCache.Clear();
    }

    public void NotifyBindingsChanged()
    {
        _bindingsDirty = true;
        _subscriptionsDirty = true;
    }

    public void NotifyPanelContextChanged(Panel _)
    {
        _bindingsDirty = true;
        _subscriptionsDirty = true;
    }

    public void RemoveBindingsForSubtree(Panel root)
    {
        _propertyBindings.RemoveAll(b => IsSameOrDescendant(b.TargetPanel, root));
        _contextBindings.RemoveAll(b => IsSameOrDescendant(b.TargetPanel, root));
        _bindingsDirty = true;
        _subscriptionsDirty = true;
    }

    public void RegisterPropertyBinding(Panel panel, PropertyInfo targetProperty, string sourceProperty)
    {
        _propertyBindings.Add(new PropertyBinding(panel, targetProperty, sourceProperty));
        _bindingsDirty = true;
        _subscriptionsDirty = true;
    }

    public void RegisterContextBinding(Panel panel, string sourceProperty)
    {
        _contextBindings.Add(new ContextBinding(panel, sourceProperty));
        _bindingsDirty = true;
        _subscriptionsDirty = true;
    }

    public void Update()
    {
        if (_propertyBindings.Count == 0 && _contextBindings.Count == 0)
            return;

        if (_subscriptionsDirty)
            RebuildSubscriptions();
        if (!_bindingsDirty)
            return;

        _isUpdating = true;
        try
        {
            foreach (var binding in _contextBindings)
                ApplyContextBinding(binding);

            foreach (var binding in _propertyBindings)
                ApplyPropertyBinding(binding);
        }
        finally
        {
            _isUpdating = false;
            _bindingsDirty = false;
        }
    }

    private void RebuildSubscriptions()
    {
        foreach (var (source, handler) in _subscriptions)
            source.PropertyChanged -= handler;
        _subscriptions.Clear();

        foreach (var binding in _contextBindings)
            TrySubscribe(binding.TargetPanel.Parent?.BindingContext);
        foreach (var binding in _propertyBindings)
            TrySubscribe(binding.TargetPanel.BindingContext);

        _subscriptionsDirty = false;
    }

    private void TrySubscribe(object? context)
    {
        if (context is not INotifyPropertyChanged npc || _subscriptions.ContainsKey(npc))
            return;

        PropertyChangedEventHandler handler = (_, _) =>
        {
            _bindingsDirty = true;
            if (!_isUpdating)
                _subscriptionsDirty = true;
        };

        npc.PropertyChanged += handler;
        _subscriptions[npc] = handler;
    }

    private void ApplyContextBinding(ContextBinding binding)
    {
        var parentContext = binding.TargetPanel.Parent?.BindingContext;
        if (parentContext == null)
            return;

        if (!TryGetSourceValue(parentContext, binding.SourceProperty, out var contextValue))
            return;

        if (ReferenceEquals(binding.TargetPanel.BindingContext, contextValue))
            return;

        binding.TargetPanel.SetBindingContext(contextValue);
    }

    private void ApplyPropertyBinding(PropertyBinding binding)
    {
        if (!IsInTree(binding.TargetPanel))
            return;

        var context = binding.TargetPanel.BindingContext;
        if (context == null)
            return;
        if (!TryGetSourceValue(context, binding.SourceProperty, out var sourceValue))
            return;

        if (!CanvasValueConverter.TryConvertValue(sourceValue, binding.TargetProperty.PropertyType, out var converted))
            return;

        var currentValue = binding.TargetProperty.GetValue(binding.TargetPanel);
        if (AreEqual(currentValue, converted))
            return;

        try
        {
            binding.TargetProperty.SetValue(binding.TargetPanel, converted);
            binding.TargetPanel.InvalidateLayout();
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI binding set failed: {binding.TargetPanel.GetType().Name}.{binding.TargetProperty.Name} ({ex.Message})");
        }
    }

    private static bool TryGetSourceValue(object context, string sourceProperty, out object? value)
    {
        value = null;
        var key = (context.GetType(), sourceProperty);
        if (!SourcePropertyCache.TryGetValue(key, out var sourceProp))
        {
            sourceProp = context.GetType().GetProperty(
                sourceProperty,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            SourcePropertyCache[key] = sourceProp;
        }

        if (sourceProp == null)
            return false;

        value = sourceProp.GetValue(context);
        return true;
    }

    private static bool AreEqual(object? a, object? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a == null || b == null)
            return false;
        return a.Equals(b);
    }

    private static bool IsInTree(Panel panel)
    {
        if (panel is RootPanel)
            return true;
        return panel.Parent != null;
    }

    private static bool IsSameOrDescendant(Panel candidate, Panel ancestor)
    {
        for (Panel? current = candidate; current != null; current = current.Parent)
        {
            if (ReferenceEquals(current, ancestor))
                return true;
        }
        return false;
    }

    private readonly record struct PropertyBinding(
        Panel TargetPanel,
        PropertyInfo TargetProperty,
        string SourceProperty);

    private readonly record struct ContextBinding(
        Panel TargetPanel,
        string SourceProperty);
}
