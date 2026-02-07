using System.Reflection;
using System.Runtime.Loader;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Scripting;

public class GameAssemblyLoader
{
    private sealed class GameAssemblyLoadContext : AssemblyLoadContext
    {
        private static readonly string EngineAssemblyName = typeof(Component).Assembly.GetName().Name ?? "FrinkyEngine.Core";
        private readonly AssemblyDependencyResolver _resolver;

        public GameAssemblyLoadContext(string gameAssemblyPath)
            : base("GameAssembly", isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(gameAssemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Share engine types with the default context so `Component` type identity matches.
            if (string.Equals(assemblyName.Name, EngineAssemblyName, StringComparison.OrdinalIgnoreCase))
                return typeof(Component).Assembly;

            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (!string.IsNullOrWhiteSpace(assemblyPath))
                return LoadFromAssemblyPath(assemblyPath);

            return null;
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (!string.IsNullOrWhiteSpace(libraryPath))
                return LoadUnmanagedDllFromPath(libraryPath);

            return nint.Zero;
        }
    }

    private AssemblyLoadContext? _loadContext;
    private Assembly? _gameAssembly;

    public Assembly? GameAssembly => _gameAssembly;

    public bool LoadAssembly(string dllPath)
    {
        if (!File.Exists(dllPath))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(dllPath);
            _loadContext = new GameAssemblyLoadContext(fullPath);
            _gameAssembly = _loadContext.LoadFromAssemblyPath(fullPath);
            ComponentTypeResolver.RegisterAssembly(_gameAssembly);
            return true;
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderMessages = ex.LoaderExceptions
                .Where(e => e != null)
                .Select(e => e!.Message)
                .Distinct()
                .ToList();
            if (loaderMessages.Count > 0)
                Rendering.FrinkyLog.Error("Failed to load game assembly: " + string.Join(" | ", loaderMessages));
            else
                Rendering.FrinkyLog.Error($"Failed to load game assembly: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Rendering.FrinkyLog.Error($"Failed to load game assembly: {ex.Message}");
            return false;
        }
    }

    public void Unload()
    {
        if (_gameAssembly != null)
            ComponentTypeResolver.UnregisterAssembly(_gameAssembly);

        _gameAssembly = null;
        _loadContext?.Unload();
        _loadContext = null;
    }

    public bool ReloadAssembly(string dllPath)
    {
        Unload();
        return LoadAssembly(dllPath);
    }
}
