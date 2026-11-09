#nullable enable

public abstract class ModificatorOfPattern<TSquare> : ModifierOfMovement<Pattern<TSquare>, TSquare>, IPureShapeModifier where TSquare : struct, ISquare<TSquare>
{ }