#nullable enable

using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Снимает Stay-флаг у указанных клеток в <see cref="Pattern{TSquare}"/>.
/// </summary>
/// <remarks>
/// Не уважает <see cref="Pattern{TSquare}.GuaranteesAtLeastOneStayable"/>:
/// форма пересоздаётся с <c>guarantee = false</c> (второй элемент из
/// <c>DataWithSetStayable</c>). Если модификатор снял последнюю Stay-клетку —
/// движение фигуры полностью блокируется.
/// </remarks>
public class RemoverStayableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public RemoverStayableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
    {
        (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee) = sequence.DataWithSetStayable(Squares, Stayable.NotStay);
        return new Pattern<TSquare>(squares, guarantee);
    }
}