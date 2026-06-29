#nullable enable

public class RemoverStayableOfArea<TAxes> : ModificatorOfArea<TAxes> where TAxes : struct, IAxes
{
    public int Index { get; }
    public RemoverStayableOfArea(int index) => Index = index;
    public override RelativeArea<TAxes> Apply(RelativeArea<TAxes> sequence)
    {
        var (stayables, length, isCircle, _) = sequence.DataWithRemovedStayable(Index);
        return new RelativeArea<TAxes>(stayables, length, isCircle, false);
    }
}