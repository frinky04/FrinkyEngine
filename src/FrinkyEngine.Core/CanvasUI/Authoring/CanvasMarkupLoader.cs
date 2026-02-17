using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FrinkyEngine.Core.CanvasUI.Styles.Css;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal static class CanvasMarkupLoader
{
    private static readonly Dictionary<string, Type> PanelTypes = BuildPanelTypeMap();

    public static Panel LoadIntoParent(RootPanel root, Panel parent, string markup)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(markup, LoadOptions.SetLineInfo);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"CanvasUI markup parse failed: {ex.Message}", ex);
        }

        if (doc.Root == null)
            throw new InvalidOperationException("CanvasUI markup must contain a root element.");

        return BuildElement(root, parent, doc.Root);
    }

    private static Panel BuildElement(RootPanel root, Panel parent, XElement element)
    {
        var panelType = ResolvePanelType(element.Name.LocalName);
        if (panelType == null)
            throw BuildLocationException(element, $"Unknown panel tag '{element.Name.LocalName}'.");

        var panel = (Panel?)Activator.CreateInstance(panelType);
        if (panel == null)
            throw BuildLocationException(element, $"Could not instantiate panel type '{panelType.Name}'.");

        // Apply attributes (classes, inline styles, bindings) before adding to the tree,
        // so that OnCreated (called by AddChild) sees the correct classes and styles.
        ApplyAttributes(root, panel, element);
        parent.AddChild(panel);

        foreach (var childElement in element.Elements())
            BuildElement(root, panel, childElement);

        return panel;
    }

    private static void ApplyAttributes(RootPanel root, Panel panel, XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            string attrName = attr.Name.LocalName.Trim();
            string attrValue = attr.Value;
            if (string.IsNullOrEmpty(attrName))
                continue;

            if (attrName.Equals("class", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var cls in attrValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    panel.AddClass(cls);
                continue;
            }

            if (attrName.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                ApplyInlineStyle(panel, attrValue);
                continue;
            }

            bool isBinding = CanvasValueConverter.TryParseBindingExpression(attrValue, out string sourceProperty);

            if (attrName.Equals("context", StringComparison.OrdinalIgnoreCase))
            {
                if (isBinding)
                {
                    root.BindingManager.RegisterContextBinding(panel, sourceProperty);
                }
                else
                {
                    WarnWithLocation(attr, $"Static 'context' value '{attrValue}' is unsupported. Use binding syntax like '{{ViewModel}}'.");
                }
                continue;
            }

            if (attrName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                BindEvent(panel, attrName, attrValue, attr);
                continue;
            }

            var panelProp = ResolvePanelProperty(panel.GetType(), attrName);
            if (panelProp == null || !panelProp.CanWrite)
            {
                WarnWithLocation(attr, $"Unknown or read-only attribute '{attrName}' on panel '{panel.GetType().Name}'.");
                continue;
            }

            if (isBinding)
            {
                root.BindingManager.RegisterPropertyBinding(panel, panelProp, sourceProperty);
                continue;
            }

            if (!CanvasValueConverter.TryConvertValue(attrValue, panelProp.PropertyType, out object? converted))
            {
                WarnWithLocation(attr, $"Could not convert '{attrValue}' to {panelProp.PropertyType.Name} for '{panelProp.Name}'.");
                continue;
            }

            try
            {
                panelProp.SetValue(panel, converted);
                panel.InvalidateLayout();
            }
            catch (Exception ex)
            {
                WarnWithLocation(attr, $"Failed to set '{panelProp.Name}': {ex.Message}");
            }
        }
    }

    private static void ApplyInlineStyle(Panel panel, string styleText)
    {
        if (string.IsNullOrWhiteSpace(styleText))
            return;

        try
        {
            var rules = CssParser.Parse($"* {{ {styleText} }}");
            if (rules.Count == 0)
                return;
            CanvasStyleSheetMerge.MergeInto(panel.Style, rules[0].Declarations);
            panel.InvalidateLayout();
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI inline style parse failed: {ex.Message}");
        }
    }

    private static void BindEvent(Panel panel, string attrName, string methodName, XAttribute attr)
    {
        string suffix = attrName[2..];
        if (string.IsNullOrWhiteSpace(suffix))
            return;

        string eventName = "On" + ToPascalCase(suffix);
        var evt = panel.GetType().GetEvent(
            eventName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

        if (evt == null)
        {
            WarnWithLocation(attr, $"Event '{eventName}' not found on panel '{panel.GetType().Name}'.");
            return;
        }

        if (!CanvasEventBinder.TryBind(panel, evt, methodName.Trim()))
            WarnWithLocation(attr, $"Unsupported event signature for '{eventName}' on panel '{panel.GetType().Name}'.");
    }

    private static PropertyInfo? ResolvePanelProperty(Type panelType, string attributeName)
    {
        string candidate = ToPascalCase(attributeName);
        return panelType.GetProperty(
            candidate,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
    }

    private static Type? ResolvePanelType(string tagName)
    {
        PanelTypes.TryGetValue(tagName, out var type);
        return type;
    }

    private static Dictionary<string, Type> BuildPanelTypeMap()
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var panelBase = typeof(Panel);

        foreach (var type in panelBase.Assembly.GetTypes())
        {
            if (!panelBase.IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                continue;
            if (type == typeof(RootPanel))
                continue;
            if (type.GetConstructor(Type.EmptyTypes) == null)
                continue;

            map[type.Name] = type;
            if (!string.IsNullOrEmpty(type.FullName))
                map[type.FullName] = type;
        }

        return map;
    }

    private static void WarnWithLocation(XObject xmlNode, string message)
    {
        FrinkyLog.Warning($"{message} {FormatLocation(xmlNode)}");
    }

    private static Exception BuildLocationException(XObject xmlNode, string message)
    {
        return new InvalidOperationException($"{message} {FormatLocation(xmlNode)}");
    }

    private static string FormatLocation(XObject xmlNode)
    {
        if (xmlNode is IXmlLineInfo li && li.HasLineInfo())
            return $"(line {li.LineNumber}, col {li.LinePosition})";
        return "(line unknown)";
    }

    private static string ToPascalCase(string raw)
    {
        var parts = raw
            .Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return raw;
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
