#pragma warning disable CS1591

using System.Reflection;
using System.Text.RegularExpressions;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// Discovers and resolves <see cref="FObject"/> subclasses from engine and game assemblies.
/// </summary>
public static class FObjectTypeResolver
{
    private static readonly Dictionary<string, Type> _typeMap = new();
    private static readonly Dictionary<Assembly, List<string>> _assemblyKeys = new();
    private static readonly Assembly _engineAssembly = typeof(FObject).Assembly;

    static FObjectTypeResolver()
    {
        RegisterAssembly(_engineAssembly);
    }

    /// <summary>
    /// Scans an assembly for concrete <see cref="FObject"/> subclasses and registers them.
    /// </summary>
    public static void RegisterAssembly(Assembly assembly)
    {
        var keys = new List<string>();
        IEnumerable<Type> candidateTypes;

        try
        {
            candidateTypes = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            candidateTypes = ex.Types.Where(t => t != null)!;
        }

        foreach (var type in candidateTypes)
        {
            if (type.IsSubclassOf(typeof(FObject)) && !type.IsAbstract)
            {
                _typeMap[type.FullName!] = type;
                _typeMap[type.Name] = type;
                keys.Add(type.FullName!);
                keys.Add(type.Name);
            }
        }

        _assemblyKeys[assembly] = keys;
    }

    /// <summary>
    /// Removes all FObject types that were registered from the specified assembly.
    /// </summary>
    public static void UnregisterAssembly(Assembly assembly)
    {
        if (!_assemblyKeys.TryGetValue(assembly, out var keys))
            return;

        foreach (var key in keys)
            _typeMap.Remove(key);

        _assemblyKeys.Remove(assembly);
    }

    /// <summary>
    /// Resolves a type name (short or fully qualified) to a <see cref="Type"/>.
    /// </summary>
    public static Type? Resolve(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var type))
            return type;
        return null;
    }

    /// <summary>
    /// Gets all distinct registered FObject types across all assemblies.
    /// </summary>
    public static IEnumerable<Type> GetAllTypes() => _typeMap.Values.Distinct();

    /// <summary>
    /// Gets all registered FObject types that are assignable to the specified base type.
    /// </summary>
    public static IEnumerable<Type> GetTypesAssignableTo(Type baseType)
    {
        return _typeMap.Values.Distinct().Where(t => baseType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets the fully qualified type name used as a serialization key.
    /// </summary>
    public static string GetTypeName(Type type) => type.FullName!;

    /// <summary>
    /// Gets a human-readable label indicating whether a type comes from the engine or a game assembly.
    /// </summary>
    public static string GetAssemblySource(Type type)
    {
        return type.Assembly == _engineAssembly ? "Engine" : type.Assembly.GetName().Name ?? "Game";
    }

    private static readonly Regex PascalCaseRegex =
        new(@"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);

    /// <summary>
    /// Gets a human-readable display name for an FObject type.
    /// </summary>
    public static string GetDisplayName(Type type)
    {
        try
        {
            var instance = (FObject)Activator.CreateInstance(type)!;
            return instance.DisplayName;
        }
        catch
        {
            return PascalCaseRegex.Replace(type.Name, " $1$2");
        }
    }
}
