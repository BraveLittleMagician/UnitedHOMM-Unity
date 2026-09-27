#nullable enable

public abstract class ModificatorOfGraph<TSquare> : ModifierOfMovement<RelativeGraph<TSquare>, TSquare> where TSquare : struct, ISquare<TSquare> { }