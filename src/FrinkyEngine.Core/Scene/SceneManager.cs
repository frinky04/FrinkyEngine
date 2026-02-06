using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Scene;

public class SceneManager
{
    public static SceneManager Instance { get; } = new();

    public Scene? ActiveScene { get; private set; }

    public Scene NewScene(string name = "Untitled")
    {
        var scene = new Scene { Name = name };
        ActiveScene = scene;
        return scene;
    }

    public void SetActiveScene(Scene scene)
    {
        ActiveScene = scene;
    }

    public void SaveScene(string path)
    {
        if (ActiveScene == null) return;
        ActiveScene.FilePath = path;
        SceneSerializer.Save(ActiveScene, path);
    }

    public Scene? LoadScene(string path)
    {
        var scene = SceneSerializer.Load(path);
        if (scene != null)
        {
            scene.FilePath = path;
            ActiveScene = scene;
        }
        return scene;
    }
}
