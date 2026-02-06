using System.Numerics;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using ImGuiNET;
using NativeFileDialogSharp;

namespace FrinkyEngine.Editor.Panels;

public class MenuBar
{
    private readonly EditorApplication _app;
    private string _newProjectName = string.Empty;
    private string _newProjectParentDir = string.Empty;

    private bool _openNewProject;

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
                    OpenSceneDialog();
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
                {
                    SaveSceneAs();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("New Project..."))
                    _openNewProject = true;

                if (ImGui.MenuItem("Open Project..."))
                {
                    OpenProjectDialog();
                }

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
                ImGui.MenuItem("Assets", null, true);
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
        if (_openNewProject)
        {
            ImGui.OpenPopup("NewProject");
            _openNewProject = false;
        }

        DrawNewProjectPopup();
    }

    private void OpenSceneDialog()
    {
        string? defaultPath = null;
        if (_app.ProjectDirectory != null && _app.ProjectFile != null)
        {
            var assetsDir = _app.ProjectFile.GetAbsoluteAssetsPath(_app.ProjectDirectory);
            var scenesDir = Path.Combine(assetsDir, "Scenes");
            if (Directory.Exists(scenesDir))
                defaultPath = scenesDir;
            else if (Directory.Exists(assetsDir))
                defaultPath = assetsDir;
        }

        var result = Dialog.FileOpen("fscene", defaultPath);
        if (!result.IsOk) return;

        SceneManager.Instance.LoadScene(result.Path);
        _app.CurrentScene = SceneManager.Instance.ActiveScene;
        _app.SelectedEntity = null;
        _app.UpdateWindowTitle();
        FrinkyLog.Info($"Opened scene: {result.Path}");
    }

    private void SaveSceneAs()
    {
        if (_app.CurrentScene == null) return;

        // Default to the project's assets directory if a project is open
        string? defaultPath = null;
        if (_app.ProjectDirectory != null && _app.ProjectFile != null)
        {
            var assetsDir = _app.ProjectFile.GetAbsoluteAssetsPath(_app.ProjectDirectory);
            var scenesDir = Path.Combine(assetsDir, "Scenes");
            if (Directory.Exists(scenesDir))
                defaultPath = scenesDir;
            else if (Directory.Exists(assetsDir))
                defaultPath = assetsDir;
        }

        var result = Dialog.FileSave("fscene", defaultPath);
        if (!result.IsOk) return;

        var path = result.Path;

        // Auto-append .fscene if missing
        if (!path.EndsWith(".fscene", StringComparison.OrdinalIgnoreCase))
            path += ".fscene";

        SceneManager.Instance.SaveScene(path);
        _app.UpdateWindowTitle();
        FrinkyLog.Info($"Scene saved to: {path}");
    }

    private void OpenProjectDialog()
    {
        var result = Dialog.FileOpen("fproject");
        if (!result.IsOk) return;

        _app.OpenProject(result.Path);
    }

    private void DrawNewProjectPopup()
    {
        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
                                 viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(500, 0), ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal("NewProject", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Project Name", ref _newProjectName, 256);

            ImGui.InputText("Parent Directory", ref _newProjectParentDir, 512);
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                var result = Dialog.FolderPicker(
                    string.IsNullOrWhiteSpace(_newProjectParentDir) ? null : _newProjectParentDir);
                if (result.IsOk)
                    _newProjectParentDir = result.Path;
            }

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
}
