#nullable enable

public abstract class ModifierOfMovement { }

public abstract class ModifierOfMovement<TSequence, TPosition> : ModifierOfMovement, IModifier, IShapeTransformModifier<TSequence, TPosition> where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct, ISquare<TPosition>
{
    public virtual ShapeTarget Target => ShapeTarget.Both;
    public virtual string ModifierName
    {
        get
        {
            var n = GetType().Name;
            int i = n.IndexOf('`');
            return i < 0 ? n : n.Substring(0, i);
        }
    }
    public virtual int MaxDuration => -1;
    public virtual int CurrentDuration => -1;
    public virtual int MaxCharges => -1;
    public virtual int CurrentCharges => -1;
    public virtual void Attach(ModifierContext context) { }
    public virtual void Detach() { }
    public virtual bool TryConsumeCharge() => true;
    public virtual void RestoreCharges(int charges) { }
    public virtual void RestoreDuration(int duration) { }
    public abstract TSequence Apply(TSequence sequence);
}