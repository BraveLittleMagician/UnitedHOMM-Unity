#nullable enable

public class DecreaserRadiusOfArea<TAxes> : ModificatorOfArea<TAxes> where TAxes : struct, IAxes
{
    public override RelativeArea<TAxes> Apply(RelativeArea<TAxes> sequence)
    {
        var (stayables, length, isCircle, guarantees) = sequence.DataWithDecreasedRadius();
        return new RelativeArea<TAxes>(stayables, length, isCircle, false);
    }
}