using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Scene;

/// <summary>
/// Singleton that owns the active <see cref="Scene"/> and provides load/save operations.
/// </summary>
public class SceneManager
{
    /// <summary>
    /// The global scene manager instance.
    /// </summary>
    public static SceneManager Instance { get; } = new();

    /// <summary>
    /// The currently loaded scene, or <c>null</c> if no scene is active.
    /// </summary>
    public Scene? ActiveScene { get; private set; }

    /// <summary>
    /// Creates a new empty scene and makes it the active scene.
    /// </summary>
    /// <param name="name">Display name for the new scene.</param>
    /// <returns>The newly created scene.</returns>
    public Scene NewScene(string name = "Untitled")
    {
        ActiveScene?.Dispose();
        var scene = new Scene { Name = name };
        ActiveScene = scene;
        return scene;
    }

    /// <summary>
    /// Sets the given scene as the active scene without saving or unloading the previous one.
    /// </summary>
    /// <param name="scene">The scene to activate.</param>
    public void SetActiveScene(Scene scene)
    {
        if (!ReferenceEquals(ActiveScene, scene))
            ActiveScene?.Dispose();
        ActiveScene = scene;
    }

    /// <summary>
    /// Saves the active scene to the specified file path in <c>.fscene</c> JSON format.
    /// </summary>
    /// <param name="path">Destination file path.</param>
    public void SaveScene(string path)
    {
        if (ActiveScene == null) return;
        ActiveScene.FilePath = path;
        SceneSerializer.Save(ActiveScene, path);
    }

    /// <summary>
    /// Loads a scene from the specified <c>.fscene</c> file and makes it the active scene.
    /// </summary>
    /// <param name="path">Path to the scene file.</param>
    /// <returns>The loaded scene, or <c>null</c> if loading failed.</returns>
    public Scene? LoadScene(string path)
    {
        var scene = SceneSerializer.Load(path);
        if (scene != null)
        {
            scene.FilePath = path;
            ActiveScene?.Dispose();
            ActiveScene = scene;
        }
        return scene;
    }
}
