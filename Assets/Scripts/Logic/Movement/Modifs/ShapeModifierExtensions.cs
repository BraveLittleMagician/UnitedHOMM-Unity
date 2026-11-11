
#nullable enable


public static class ShapeModifierExtensions
{
    public static bool IsPureShape(IModifier modifier) => modifier is IPureShapeModifier;
}