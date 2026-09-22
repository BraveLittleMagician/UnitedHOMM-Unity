#nullable enable

public abstract class ModifierOfMovement { }

public abstract class ModifierOfMovement<TSequence, TPosition> : ModifierOfMovement where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct
{
    public abstract TSequence Apply(TSequence sequence);
}