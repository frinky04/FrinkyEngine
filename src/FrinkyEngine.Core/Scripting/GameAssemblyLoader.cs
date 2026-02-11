using System.Reflection;
using System.Runtime.Loader;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Scripting;

/// <summary>
/// Loads and unloads game script assemblies using a collectible <see cref="AssemblyLoadContext"/> for hot-reload support.
/// Automatically registers loaded component types with <see cref="ComponentTypeResolver"/>.
/// </summary>
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

    /// <summary>
    /// The currently loaded game assembly, or <c>null</c> if none is loaded.
    /// </summary>
    public Assembly? GameAssembly => _gameAssembly;

    /// <summary>
    /// Loads a game assembly from the specified DLL path into an isolated load context.
    /// </summary>
    /// <param name="dllPath">Path to the game assembly DLL.</param>
    /// <returns><c>true</c> if the assembly was loaded successfully.</returns>
    public bool LoadAssembly(string dllPath)
    {
        if (!File.Exists(dllPath))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(dllPath);
            _loadContext = new GameAssemblyLoadContext(fullPath);
            var bytes = File.ReadAllBytes(fullPath);
            _gameAssembly = _loadContext.LoadFromStream(new MemoryStream(bytes));
            ComponentTypeResolver.RegisterAssembly(_gameAssembly);
            FObjectTypeResolver.RegisterAssembly(_gameAssembly);
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

    /// <summary>
    /// Unloads the current game assembly and its load context, unregistering all its component types.
    /// </summary>
    public void Unload()
    {
        if (_gameAssembly != null)
        {
            FObjectTypeResolver.UnregisterAssembly(_gameAssembly);
            ComponentTypeResolver.UnregisterAssembly(_gameAssembly);
        }

        _gameAssembly = null;
        _loadContext?.Unload();
        _loadContext = null;
    }

    /// <summary>
    /// Unloads the current assembly and loads a new one from the specified path (hot-reload).
    /// </summary>
    /// <param name="dllPath">Path to the new game assembly DLL.</param>
    /// <returns><c>true</c> if the new assembly was loaded successfully.</returns>
    public bool ReloadAssembly(string dllPath)
    {
        Unload();
        return LoadAssembly(dllPath);
    }
}
