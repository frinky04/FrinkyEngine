using System.Reflection;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Serialization;

public static class ComponentTypeResolver
{
    private static readonly Dictionary<string, Type> _typeMap = new();

    static ComponentTypeResolver()
    {
        RegisterAssembly(typeof(Component).Assembly);
    }

    public static void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
            {
                _typeMap[type.FullName!] = type;
                _typeMap[type.Name] = type;
            }
        }
    }

    public static Type? Resolve(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var type))
            return type;
        return null;
    }

    public static IEnumerable<Type> GetAllComponentTypes() => _typeMap.Values.Distinct();

    public static string GetTypeName(Type type) => type.FullName!;
}
