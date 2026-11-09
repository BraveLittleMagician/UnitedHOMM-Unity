#nullable enable

public abstract class ModificatorOfGraph<TSquare> : ModifierOfMovement<RelativeGraph<TSquare>, TSquare>, IPureShapeModifier where TSquare : struct, ISquare<TSquare>
{ }