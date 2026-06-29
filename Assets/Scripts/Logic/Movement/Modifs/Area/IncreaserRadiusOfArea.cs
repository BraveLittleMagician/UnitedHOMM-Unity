#nullable enable

public class IncreaserRadiusOfArea<TAxes> : ModificatorOfArea<TAxes> where TAxes : struct, IAxes
{
    public override RelativeArea<TAxes> Apply(RelativeArea<TAxes> sequence) => sequence.WithIncreasedRadius();
}