#nullable enable

/// <summary>
/// Уменьшает радиус обхода <see cref="RelativeArea{TSquare}"/> на 1
/// (обрезает Stay-клетки за пределами нового радиуса).
/// </summary>
/// <remarks>
/// Не уважает <see cref="RelativeArea{TSquare}.GuaranteesAtLeastOneStayable"/>:
/// форма пересоздаётся с <c>guarantee: false</c>. Если модификатор срезал все
/// Stay-клетки — движение фигуры полностью блокируется.
/// </remarks>
public class DecreaserRadiusOfArea<TSquare> : ModificatorOfArea<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public override RelativeArea<TSquare> Apply(RelativeArea<TSquare> sequence)
    {
        var (activeAxes, stayables, length, isCircle, guarantees) = sequence.DataWithDecreasedRadius();
        return new RelativeArea<TSquare>(activeAxes, stayables, length, isCircle, false);
    }
}