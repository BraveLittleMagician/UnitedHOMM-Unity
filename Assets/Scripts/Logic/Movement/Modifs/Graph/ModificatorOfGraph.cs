#nullable enable

public abstract class ModificatorOfGraph<TSquare> : ModifierOfMovement<RelativeGraph<TSquare>, Square> where TSquare : struct, ISquare<TSquare> { }