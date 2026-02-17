using Facebook.Yoga;

namespace FrinkyEngine.Core.CanvasUI.Styles;

public enum LengthUnit
{
    Pixels,
    Percent,
    Auto,
}

public readonly struct Length
{
    public float Value { get; }
    public LengthUnit Unit { get; }

    private Length(float value, LengthUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Length Px(float value) => new(value, LengthUnit.Pixels);
    public static Length Pct(float value) => new(value, LengthUnit.Percent);
    public static readonly Length Auto = new(0, LengthUnit.Auto);

    public static implicit operator Length(float pixels) => Px(pixels);
    public static implicit operator Length(int pixels) => Px(pixels);

    internal YogaValue ToYoga()
    {
        return Unit switch
        {
            LengthUnit.Pixels => YogaValue.Point(Value),
            LengthUnit.Percent => YogaValue.Percent(Value),
            LengthUnit.Auto => YogaValue.Auto(),
            _ => YogaValue.Auto(),
        };
    }

    /// <summary>
    /// Converts to YogaValue, using Undefined instead of Auto.
    /// Use for min/max dimensions where Auto is not a valid Yoga value.
    /// </summary>
    internal YogaValue ToYogaMinMax()
    {
        return Unit switch
        {
            LengthUnit.Pixels => YogaValue.Point(Value),
            LengthUnit.Percent => YogaValue.Percent(Value),
            _ => YogaValue.Undefined(),
        };
    }
}
