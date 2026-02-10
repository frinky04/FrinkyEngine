namespace FrinkyEngine.Editor;

/// <summary>
/// Describes a project template that can be used to scaffold new game projects.
/// </summary>
public sealed class ProjectTemplate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int SortOrder { get; init; }
    public required string SourceName { get; init; }
    public required string ContentDirectory { get; init; }
}
