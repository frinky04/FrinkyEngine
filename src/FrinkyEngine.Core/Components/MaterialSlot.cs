using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Components;

public class MaterialSlot
{
    public MaterialType MaterialType { get; set; } = MaterialType.SolidColor;
    public string TexturePath { get; set; } = string.Empty;
}
