#nullable enable

using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Удаляет указанные клетки из <see cref="Pattern{TSquare}"/>.
/// </summary>
/// <remarks>
/// Не уважает <see cref="Pattern{TSquare}.GuaranteesAtLeastOneStayable"/>:
/// форма пересоздаётся с <c>guarantee = false</c> (второй элемент из
/// <c>DataWithRemoved</c>). Если модификатор удалил последнюю Stay-клетку —
/// движение фигуры полностью блокируется.
/// </remarks>
public class DecreaserOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public DecreaserOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
    {
        var (squares, attackable, guarantee) = sequence.DataWithRemoved(Squares);
        return new Pattern<TSquare>(squares, guarantee);
    }
}