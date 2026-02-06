using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class PlanePrimitive : PrimitiveComponent
{
    private float _width = 10.0f;
    private float _depth = 10.0f;
    private int _resolutionX = 1;
    private int _resolutionZ = 1;

    public float Width
    {
        get => _width;
        set { if (_width != value) { _width = value; MarkMeshDirty(); } }
    }

    public float Depth
    {
        get => _depth;
        set { if (_depth != value) { _depth = value; MarkMeshDirty(); } }
    }

    public int ResolutionX
    {
        get => _resolutionX;
        set { if (_resolutionX != value) { _resolutionX = value; MarkMeshDirty(); } }
    }

    public int ResolutionZ
    {
        get => _resolutionZ;
        set { if (_resolutionZ != value) { _resolutionZ = value; MarkMeshDirty(); } }
    }

    protected override Mesh CreateMesh() => Raylib.GenMeshPlane(_width, _depth, _resolutionX, _resolutionZ);
}
