using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Serialization;
using Hexa.NET.ImGui;

namespace FrinkyEngine.Editor.Assets.Creation;

public sealed class ScriptAssetCreationFactory : IAssetCreationFactory
{
    private Type _selectedBaseClassType = typeof(Component);
    private string _baseClassSearch = string.Empty;

    public string Id => "script";
    public string DisplayName => "Script";
    public string NameHint => "Script Name";
    public string Extension => ".cs";
    public AssetType AssetType => AssetType.Script;

    public void Reset(EditorApplication app)
    {
        _selectedBaseClassType = typeof(Component);
        _baseClassSearch = string.Empty;
    }

    public void DrawOptions(EditorApplication app)
    {
        var selectedDisplayName = GetBaseClassDisplayName(_selectedBaseClassType);
        ImGui.TextUnformatted($"Base Class: {selectedDisplayName}");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##baseClassSearch", "Search...", ref _baseClassSearch, 256);
        ImGui.Separator();

        var componentTypes = ComponentTypeResolver.GetAllComponentTypes()
            .Where(t => t != typeof(Core.Components.TransformComponent) && !t.IsAbstract)
            .ToList();
        var fobjectTypes = FObjectTypeResolver.GetAllTypes()
            .Where(t => !t.IsAbstract)
            .ToList();

        var isSearching = !string.IsNullOrWhiteSpace(_baseClassSearch);
        var viewport = ImGui.GetMainViewport();
        float maxHeight = viewport.Size.Y * 0.45f;
        ImGui.BeginChild("##base_class_list", new Vector2(0, maxHeight), ImGuiChildFlags.None, ImGuiWindowFlags.None);

        if (isSearching)
        {
            DrawSearchResults(componentTypes, fobjectTypes, _baseClassSearch.Trim());
        }
        else
        {
            DrawBrowseTree(componentTypes, fobjectTypes);
        }

        ImGui.EndChild();
    }

    public bool TryValidateName(string name, out string? validationMessage)
    {
        if (!ScriptCreator.IsValidClassName(name))
        {
            validationMessage = "Invalid C# class name.";
            return false;
        }

        validationMessage = null;
        return true;
    }

    public string BuildRelativePath(string name)
    {
        return $"{name}{Extension}";
    }

    public bool TryCreate(EditorApplication app, string name, out string createdRelativePath, out string? errorMessage)
    {
        createdRelativePath = BuildRelativePath(name);
        errorMessage = null;

        var assetsPath = AssetManager.Instance.AssetsPath;
        if (string.IsNullOrWhiteSpace(assetsPath))
        {
            errorMessage = "Open a project first.";
            return false;
        }

        var fullPath = Path.Combine(assetsPath, createdRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            errorMessage = "File already exists.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(assetsPath);
            var namespaceName = app.ProjectFile?.ProjectName ?? "Game";
            var content = ScriptCreator.GenerateScript(name, namespaceName, _selectedBaseClassType);
            File.WriteAllText(fullPath, content);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create script: {ex.Message}";
            return false;
        }
    }

