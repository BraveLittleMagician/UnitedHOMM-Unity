#nullable enable

using System.Collections.Generic;

/// <summary>
/// Удаляет указанные клетки из <see cref="RelativeGraph{TSquare}"/>
/// (через <c>Remove</c> на копии графа).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RelativeGraph{TSquare}.Remove(Vector3Int)"/> защищает от удаления
/// origin-сегмента при <c>GuaranteesAtLeastOneStayable = true</c>, но не проверяет,
/// что после удаления останется хотя бы одна Stay-клетка. Если единственная
/// Stay-клетка была в удаляемой позиции (не в origin-offset) — форма опустеет,
/// флаг <c>GuaranteesAtLeastOneStayable</c> останется <c>true</c>,
/// а инвариант «флаг ⇒ ≥1 Stay» нарушится.
/// </para>
/// <para>
/// Это не задевает текущий геймплей (модификатор не используется в данных фигур).
/// Если единообразие с <c>SetStayableOfGraph</c> понадобится — расширить проверку
/// в <c>RelativeGraph.Remove(TSquare)</c>: не разрешать удаление, если в удаляемом
/// узле была последняя Stay-клетка графа.
/// </para>
/// </remarks>
public class DecreaserOfGraph<TSquare> : ModificatorOfGraph<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public IEnumerable<TSquare> Positions { get; }
    public DecreaserOfGraph(IEnumerable<TSquare> positions) => Positions = positions;
    public override RelativeGraph<TSquare> Apply(RelativeGraph<TSquare> sequence)
    {
        var copy = sequence.Copy();
        foreach (var square in Positions) copy.Remove(square);
        return copy;
    }
}