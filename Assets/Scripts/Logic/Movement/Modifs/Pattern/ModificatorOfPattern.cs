#nullable enable

public abstract class ModificatorOfPattern<TSquare> : ModifierOfMovement<Pattern<TSquare>, TSquare> where TSquare : struct, ISquare<TSquare> { }