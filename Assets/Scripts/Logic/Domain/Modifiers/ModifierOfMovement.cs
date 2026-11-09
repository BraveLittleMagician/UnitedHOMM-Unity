#nullable enable

public abstract class ModifierOfMovement { }

/// <summary>
/// Базовый класс shape-модификатора: умеет применять трансформацию к форме движения.
/// </summary>
/// <remarks>
/// <para>
/// <b>Соглашение для «удаляющих» shape-модификаторов</b>
/// (<c>Remover*</c>, <c>Decreaser*</c>):
/// </para>
/// <para>
/// Над immutable-формами (<see cref="RelativeArea{TSquare}"/>, <see cref="Pattern{TSquare}"/>)
/// removal-модификаторы пересоздают форму с <c>guarantee: false</c> и <b>не уважают</b>
/// <see cref="ISequence.GuaranteesAtLeastOneStayable"/>. После применения у фигуры
/// может остаться 0 Stay-клеток — движение полностью блокировано. Это осознанно:
/// модификатор должен уметь парализовать фигуру, если так задумано дизайном.
/// </para>
/// <para>
/// Над mutable-формой (<see cref="RelativeGraph{TSquare}"/>) removal-модификаторы
/// работают через мутацию копии графа и попадают в проверки
/// <c>RelativeGraph.SetStayable</c> / <c>RelativeGraph.Remove</c>. Там защита частичная:
/// <c>SetStayableOfGraph</c> действительно не даёт опустить форму в 0 Stay,
/// а <c>DecreaserOfGraph</c> защищает только origin-сегмент, но может обнулить
/// Stay-набор, если единственная Stay-клетка была в удаляемой позиции.
/// См. документацию каждого модификатора.
/// </para>
/// </remarks>
public abstract class ModifierOfMovement<TSequence, TPosition> : ModifierOfMovement, IModifier, IShapeTransformModifier<TSequence, TPosition> where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct, ISquare<TPosition>
{
    public virtual ShapeTarget Target => ShapeTarget.Both;

    public virtual string ModifierName => GetType().Name;
    public virtual int MaxDuration => -1;
    public virtual int CurrentDuration => -1;
    public virtual int MaxCharges => -1;
    public virtual int CurrentCharges => -1;

    public virtual void Attach(ModifierContext context) { }
    public virtual void Detach() { }
    public virtual bool TryConsumeCharge() => true;
    public virtual void RestoreCharges(int charges) { }
    public virtual void RestoreDuration(int duration) { }

    public abstract TSequence Apply(TSequence sequence);
}