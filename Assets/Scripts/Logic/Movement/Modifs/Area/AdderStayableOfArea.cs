#nullable enable

public class AdderStayableOfArea<TSquare> : ModificatorOfArea<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public int Index { get; }
    public AdderStayableOfArea(int index) => Index = index;
    public override RelativeArea<TSquare> Apply(RelativeArea<TSquare> sequence) => sequence.WithAddedStayable(Index);
}