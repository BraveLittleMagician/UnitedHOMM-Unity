#nullable enable

public abstract class ModificatorOfPattern<TSquare> : ModifierOfMovement<Pattern<TSquare>, Square> where TSquare : struct, ISquare<TSquare> { }