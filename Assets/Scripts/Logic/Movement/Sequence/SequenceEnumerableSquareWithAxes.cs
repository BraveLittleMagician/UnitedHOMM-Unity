#nullable enable

public abstract class SequenceEnumerableSquareWithAxes<TAxes> : SequenceEnumerable<Square> where TAxes : struct, IAxes
{
    protected SequenceEnumerableSquareWithAxes(Square startSquare, TAxes axes) : base(startSquare)
    {
        Axes = axes;
    }

    public TAxes Axes { get; }
}