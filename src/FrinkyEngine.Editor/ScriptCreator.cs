using System.Text.RegularExpressions;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Editor;

public static class ScriptCreator
{
    private static readonly Regex ValidIdentifier = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public static bool IsValidClassName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && ValidIdentifier.IsMatch(name);
    }

    public static string GenerateScript(string className, string namespaceName, Type baseClass)
    {
        var baseClassName = baseClass.Name;
        var usings = new List<string>
        {
            "using System.Numerics;",
            "using FrinkyEngine.Core.ECS;"
        };

        // Add the base class namespace if it's not Component (already in ECS namespace)
        if (baseClass != typeof(Component) && baseClass.Namespace != null
            && baseClass.Namespace != "FrinkyEngine.Core.ECS")
        {
            usings.Add($"using {baseClass.Namespace};");
        }

        usings.Sort();
        var usingBlock = string.Join("\n", usings);

        return $$"""
            {{usingBlock}}

            namespace {{namespaceName}}.Scripts;

            public class {{className}} : {{baseClassName}}
            {
                public override void Start()
                {
                    base.Start();
                }

                public override void Update(float dt)
                {
                    base.Update(dt);
                }
            }
            """;
    }
}
