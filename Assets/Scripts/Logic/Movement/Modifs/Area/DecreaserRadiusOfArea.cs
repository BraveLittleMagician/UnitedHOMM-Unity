#nullable enable

public class DecreaserRadiusOfArea<TSquare> : ModificatorOfArea<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public override RelativeArea<TSquare> Apply(RelativeArea<TSquare> sequence)
    {
        var (activeAxes, stayables, length, isCircle, guarantees) = sequence.DataWithDecreasedRadius();
        return new RelativeArea<TSquare>(activeAxes, stayables, length, isCircle, false);
    }
}