#nullable enable
public abstract class ModificatorOfArea<TSquare> : ModifierOfMovement<RelativeArea<TSquare>, TSquare>, IPureShapeModifier where TSquare : struct, ISquare<TSquare> { }