using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class SpherePrimitive : PrimitiveComponent
{
    private float _radius = 0.5f;
    private int _rings = 16;
    private int _slices = 16;

    public float Radius
    {
        get => _radius;
        set { if (_radius != value) { _radius = value; MarkMeshDirty(); } }
    }

    public int Rings
    {
        get => _rings;
        set { if (_rings != value) { _rings = value; MarkMeshDirty(); } }
    }

    public int Slices
    {
        get => _slices;
        set { if (_slices != value) { _slices = value; MarkMeshDirty(); } }
    }

    protected override Mesh CreateMesh() => Raylib.GenMeshSphere(_radius, _rings, _slices);
}
