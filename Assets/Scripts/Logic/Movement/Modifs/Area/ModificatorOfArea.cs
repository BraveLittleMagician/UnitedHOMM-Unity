#nullable enable

public abstract class ModificatorOfArea<TSquare> : ModifierOfMovement<RelativeArea<TSquare>, TSquare> where TSquare : struct, ISquare<TSquare> { }