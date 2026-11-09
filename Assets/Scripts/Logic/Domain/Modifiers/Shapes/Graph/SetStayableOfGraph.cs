#nullable enable

using System.Collections.Generic;

/// <summary>
/// Устанавливает Stay-флаг у указанных клеток в <see cref="RelativeGraph{TSquare}"/>.
/// </summary>
/// <remarks>
/// Работает через <c>RelativeGraph.SetStayable</c>, которая уважает
/// <see cref="RelativeGraph{TSquare}.GuaranteesAtLeastOneStayable"/>:
/// снять последнюю Stay-клетку через этот модификатор нельзя
/// (<c>SetStayable(..., NotStay)</c> вернёт <c>false</c>, граф не изменится).
/// </remarks>
public class SetStayableOfGraph<TSquare> : ModificatorOfGraph<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public IEnumerable<(TSquare position, Stayable stayable)> Cells { get; }
    public SetStayableOfGraph(IEnumerable<(TSquare, Stayable)> cells) => Cells = cells;
    public override RelativeGraph<TSquare> Apply(RelativeGraph<TSquare> sequence)
    {
        var copy = sequence.Copy();
        foreach (var (position, stayable) in Cells)
            copy.SetStayable(position, stayable);
        return copy;
    }
}