#nullable enable

public class RemoverStayableOfArea<TSquare> : ModificatorOfArea<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public int Index { get; }
    public RemoverStayableOfArea(int index) => Index = index;
    public override RelativeArea<TSquare> Apply(RelativeArea<TSquare> sequence)
    {
        var (activeAxes, stayables, length, isCircle, _) = sequence.DataWithRemovedStayable(Index);
        return new RelativeArea<TSquare>(activeAxes, stayables, length, isCircle, false);
    }
}