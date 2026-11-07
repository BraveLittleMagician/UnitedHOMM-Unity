#nullable enable

public interface IShapeTransformModifier
{
    ShapeTarget Target { get; }
}
public interface IShapeTransformModifier<TSequence, TPosition> : IShapeTransformModifier where TPosition : struct, ISquare<TPosition> where TSequence : notnull, ISequence<TPosition, TSequence>
{
    TSequence Apply(TSequence sequence);
}