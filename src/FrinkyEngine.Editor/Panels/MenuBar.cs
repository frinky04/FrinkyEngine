using System.Numerics;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Scene;
using FrinkyEngine.Core.Serialization;
using ImGuiNET;
using NativeFileDialogSharp;

namespace FrinkyEngine.Editor.Panels;

public class MenuBar
{
    private readonly EditorApplication _app;
    private string _newProjectName = string.Empty;
    private string _newProjectParentDir = string.Empty;

    private bool _openNewProject;
    private bool _openCreateScript;

    // Create Script modal state
    private string _newScriptName = string.Empty;
    private int _selectedBaseClassIndex;
    private string[] _baseClassOptions = Array.Empty<string>();
    private Type[] _baseClassTypes = Array.Empty<Type>();

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
                if (ImGui.MenuItem("New Scene", KeybindManager.Instance.GetShortcutText(EditorAction.NewScene)))
                {
                    _app.NewScene();
                }

                if (ImGui.MenuItem("Open Scene...", KeybindManager.Instance.GetShortcutText(EditorAction.OpenScene)))
                {
                    OpenSceneDialog();
                }

                if (ImGui.MenuItem("Save Scene", KeybindManager.Instance.GetShortcutText(EditorAction.SaveScene)))
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

                if (ImGui.MenuItem("Save Scene As...", KeybindManager.Instance.GetShortcutText(EditorAction.SaveSceneAs)))
                {
                    SaveSceneAs();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("New Project...", KeybindManager.Instance.GetShortcutText(EditorAction.NewProject)))
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
                ImGui.MenuItem("Undo", KeybindManager.Instance.GetShortcutText(EditorAction.Undo), false, false);
                ImGui.MenuItem("Redo", KeybindManager.Instance.GetShortcutText(EditorAction.Redo), false, false);

                ImGui.Separator();

                var hasSelection = _app.SelectedEntity != null;

                ImGui.BeginDisabled(!hasSelection);
                if (ImGui.MenuItem("Delete", KeybindManager.Instance.GetShortcutText(EditorAction.DeleteEntity)))
                {
                    if (_app.SelectedEntity != null && _app.CurrentScene != null)
                    {
                        _app.CurrentScene.RemoveEntity(_app.SelectedEntity);
                        _app.SelectedEntity = null;
                    }
                }

                if (ImGui.MenuItem("Duplicate", KeybindManager.Instance.GetShortcutText(EditorAction.DuplicateEntity)))
                {
                    FrinkyLog.Info("Duplicate entity not yet implemented.");
                }

                if (ImGui.MenuItem("Rename", KeybindManager.Instance.GetShortcutText(EditorAction.RenameEntity)))
                {
                    if (_app.SelectedEntity != null)
                        _app.InspectorPanel.FocusNameField = true;
                }
                ImGui.EndDisabled();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Scripts"))
            {
                var hasProject = _app.ProjectDirectory != null;
                var isBuilding = ScriptBuilder.IsBuilding;

                ImGui.BeginDisabled(!hasProject || isBuilding);
                if (ImGui.MenuItem("Build Scripts", KeybindManager.Instance.GetShortcutText(EditorAction.BuildScripts)))
                {
                    _app.BuildScripts();
                }
                ImGui.EndDisabled();

                if (isBuilding)
                {
                    ImGui.TextDisabled("Building...");
                }

                ImGui.Separator();

                ImGui.BeginDisabled(!hasProject);
                if (ImGui.MenuItem("Create Script..."))
                {
                    _openCreateScript = true;
                    _newScriptName = string.Empty;
                    _selectedBaseClassIndex = 0;
                    RefreshBaseClassOptions();
                }
                ImGui.EndDisabled();

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

            var shortcut = KeybindManager.Instance.GetShortcutText(EditorAction.PlayStop);
            if (_app.Mode == EditorMode.Edit)
            {
                if (ImGui.MenuItem("Play", shortcut))
                    _app.EnterPlayMode();
            }
            else
            {
                if (ImGui.MenuItem("Stop", shortcut))
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

        if (_openCreateScript)
        {
            ImGui.OpenPopup("CreateScript");
            _openCreateScript = false;
        }

        DrawNewProjectPopup();
        DrawCreateScriptPopup();
    }

    private void RefreshBaseClassOptions()
    {
        var types = new List<Type> { typeof(Component) };
        foreach (var type in ComponentTypeResolver.GetAllComponentTypes())
        {
            if (type != typeof(Core.Components.TransformComponent) && !type.IsAbstract)
                types.Add(type);
        }

        _baseClassTypes = types.ToArray();
        _baseClassOptions = types.Select(t => t.Name).ToArray();
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

    public void TriggerOpenScene() => OpenSceneDialog();
    public void TriggerSaveSceneAs() => SaveSceneAs();
    public void TriggerNewProject() => _openNewProject = true;

    private void DrawCreateScriptPopup()
    {
        var viewport = ImGui.GetMainViewport();
        var center = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f,
                                 viewport.WorkPos.Y + viewport.WorkSize.Y * 0.5f);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(450, 0), ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal("CreateScript", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Script Name", ref _newScriptName, 256);

            if (_baseClassOptions.Length > 0)
            {
                ImGui.Combo("Base Class", ref _selectedBaseClassIndex, _baseClassOptions, _baseClassOptions.Length);
            }

            // File path preview
            var nameValid = ScriptCreator.IsValidClassName(_newScriptName);
            if (nameValid && _app.ProjectDirectory != null)
            {
                var filePath = Path.Combine(_app.ProjectDirectory, "Assets", "Scripts", $"{_newScriptName}.cs");
                var relativePath = Path.GetRelativePath(_app.ProjectDirectory, filePath);
                ImGui.TextDisabled($"File: {relativePath}");

                if (File.Exists(filePath))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.3f, 0.3f, 1));
                    ImGui.TextWrapped("A script with this name already exists!");
                    ImGui.PopStyleColor();
                }
            }
            else if (!string.IsNullOrEmpty(_newScriptName) && !nameValid)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.3f, 0.3f, 1));
                ImGui.TextWrapped("Invalid C# class name.");
                ImGui.PopStyleColor();
            }

            var canCreate = nameValid
                && _app.ProjectDirectory != null
                && !File.Exists(Path.Combine(_app.ProjectDirectory, "Assets", "Scripts", $"{_newScriptName}.cs"));

            ImGui.BeginDisabled(!canCreate);
            if (ImGui.Button("Create"))
            {
                var baseType = _selectedBaseClassIndex < _baseClassTypes.Length
                    ? _baseClassTypes[_selectedBaseClassIndex]
                    : typeof(Component);

                var namespaceName = _app.ProjectFile?.ProjectName ?? "Game";
                var scriptsDir = Path.Combine(_app.ProjectDirectory!, "Assets", "Scripts");
                Directory.CreateDirectory(scriptsDir);

                var filePath = Path.Combine(scriptsDir, $"{_newScriptName}.cs");
                var content = ScriptCreator.GenerateScript(_newScriptName, namespaceName, baseType);
                File.WriteAllText(filePath, content);
                var logPath = Path.GetRelativePath(_app.ProjectDirectory!, filePath);
                FrinkyLog.Info($"Created script: {logPath}");
                NotificationManager.Instance.Post($"Created script: {_newScriptName}.cs", NotificationType.Success);

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
