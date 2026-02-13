using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Core.Animation.IK;

/// <summary>
/// Caches bone hierarchy data from a Raylib <see cref="Model"/> for efficient IK lookups.
/// </summary>
public sealed class BoneHierarchy
{
    /// <summary>Number of bones in the skeleton.</summary>
    public int BoneCount { get; }

    /// <summary>Bone names indexed by bone index.</summary>
    public string[] BoneNames { get; }

    /// <summary>Parent index for each bone (-1 for root bones).</summary>
    public int[] ParentIndices { get; }

    /// <summary>
    /// Bind-pose transforms as stored in the Raylib model (typically model-space for supported formats).
    /// </summary>
    public (Vector3 translation, Quaternion rotation, Vector3 scale)[] BindPoseLocal { get; }

    /// <summary>
    /// Creates a <see cref="BoneHierarchy"/> by reading bone data from a Raylib model.
    /// </summary>
    public unsafe BoneHierarchy(Model model)
    {
        BoneCount = model.BoneCount;
        BoneNames = new string[BoneCount];
        ParentIndices = new int[BoneCount];
        BindPoseLocal = new (Vector3, Quaternion, Vector3)[BoneCount];

        for (int i = 0; i < BoneCount; i++)
        {
            var bone = model.Bones[i];
            BoneNames[i] = new string(bone.Name, 0, 32).TrimEnd('\0');
            ParentIndices[i] = bone.Parent;

            var t = model.BindPose[i];
            BindPoseLocal[i] = (t.Translation, t.Rotation, t.Scale);
        }
    }

    /// <summary>
    /// Finds a bone by name. Returns -1 if not found.
    /// </summary>
    public int FindBone(string name)
    {
        for (int i = 0; i < BoneCount; i++)
        {
            if (string.Equals(BoneNames[i], name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Walks parent pointers from <paramref name="endBone"/> up to <paramref name="rootBone"/>,
    /// returning an ordered chain from root to end (inclusive). Returns null if no path exists.
    /// </summary>
    public int[]? GetChain(int rootBone, int endBone)
    {
        if (rootBone < 0 || rootBone >= BoneCount || endBone < 0 || endBone >= BoneCount)
            return null;

        var chain = new List<int>();
        int current = endBone;
        while (current >= 0 && current < BoneCount)
        {
            chain.Add(current);
            if (current == rootBone)
            {
                chain.Reverse();
                return chain.ToArray();
            }
            current = ParentIndices[current];
        }

        return null; // rootBone is not an ancestor of endBone
    }

    /// <summary>
    /// Returns bone names for use in a dropdown, prefixed with "(none)" at index 0.
    /// Bone index = dropdown index - 1.
    /// </summary>
    public string[] GetBoneNamesForDropdown()
    {
        var names = new string[BoneCount + 1];
        names[0] = "(none)";
        for (int i = 0; i < BoneCount; i++)
            names[i + 1] = string.IsNullOrWhiteSpace(BoneNames[i]) ? $"Bone {i}" : BoneNames[i];
        return names;
    }
}
