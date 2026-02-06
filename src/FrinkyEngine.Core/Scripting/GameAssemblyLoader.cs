using System.Reflection;
using System.Runtime.Loader;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Scripting;

public class GameAssemblyLoader
{
    private AssemblyLoadContext? _loadContext;
    private Assembly? _gameAssembly;

    public Assembly? GameAssembly => _gameAssembly;

    public bool LoadAssembly(string dllPath)
    {
        if (!File.Exists(dllPath))
            return false;

        try
        {
            _loadContext = new AssemblyLoadContext("GameAssembly", isCollectible: true);
            using var fs = File.OpenRead(dllPath);
            _gameAssembly = _loadContext.LoadFromStream(fs);
            ComponentTypeResolver.RegisterAssembly(_gameAssembly);
            return true;
        }
        catch (Exception ex)
        {
            Rendering.FrinkyLog.Error($"Failed to load game assembly: {ex.Message}");
            return false;
        }
    }

    public void Unload()
    {
        _gameAssembly = null;
        _loadContext?.Unload();
        _loadContext = null;
    }
}
