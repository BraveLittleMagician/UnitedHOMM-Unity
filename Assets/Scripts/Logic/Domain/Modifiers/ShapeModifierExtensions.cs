#nullable enable

public static class ShapeModifierExtensions
{
    /// <summary>
    /// Возвращает <c>true</c>, если модификатор — чистый shape-модификатор без
    /// состояния. Такие модификаторы не попадают в snapshot и не копируются
    /// при промоушене.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Проверка идёт по маркеру <see cref="IPureShapeModifier"/>, а не по значениям
    /// <c>MaxDuration</c> / <c>CurrentCharges</c> / и т.п. «Чистота» — это свойство
    /// типа, а не текущих значений полей.
    /// </para>
    /// <para>
    /// Конкретно:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="IncreaserRadiusOfArea{TSquare}"/>, <see cref="DecreaserOfGraph{TSquare}"/>
    /// и прочие наследники <see cref="ModificatorOfArea{TSquare}"/>,
    /// <see cref="ModificatorOfPattern{TSquare}"/>, <see cref="ModificatorOfGraph{TSquare}"/>
    /// реализуют <see cref="IPureShapeModifier"/> — их достаточно применить к форме
    /// в рантайме, никакой state не нужен. → <c>true</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="OpeningDoubleMoveModifier"/> — stateful: хранит charges, подписан на
    /// <see cref="PieceEventBus.PieceStartedAction"/>, сам себя удаляет из
    /// <see cref="ModifierHost"/>. Он реализует <see cref="IShapeTransformModifier"/>
    /// (добавляет клетку <c>(0,2,0)</c> в локальной системе), но <b>не</b>
    /// реализует <see cref="IPureShapeModifier"/>. → <c>false</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Гипотетический <c>DurationRadiusOfArea(duration: 2)</c> (радиус +1 на 2 хода)
    /// будет наследником <see cref="ModifierOfMovement{TSequence, TPosition}"/>
    /// напрямую, без маркера → <c>false</c>. Это правильный default: новый
    /// shape-модификатор с состоянием автоматически попадает в snapshot и
    /// переживает undo/promotion, а не теряется как «чистый».
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static bool IsPureShape(IModifier modifier)
        => modifier is IPureShapeModifier;
}