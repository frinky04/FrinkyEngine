using System.Reflection;
using System.Text.RegularExpressions;

namespace FrinkyEngine.Core.Rendering.PostProcessing;

/// <summary>
/// Discovers and resolves <see cref="PostProcessEffect"/> subclasses from engine and game assemblies.
/// Parallel to <see cref="FrinkyEngine.Core.Serialization.ComponentTypeResolver"/> but for post-processing effects.
/// </summary>
public static class PostProcessEffectResolver
{
    private static readonly Dictionary<string, Type> _typeMap = new();
    private static readonly Dictionary<Assembly, List<string>> _assemblyKeys = new();
    private static readonly Assembly _engineAssembly = typeof(PostProcessEffect).Assembly;

    static PostProcessEffectResolver()
    {
        RegisterAssembly(_engineAssembly);
    }

    /// <summary>
    /// Scans an assembly for concrete <see cref="PostProcessEffect"/> subclasses and registers them.
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
            if (type.IsSubclassOf(typeof(PostProcessEffect)) && !type.IsAbstract)
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
    /// Removes all effect types that were registered from the specified assembly.
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
    /// Gets all distinct registered effect types across all assemblies.
    /// </summary>
    /// <returns>An enumerable of effect types.</returns>
    public static IEnumerable<Type> GetAllEffectTypes() => _typeMap.Values.Distinct();

    /// <summary>
    /// Gets the fully qualified type name used as a serialization key.
    /// </summary>
    /// <param name="type">The effect type.</param>
    /// <returns>The full type name.</returns>
    public static string GetTypeName(Type type) => type.FullName!;

    /// <summary>
    /// Gets a human-readable label indicating whether a type comes from the engine or a game assembly.
    /// </summary>
    /// <param name="type">The effect type.</param>
    /// <returns>"Engine" for built-in types, or the game assembly name.</returns>
    public static string GetAssemblySource(Type type)
    {
        return type.Assembly == _engineAssembly ? "Engine" : type.Assembly.GetName().Name ?? "Game";
    }

    private static readonly Regex PascalCaseRegex =
        new(@"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z][a-z])", RegexOptions.Compiled);

    /// <summary>
    /// Gets a human-readable display name for an effect type by creating an instance and reading <see cref="PostProcessEffect.DisplayName"/>.
    /// Falls back to stripping "Effect" suffix and inserting spaces.
    /// </summary>
    /// <param name="type">The effect type.</param>
    /// <returns>A display name string.</returns>
    public static string GetDisplayName(Type type)
    {
        try
        {
            var instance = (PostProcessEffect)Activator.CreateInstance(type)!;
            return instance.DisplayName;
        }
        catch
        {
            var name = type.Name;
            if (name.EndsWith("Effect"))
                name = name[..^"Effect".Length];
            return PascalCaseRegex.Replace(name, " $1$2");
        }
    }
}
