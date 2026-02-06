using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class CylinderPrimitive : PrimitiveComponent
{
    private float _radius = 0.5f;
    private float _height = 2.0f;
    private int _slices = 16;

    public float Radius
    {
        get => _radius;
        set { if (_radius != value) { _radius = value; MarkMeshDirty(); } }
    }

    public float Height
    {
        get => _height;
        set { if (_height != value) { _height = value; MarkMeshDirty(); } }
    }

    public int Slices
    {
        get => _slices;
        set { if (_slices != value) { _slices = value; MarkMeshDirty(); } }
    }

    protected override Mesh CreateMesh() => Raylib.GenMeshCylinder(_radius, _height, _slices);
}
