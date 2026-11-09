#nullable enable

/// <summary>
/// Снимает Stay-флаг у указанного радиуса в <see cref="RelativeArea{TSquare}"/>.
/// </summary>
/// <remarks>
/// Не уважает <see cref="RelativeArea{TSquare}.GuaranteesAtLeastOneStayable"/>:
/// форма пересоздаётся с <c>guarantee: false</c>. Если модификатор снимает
/// последнюю Stay-клетку — движение фигуры полностью блокируется.
/// </remarks>
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