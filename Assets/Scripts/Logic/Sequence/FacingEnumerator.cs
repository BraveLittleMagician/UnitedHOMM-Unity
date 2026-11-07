#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Обёртка над <see cref="ISequenceEnumerator{TPosition}"/>, которая прозрачно
/// переводит координаты между локальной системой формы и мировой системой
/// резолвера.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SequenceMovementResolver"/> всегда работает в мировых
/// (grid-)координатах. Но формы движения (<see cref="PathSetPattern"/>,
/// <see cref="Pattern{TSquare}"/>, <see cref="RelativeArea{TSquare}"/>,
/// <see cref="RelativeGraph{TSquare}"/>) заданы в локальной системе, где
/// «вперёд» = <c>(0, 1, 0)</c> — иначе <see cref="OpeningDoubleMoveModifier"/>
/// не смог бы добавить «две клетки вперёд» без знания о
/// <see cref="PieceModel.Facing"/>.
/// </para>
/// <para>
/// Резолвер оборачивает форму в <see cref="FacingEnumerator"/> и получает
/// энумератор, который:
/// </para>
/// <list type="bullet">
/// <item><description>отдаёт клетки в мировых координатах через
/// <see cref="FacingRotation.Rotate(Square, FacingDirection)"/>;</description></item>
/// <item><description>принимает клетки обратно (в <see cref="GetNeighbors"/>,
/// <see cref="CanMoveTo(Square)"/>) и разворачивает их через
/// <see cref="FacingRotation.Unrotate(Square, FacingDirection)"/> перед
/// делегированием;</description></item>
/// <item><description>для путей (<see cref="CanMoveTo(IPath{Square})"/>)
/// разворачивает весь массив позиций, чтобы внутренний энумератор получил
/// путь в локальной системе.</description></item>
/// </list>
/// <para>
/// <see cref="MaxDepth"/> прокидывается из внутреннего энумератора без
/// изменений — вращение не влияет на глубину.
/// </para>
/// <para>
/// <see cref="PositionsAttackable"/> переводится аналогично <see cref="PositionsAll"/>:
/// ключи поворачиваются через <see cref="FacingRotation.Rotate(Square, FacingDirection)"/>,
/// значения (<see cref="Attackable"/>) сохраняются как есть.
/// </para>
/// </remarks>
public sealed class FacingEnumerator : ISequenceEnumerator<Square>
{
    private readonly ISequenceEnumerator<Square> _inner;
    private readonly FacingDirection _facing;
    private readonly List<Square> _stayWorld;
    private readonly Dictionary<Square, Stayable> _positionsWorld;
    private readonly Dictionary<Square, Attackable> _attackableWorld;

    public FacingEnumerator(ISequenceEnumerator<Square> inner, FacingDirection facing, Square startWorld)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _facing = facing;
        StartPosition = startWorld;

        _positionsWorld = new Dictionary<Square, Stayable>(inner.PositionsAll.Count);
        foreach (var (local, stay) in inner.PositionsAll)
            _positionsWorld[FacingRotation.Rotate(local, facing)] = stay;

        _stayWorld = _positionsWorld
            .Where(kv => kv.Value == Stayable.Stay)
            .Select(kv => kv.Key)
            .ToList();

        var innerAttackable = inner.PositionsAttackable;
        _attackableWorld = new Dictionary<Square, Attackable>(innerAttackable.Count);
        foreach (var (local, attack) in innerAttackable)
            _attackableWorld[FacingRotation.Rotate(local, facing)] = attack;
    }
    public bool SupportsExplicitAttackable => _inner.SupportsExplicitAttackable;
    public int MaxDepth => _inner.MaxDepth;
    public Square StartPosition { get; }
    public List<Square> PositionsStayables => _stayWorld;
    public Dictionary<Square, Stayable> PositionsAll => _positionsWorld;
    public IReadOnlyDictionary<Square, Attackable> PositionsAttackable => _attackableWorld;

    public IEnumerable<Square> GetNeighbors(Square cellWorld)
    {
        var local = FacingRotation.Unrotate(cellWorld, _facing);
        foreach (var nLocal in _inner.GetNeighbors(local))
            yield return FacingRotation.Rotate(nLocal, _facing);
    }

    public IPath<Square> CombinePositionsIntoPath(Square[] positions)
        => new PathSequence<Square>(positions);

    public bool CanMoveTo(Square targetWorld)
        => _inner.CanMoveTo(FacingRotation.Unrotate(targetWorld, _facing));

    public bool CanMoveTo(IPath<Square> pathWorld)
    {
        var positions = pathWorld.Positions;
        if (positions.Count == 0) return false;
        if (!positions[0].Equals(StartPosition)) return false;
        var local = new Square[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            local[i] = FacingRotation.Unrotate(positions[i], _facing);
        return _inner.CanMoveTo(new PathSequence<Square>(local));
    }
}