    private void DrawSearchResults(List<Type> componentTypes, List<Type> fobjectTypes, string search)
    {
        foreach (var type in componentTypes)
        {
            var displayName = ComponentTypeResolver.GetDisplayName(type);
            if (!displayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !type.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var source = ComponentTypeResolver.GetAssemblySource(type);
            var isSelected = _selectedBaseClassType == type;
            if (ImGui.Selectable($"{displayName}  [{source}]", isSelected))
                _selectedBaseClassType = type;
            DrawBaseClassTooltip(type, typeof(Component));
        }

        foreach (var type in fobjectTypes)
        {
            var displayName = FObjectTypeResolver.GetDisplayName(type);
            if (!displayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !type.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var source = FObjectTypeResolver.GetAssemblySource(type);
            var isSelected = _selectedBaseClassType == type;
            if (ImGui.Selectable($"{displayName}  [{source}]", isSelected))
                _selectedBaseClassType = type;
            DrawBaseClassTooltip(type, typeof(FObject));
        }

        if ("Component".Contains(search, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.Selectable("Component (base)", _selectedBaseClassType == typeof(Component)))
                _selectedBaseClassType = typeof(Component);
        }

        if ("Data Object".Contains(search, StringComparison.OrdinalIgnoreCase)
            || "FObject".Contains(search, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.Selectable("Data Object (base)", _selectedBaseClassType == typeof(FObject)))
                _selectedBaseClassType = typeof(FObject);
        }
    }

    private void DrawBrowseTree(List<Type> componentTypes, List<Type> fobjectTypes)
    {
        var engineComponents = componentTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) == "Engine").ToList();
        var gameComponents = componentTypes.Where(t => ComponentTypeResolver.GetAssemblySource(t) != "Engine").ToList();
        var engineFObjects = fobjectTypes.Where(t => FObjectTypeResolver.GetAssemblySource(t) == "Engine").ToList();
        var gameFObjects = fobjectTypes.Where(t => FObjectTypeResolver.GetAssemblySource(t) != "Engine").ToList();

        if (ImGui.CollapsingHeader("Components", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.PushID("ComponentBase");
            if (ImGui.Selectable("  Component (base)", _selectedBaseClassType == typeof(Component)))
                _selectedBaseClassType = typeof(Component);
            ImGui.PopID();

            if (engineComponents.Count > 0 && ImGui.TreeNode("Engine##comp"))
            {
                DrawCategoryNode(BuildCategoryTree(engineComponents, true), true);
                ImGui.TreePop();
            }

            if (gameComponents.Count > 0 && ImGui.TreeNode("Game##comp"))
            {
                DrawCategoryNode(BuildCategoryTree(gameComponents, true), true);
                ImGui.TreePop();
            }
        }

        if (ImGui.CollapsingHeader("Data Objects", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.PushID("FObjectBase");
            if (ImGui.Selectable("  Data Object (base)", _selectedBaseClassType == typeof(FObject)))
                _selectedBaseClassType = typeof(FObject);
            ImGui.PopID();

            if (engineFObjects.Count > 0 && ImGui.TreeNode("Engine##fobj"))
            {
                DrawCategoryNode(BuildCategoryTree(engineFObjects, false), false);
                ImGui.TreePop();
            }

            if (gameFObjects.Count > 0 && ImGui.TreeNode("Game##fobj"))
            {
                DrawCategoryNode(BuildCategoryTree(gameFObjects, false), false);
                ImGui.TreePop();
            }
        }
    }

    private static string GetBaseClassDisplayName(Type type)
    {
        if (type == typeof(Component))
            return "Component";
        if (type == typeof(FObject))
            return "Data Object";
        if (type.IsSubclassOf(typeof(Component)))
            return ComponentTypeResolver.GetDisplayName(type);
        if (type.IsSubclassOf(typeof(FObject)))
            return FObjectTypeResolver.GetDisplayName(type);
        return type.Name;
    }

    private sealed class ScriptCategoryNode
    {
        public Dictionary<string, ScriptCategoryNode> Children { get; } = new();
        public List<Type> Types { get; } = new();
    }

    private static ScriptCategoryNode BuildCategoryTree(List<Type> types, bool isComponent)
    {
        var root = new ScriptCategoryNode();
        foreach (var type in types)
        {
            var category = isComponent ? ComponentTypeResolver.GetCategory(type) : null;
            var target = root;
            if (!string.IsNullOrEmpty(category))
            {
                var segments = category.Split('/');
                foreach (var segment in segments)
                {
                    if (!target.Children.TryGetValue(segment, out var child))
                    {
                        child = new ScriptCategoryNode();
                        target.Children[segment] = child;
                    }

                    target = child;
                }
            }

            target.Types.Add(type);
        }

        return root;
    }

    private void DrawCategoryNode(ScriptCategoryNode node, bool isComponent)
    {
        foreach (var type in node.Types)
        {
            var displayName = isComponent
                ? ComponentTypeResolver.GetDisplayName(type)
                : FObjectTypeResolver.GetDisplayName(type);
            var isSelected = _selectedBaseClassType == type;
            if (ImGui.Selectable($"  {displayName}", isSelected))
                _selectedBaseClassType = type;
            DrawBaseClassTooltip(type, isComponent ? typeof(Component) : typeof(FObject));
        }

        foreach (var (categoryName, child) in node.Children.OrderBy(kv => kv.Key))
        {
            if (ImGui.TreeNode(categoryName))
            {
                DrawCategoryNode(child, isComponent);
                ImGui.TreePop();
            }
        }
    }

    private static void DrawBaseClassTooltip(Type type, Type directBase)
    {
        if (ImGui.IsItemHovered() && type.BaseType != null && type.BaseType != directBase)
            ImGui.SetTooltip($"Extends {type.BaseType.Name}");
    }
}
