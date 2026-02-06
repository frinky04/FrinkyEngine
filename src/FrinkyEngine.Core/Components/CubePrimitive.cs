using Raylib_cs;

namespace FrinkyEngine.Core.Components;

public class CubePrimitive : PrimitiveComponent
{
    private float _width = 1.0f;
    private float _height = 1.0f;
    private float _depth = 1.0f;

    public float Width
    {
        get => _width;
        set { if (_width != value) { _width = value; MarkMeshDirty(); } }
    }

    public float Height
    {
        get => _height;
        set { if (_height != value) { _height = value; MarkMeshDirty(); } }
    }

    public float Depth
    {
        get => _depth;
        set { if (_depth != value) { _depth = value; MarkMeshDirty(); } }
    }

    protected override Mesh CreateMesh() => Raylib.GenMeshCube(_width, _height, _depth);
}
