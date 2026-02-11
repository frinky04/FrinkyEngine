using System.Numerics;
using System.Reflection;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Editor.Panels;

internal static class InspectorReflectionHelpers
{
    public static bool IsInspectableComponentProperty(PropertyInfo prop)
    {
        if (!prop.CanRead)
            return false;
        if (prop.GetCustomAttribute<InspectorHiddenAttribute>() != null)
            return false;
        if (prop.Name is "Entity" or "HasStarted" or "Enabled" or "RenderModel")
            return false;
        if (prop.CanWrite)
            return true;
        return prop.GetCustomAttribute<InspectorReadOnlyAttribute>() != null;
    }

    public static bool IsListType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    public static bool IsInlineObjectType(Type type)
    {
        if (type == typeof(string)
            || type.IsPrimitive
            || type.IsEnum
            || type == typeof(decimal)
            || type == typeof(Vector2)
            || type == typeof(Vector3)
            || type == typeof(Quaternion)
            || type == typeof(Color)
            || type == typeof(EntityReference)
            || type == typeof(AssetReference)
            || typeof(FObject).IsAssignableFrom(type))
        {
            return false;
        }

        return type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum);
    }

    public static bool TryEvaluateBoolMember(object target, string memberName, out bool value)
    {
        value = false;
        var type = target.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var property = type.GetProperty(memberName, flags);
        if (property != null && property.CanRead && property.PropertyType == typeof(bool))
        {
            value = (bool?)property.GetValue(target) ?? false;
            return true;
        }

        var field = type.GetField(memberName, flags);
        if (field != null && field.FieldType == typeof(bool))
        {
            value = (bool?)field.GetValue(target) ?? false;
            return true;
        }

        var method = type.GetMethod(memberName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
        if (method != null && method.ReturnType == typeof(bool))
        {
            value = (bool?)method.Invoke(target, null) ?? false;
            return true;
        }

        return false;
    }

    public static bool TryEvaluateEnumMember(object target, string memberName, out Enum? value)
    {
        value = null;
        var type = target.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var property = type.GetProperty(memberName, flags);
        if (property != null && property.CanRead && property.PropertyType.IsEnum)
        {
            value = property.GetValue(target) as Enum;
            return value != null;
        }

        var field = type.GetField(memberName, flags);
        if (field != null && field.FieldType.IsEnum)
        {
            value = field.GetValue(target) as Enum;
            return value != null;
        }

        var method = type.GetMethod(memberName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
        if (method != null && method.ReturnType.IsEnum)
        {
            value = method.Invoke(target, null) as Enum;
            return value != null;
        }

        return false;
    }
}
