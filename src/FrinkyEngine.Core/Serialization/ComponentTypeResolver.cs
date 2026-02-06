using System.Reflection;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;

public static class ComponentTypeResolver
{
    private static readonly Dictionary<string, Type> _typeMap = new();
    private static readonly Dictionary<Assembly, List<string>> _assemblyKeys = new();
    private static readonly Assembly _engineAssembly = typeof(Component).Assembly;

    static ComponentTypeResolver()
    {
        RegisterAssembly(_engineAssembly);
    }

    public static void RegisterAssembly(Assembly assembly)
    {
        var keys = new List<string>();

        foreach (var type in assembly.GetTypes())
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

    public static void UnregisterAssembly(Assembly assembly)
    {
        if (!_assemblyKeys.TryGetValue(assembly, out var keys))
            return;

        foreach (var key in keys)
            _typeMap.Remove(key);

        _assemblyKeys.Remove(assembly);
    }

    public static Type? Resolve(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var type))
            return type;
        return null;
    }

    public static IEnumerable<Type> GetAllComponentTypes() => _typeMap.Values.Distinct();

    public static string GetTypeName(Type type) => type.FullName!;

    public static string GetAssemblySource(Type type)
    {
        return type.Assembly == _engineAssembly ? "Engine" : type.Assembly.GetName().Name ?? "Game";
    }
}
