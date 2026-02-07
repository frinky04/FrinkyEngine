using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace FrinkyEngine.Editor;

/// <summary>
/// P/Invoke bindings for cimgui DockBuilder functions not exposed by ImGui.NET.
/// </summary>
public static unsafe class ImGuiDockBuilder
{
    private const string LibName = "cimgui";

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint igDockBuilderAddNode(uint node_id, ImGuiDockNodeFlags flags);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderRemoveNode(uint node_id);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderSetNodeSize(uint node_id, Vector2 size);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint igDockBuilderSplitNode(
        uint node_id, ImGuiDir split_dir, float size_ratio_for_node_at_dir,
        uint* out_id_at_dir, uint* out_id_at_opposite_dir);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderDockWindow(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string window_name, uint node_id);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderFinish(uint node_id);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr igDockBuilderGetNode(uint node_id);

    public static uint AddNode(uint nodeId, ImGuiDockNodeFlags flags = ImGuiDockNodeFlags.None)
        => igDockBuilderAddNode(nodeId, flags);

    public static void RemoveNode(uint nodeId)
        => igDockBuilderRemoveNode(nodeId);

    public static void SetNodeSize(uint nodeId, Vector2 size)
        => igDockBuilderSetNodeSize(nodeId, size);

    public static uint SplitNode(uint nodeId, ImGuiDir dir, float sizeRatio,
        out uint outIdAtDir, out uint outIdAtOpposite)
    {
        uint atDir, atOpposite;
        var result = igDockBuilderSplitNode(nodeId, dir, sizeRatio, &atDir, &atOpposite);
        outIdAtDir = atDir;
        outIdAtOpposite = atOpposite;
        return result;
    }

    public static void DockWindow(string windowName, uint nodeId)
        => igDockBuilderDockWindow(windowName, nodeId);

    public static void Finish(uint nodeId)
        => igDockBuilderFinish(nodeId);

    public static void SetNodeLocalFlags(uint nodeId, ImGuiDockNodeFlags flags)
    {
        var node = igDockBuilderGetNode(nodeId);
        if (node == IntPtr.Zero) return;
        // ImGuiDockNode layout: ID(4) + SharedFlags(4) + LocalFlags(4)
        var localFlagsPtr = (int*)((byte*)node + 8);
        *localFlagsPtr |= (int)flags;
    }
}
