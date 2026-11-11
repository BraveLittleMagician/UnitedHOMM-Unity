#nullable enable
using System;
using System.Collections.Generic;

public sealed class Movement<TSequence> : IMovement where TSequence : notnull, ISequence<Square, TSequence>
{
    private readonly TSequence _moveSequence;
    private readonly TSequence? _attackSequence;
    private readonly bool _hasAttackSequence;
    private readonly IEventBus _eventBus;
    private readonly ModifierHost _modifierHost;
    private readonly IPiece _owner;
    private bool _disposed;

    public Movement(TSequence moveSequence, TSequence? attackSequence, bool hasAttackSequence, IEventBus eventBus, ModifierHost modifierHost, IPiece owner)
    {
        _moveSequence = moveSequence;
        _attackSequence = attackSequence;
        _hasAttackSequence = hasAttackSequence;
        _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        _modifierHost = modifierHost ?? throw new System.ArgumentNullException(nameof(modifierHost));
        _owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
    }

    private ISequenceEnumerator<Square> CreateWorldEnumerator(IPiece piece, Square fromWorld)
    {
        var transforms = _modifierHost.GetShapeTransforms<TSequence, Square>(piece.IndexInHouse);

        var move = _moveSequence;
        foreach (var t in transforms)
            if (t.Target.HasFlag(ShapeTarget.Movement))
                move = t.Apply(move);

        var startLocal = FacingRotation.Unrotate(fromWorld, piece.Facing);

        if (!_hasAttackSequence)
        {
            var inner = move.GetEnumerator(startLocal);
            return new FacingEnumerator(inner, piece.Facing, fromWorld);
        }

        var attack = _attackSequence!;
        foreach (var t in transforms)
            if (t.Target.HasFlag(ShapeTarget.Attack))
                attack = t.Apply(attack);

        var moveEnum = move.GetEnumerator(startLocal);
        var attackEnum = attack.GetEnumerator(startLocal);
        var combined = new CombinedEnumerator(moveEnum, attackEnum);
        return new FacingEnumerator(combined, piece.Facing, fromWorld);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
    public bool TryFindPath<TPos>(IPiece piece, TPos fromPosition, TPos target, IReadOnlyRoom room, out IPath<TPos>? path) where TPos : struct
    {
        path = null;
        if (fromPosition is not Square fromSquare) return false;
        if (target is not Square targetSquare) return false;
        if (targetSquare.Equals(fromSquare)) return false;
        if (room.TryToGetPiece<Square>(targetSquare, out _)) return false;

        var enumerator = CreateWorldEnumerator(piece, fromSquare);
        var positions = enumerator.PositionsAll;
        if (!positions.TryGetValue(targetSquare, out var targetFlag)) return false;
        if (targetFlag != Stayable.Stay) return false;

        var visited = new HashSet<Square> { fromSquare };
        var parent = new Dictionary<Square, Square>();
        var queue = new Queue<Square>();
        queue.Enqueue(fromSquare);
        bool found = false;
        while (queue.Count > 0 && !found)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in enumerator.GetNeighbors(current))
            {
                if (!visited.Add(neighbor)) continue;
                if (!positions.ContainsKey(neighbor)) continue;
                if (room.TryToGetPiece<Square>(neighbor, out _)) continue;
                parent[neighbor] = current;
                if (neighbor.Equals(targetSquare)) { found = true; break; }
                queue.Enqueue(neighbor);
            }
        }
        if (!found) return false;

        var pathPositions = new List<Square>();
        var node = targetSquare;
        while (!node.Equals(fromSquare))
        {
            pathPositions.Add(node);
            node = parent[node];
        }
        pathPositions.Add(fromSquare);
        pathPositions.Reverse();

        var builtPath = new PathSequence<Square>(pathPositions.ToArray());
        if (!enumerator.CanMoveTo(builtPath)) return false;
        path = (IPath<TPos>)(object)builtPath;
        return true;
    }
    public bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct
    {
        if (path == null || path.Positions.Count < 2)
        {
            error = "Path too short";
            return false;
        }
        if (typeof(TPos) != typeof(Square))
        {
            error = "Path has wrong position type";
            return false;
        }

        var fromRaw = (Square)(object)path.Positions[0];

        if (!room.TryToGetPosition<Square>(piece.IndexInHouse, out var actualFrom))
        {
            error = "Piece has no position in room";
            return false;
        }
        if (!actualFrom.Equals(fromRaw))
        {
            error = "Path does not start at piece's position";
            return false;
        }
        var target = (Square)(object)path.Positions[^1];
        if (!TryFindPath(piece, actualFrom, target, room, out _))
        {
            error = $"No valid path from {actualFrom} to {target}";
            return false;
        }
        error = "";
        return true;
    }
    public bool IsApplicableToRoom(IReadOnlyRoom room) => room is ISquarePositionRoom;
    public ISequenceEnumerator<Square>? CreateEnumeratorFor(IPiece piece, Square fromPosition) => CreateWorldEnumerator(piece, fromPosition);
    public IReadOnlyList<ShapeCell> GetShape<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room, bool includeOccupied) where TPos : struct
    {
        if (fromPosition is not Square fromSquare) return Array.Empty<ShapeCell>();

        var enumerator = CreateWorldEnumerator(piece, fromSquare);
        var positions = enumerator.PositionsAll;
        var attackables = enumerator.PositionsAttackable;
        var result = new List<ShapeCell>();

        foreach (var (cell, stayable) in positions)
        {
            if (cell.Equals(fromSquare)) continue;
            if (!includeOccupied && room.TryToGetPiece<Square>(cell, out _)) continue;
            var attackable = attackables.TryGetValue(cell, out var a) ? a : Attackable.NotAttack;
            result.Add(new ShapeCell(cell, stayable, attackable));
        }
        return result;
    }
    public IReadOnlyList<TPos> GetValidMoves<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room) where TPos : struct
    {
        if (fromPosition is not Square fromSquare) return Array.Empty<TPos>();

        var enumerator = CreateWorldEnumerator(piece, fromSquare);
        var positions = enumerator.PositionsAll;
        var result = new List<TPos>();

        foreach (var (cell, stayable) in positions)
        {
            if (cell.Equals(fromSquare)) continue;
            if (stayable != Stayable.Stay) continue;
            if (room.TryToGetPiece<Square>(cell, out _)) continue;
            result.Add((TPos)(object)cell);
        }
        return result;
    }
}