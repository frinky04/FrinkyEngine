using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using ImGuiNET;

namespace FrinkyEngine.Editor.Panels;

public class MenuBar
{
    private readonly EditorApplication _app;
    private string _sceneSavePath = string.Empty;
    private string _newProjectName = string.Empty;
    private string _newProjectParentDir = string.Empty;
    private string _projectPath = string.Empty;

    private bool _openSaveSceneAs;
    private bool _openNewProject;
    private bool _openOpenProject;

    public MenuBar(EditorApplication app)
    {
        _app = app;
    }

    public void Draw()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Scene"))
                {
                    _app.NewScene();
                }

                if (ImGui.MenuItem("Open Scene..."))
                {
                    FrinkyLog.Info("Use File > Open Project to load a .fproject file.");
                }

                if (ImGui.MenuItem("Save Scene"))
                {
                    if (_app.CurrentScene != null)
                    {
                        var path = !string.IsNullOrEmpty(_app.CurrentScene.FilePath)
                            ? _app.CurrentScene.FilePath
                            : "scene.fscene";
                        SceneManager.Instance.SaveScene(path);
                        FrinkyLog.Info($"Scene saved to: {path}");
                    }
                }

                if (ImGui.MenuItem("Save Scene As..."))
                    _openSaveSceneAs = true;

                ImGui.Separator();

                if (ImGui.MenuItem("New Project..."))
                    _openNewProject = true;

                if (ImGui.MenuItem("Open Project..."))
                    _openOpenProject = true;

                ImGui.Separator();

                if (ImGui.MenuItem("Exit"))
                {
                    Raylib_cs.Raylib.CloseWindow();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Undo", "Ctrl+Z", false, false);
                ImGui.MenuItem("Redo", "Ctrl+Y", false, false);
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                ImGui.MenuItem("Viewport", null, true);
                ImGui.MenuItem("Hierarchy", null, true);
                ImGui.MenuItem("Inspector", null, true);
                ImGui.MenuItem("Console", null, true);
                ImGui.Separator();
                if (ImGui.MenuItem("Reset Layout"))
                {
                    _app.ShouldResetLayout = true;
                }
                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (_app.Mode == EditorMode.Edit)
            {
                if (ImGui.MenuItem("Play"))
                    _app.EnterPlayMode();
            }
            else
            {
                if (ImGui.MenuItem("Stop"))
                    _app.ExitPlayMode();
            }

            ImGui.EndMainMenuBar();
        }

        // Open popups at this scope level (outside the menu) so BeginPopup can find them
        if (_openSaveSceneAs)
        {
            ImGui.OpenPopup("SaveSceneAs");
            _openSaveSceneAs = false;
        }
        if (_openNewProject)
        {
            ImGui.OpenPopup("NewProject");
            _openNewProject = false;
        }
        if (_openOpenProject)
        {
            ImGui.OpenPopup("OpenProject");
            _openOpenProject = false;
        }

        DrawSaveSceneAsPopup();
        DrawNewProjectPopup();
        DrawOpenProjectPopup();
    }

    private void DrawSaveSceneAsPopup()
    {
        if (ImGui.BeginPopup("SaveSceneAs"))
        {
            ImGui.InputText("Path", ref _sceneSavePath, 512);
            if (ImGui.Button("Save") && !string.IsNullOrWhiteSpace(_sceneSavePath))
            {
                SceneManager.Instance.SaveScene(_sceneSavePath);
                FrinkyLog.Info($"Scene saved to: {_sceneSavePath}");
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    private void DrawNewProjectPopup()
    {
        if (ImGui.BeginPopup("NewProject"))
        {
            ImGui.InputText("Project Name", ref _newProjectName, 256);
            ImGui.InputText("Parent Directory", ref _newProjectParentDir, 512);

            if (!string.IsNullOrWhiteSpace(_newProjectName) && !string.IsNullOrWhiteSpace(_newProjectParentDir))
            {
                var targetPath = Path.Combine(_newProjectParentDir, _newProjectName);
                ImGui.TextDisabled($"Target: {targetPath}");

                if (Directory.Exists(targetPath))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0.3f, 0.3f, 1));
                    ImGui.TextWrapped("Target directory already exists!");
                    ImGui.PopStyleColor();
                }
            }

            var parentExists = !string.IsNullOrWhiteSpace(_newProjectParentDir) && Directory.Exists(_newProjectParentDir);
            var nameValid = !string.IsNullOrWhiteSpace(_newProjectName);
            var targetExists = nameValid && parentExists && Directory.Exists(Path.Combine(_newProjectParentDir, _newProjectName));

            ImGui.BeginDisabled(!nameValid || !parentExists || targetExists);
            if (ImGui.Button("Create"))
            {
                _app.CreateAndOpenProject(_newProjectParentDir, _newProjectName);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    private void DrawOpenProjectPopup()
    {
        if (ImGui.BeginPopup("OpenProject"))
        {
            ImGui.InputText(".fproject Path", ref _projectPath, 512);
            if (ImGui.Button("Open") && !string.IsNullOrWhiteSpace(_projectPath))
            {
                _app.OpenProject(_projectPath);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }
}
