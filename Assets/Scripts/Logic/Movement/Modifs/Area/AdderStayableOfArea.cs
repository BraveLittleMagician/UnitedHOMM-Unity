#nullable enable

public class AdderStayableOfArea<TAxes> : ModificatorOfArea<TAxes> where TAxes : struct, IAxes
{
    public int Index { get; }
    public AdderStayableOfArea(int index) => Index = index;
    public override RelativeArea<TAxes> Apply(RelativeArea<TAxes> sequence) => sequence.WithAddedStayable(Index);
}