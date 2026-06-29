#nullable enable

public abstract class ModificatorOfArea<TAxes> : ModifierOfMovement<RelativeArea<TAxes>, Square> where TAxes : struct, IAxes { }