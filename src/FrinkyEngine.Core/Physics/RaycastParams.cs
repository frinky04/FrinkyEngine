using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Physics;

/// <summary>
/// Options for filtering raycast results.
/// </summary>
public struct RaycastParams
{
    /// <summary>
    /// When <c>true</c>, trigger colliders are included in the test.
    /// </summary>
    public bool IncludeTriggers { get; set; }

    /// <summary>
    /// Set of entities whose colliders should be skipped by the raycast.
    /// </summary>
    public HashSet<Entity>? IgnoredEntities { get; set; }

    /// <summary>
    /// Populates <see cref="IgnoredEntities"/> with the full hierarchy tree
    /// (root and all descendants) of the given entity. Walks up to the root
    /// parent, then collects the entire subtree.
    /// </summary>
    /// <param name="entity">Any entity in the tree to ignore.</param>
    public void IgnoreEntityTree(Entity entity)
    {
        IgnoredEntities ??= new HashSet<Entity>();

        // Walk up to the root
        var root = entity.Transform;
        while (root.Parent != null)
            root = root.Parent;

        // Collect the entire subtree
        CollectTree(root, IgnoredEntities);
    }

    private static void CollectTree(Components.TransformComponent transform, HashSet<Entity> set)
    {
        set.Add(transform.Entity);
        foreach (var child in transform.Children)
            CollectTree(child, set);
    }
}
