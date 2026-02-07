using System.Reflection;
using System.Text.RegularExpressions;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;

/// <summary>
/// Maps component type names to <see cref="Type"/> objects for scene deserialization.
/// Supports registering external assemblies (e.g. game scripts) for custom component types.
/// </summary>
public static class ComponentTypeResolver
{
    private static readonly Dictionary<string, Type> _typeMap = new();
    private static readonly Dictionary<Assembly, List<string>> _assemblyKeys = new();
    private static readonly Assembly _engineAssembly = typeof(Component).Assembly;

    static ComponentTypeResolver()
    {
        RegisterAssembly(_engineAssembly);
    }

    /// <summary>
    /// Scans an assembly for concrete <see cref="Component"/> subclasses and registers them by both short name and full name.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
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
            if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
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
    /// Removes all component types that were registered from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to unregister.</param>
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
    /// <param name="typeName">The type name to look up.</param>
    /// <returns>The resolved type, or <c>null</c> if not found.</returns>
    public static Type? Resolve(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var type))
            return type;
        return null;
    }

    /// <summary>
    /// Gets all distinct registered component types across all assemblies.
    /// </summary>
    /// <returns>An enumerable of component types.</returns>
    public static IEnumerable<Type> GetAllComponentTypes() => _typeMap.Values.Distinct();

    /// <summary>
    /// Gets the fully qualified type name used as a serialization key.
    /// </summary>
    /// <param name="type">The component type.</param>
    /// <returns>The full type name.</returns>
    public static string GetTypeName(Type type) => type.FullName!;

    /// <summary>
    /// Gets a human-readable label indicating whether a type comes from the engine or a game assembly.
    /// </summary>
    /// <param name="type">The component type.</param>
    /// <returns>"Engine" for built-in types, or the game assembly name.</returns>
    public static string GetAssemblySource(Type type)
    {
        return type.Assembly == _engineAssembly ? "Engine" : type.Assembly.GetName().Name ?? "Game";
    }

    /// <summary>
    /// Gets the category declared via <see cref="ComponentCategoryAttribute"/>, or <c>null</c> if none.
    /// Categories support slash-separated nesting (e.g. "Physics/Colliders").
    /// </summary>
    public static string? GetCategory(Type type)
    {
        return type.GetCustomAttribute<ComponentCategoryAttribute>()?.Category;
    }

    private static readonly Regex PascalCaseRegex =
        new(@"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);

    /// <summary>
    /// Gets a human-readable display name for a component type.
    /// Returns the <see cref="ComponentDisplayNameAttribute"/> value if present,
    /// otherwise strips "Component" suffix and inserts spaces between PascalCase words.
    /// </summary>
    public static string GetDisplayName(Type type)
    {
        var attr = type.GetCustomAttribute<ComponentDisplayNameAttribute>();
        if (attr != null)
            return attr.DisplayName;

        var name = type.Name;
        if (name.EndsWith("Component"))
            name = name[..^"Component".Length];

        return PascalCaseRegex.Replace(name, " $1$2");
    }
}
