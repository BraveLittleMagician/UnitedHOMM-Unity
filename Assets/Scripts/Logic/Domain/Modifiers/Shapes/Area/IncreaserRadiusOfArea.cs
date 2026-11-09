#nullable enable

public class IncreaserRadiusOfArea<TSquare> : ModificatorOfArea<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public override RelativeArea<TSquare> Apply(RelativeArea<TSquare> sequence) => sequence.WithIncreasedRadius();
}