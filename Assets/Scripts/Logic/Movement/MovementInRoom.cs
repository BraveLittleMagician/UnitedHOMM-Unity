#nullable enable
using System;
using System.Collections.Generic;

public abstract class MovementInRoom<TSequence> : IMovement where TSequence : notnull, ISequence<Square, TSequence>
{
    private readonly IEventBus _eventBus;
    private readonly ModifierHost _modifierHost;
    private readonly IPiece _owner;
    private bool _disposed;

    protected TSequence MovementSequence { get; }
    protected TSequence AttackSequence { get; }

    protected MovementInRoom(TSequence moveSequence, TSequence attackSequence, IEventBus eventBus, ModifierHost modifierHost, IPiece owner)
    {
        MovementSequence = moveSequence;
        AttackSequence = attackSequence;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public abstract ISequenceEnumerator<Square> CreateMoveEnumeratorFor(IPiece piece, Square fromPosition);
    public abstract ISequenceEnumerator<Square> CreateAttackEnumeratorFor(IPiece piece, Square fromPosition);
    public abstract ISequenceEnumerator<Square> CreateCombinedEnumeratorFor(IPiece piece, Square fromPosition);

    protected IReadOnlyList<IShapeTransformModifier<TSequence, Square>> GetShapeTransforms(IPiece piece) => _modifierHost.GetShapeTransforms<TSequence, Square>(piece.IndexInHouse);
    protected TSequence ApplyMovementModifiers(TSequence baseSequence, IPiece piece) => ApplyModifiers(baseSequence, piece, ShapeTarget.Movement);
    protected TSequence ApplyAttackModifiers(TSequence baseSequence, IPiece piece) => ApplyModifiers(baseSequence, piece, ShapeTarget.Attack);

    protected ISequenceEnumerator<Square> WrapWithFacing(ISequenceEnumerator<Square> innerLocal, IPiece piece, Square fromWorld) => new FacingEnumerator(innerLocal, piece.Facing, fromWorld);
    protected static ISequenceEnumerator<Square> WithAttackSemantics(ISequenceEnumerator<Square> inner)
        => inner.SupportsExplicitAttackable ? inner : new AttackSemanticsEnumerator(inner);

    private TSequence ApplyModifiers(TSequence baseSequence, IPiece piece, ShapeTarget flag)
    {
        var modified = baseSequence;
        var transforms = GetShapeTransforms(piece);
        for (int i = 0; i < transforms.Count; i++)
        {
            var t = transforms[i];
            if ((t.Target & flag) != 0)
                modified = t.Apply(modified);
        }
        return modified;
    }

    public IReadOnlyDictionary<Square, SquareInfo> GetInfos(IPiece piece, Square fromPosition) => CreateCombinedEnumeratorFor(piece, fromPosition).Squares;

    public bool CanAttack<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, out string error) where TPos : struct
    {
        if (path == null)
        {
            error = "Path is null";
            return false;
        }
        if (path.Positions.Count < 2)
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

        var enumerator = CreateAttackEnumeratorFor(piece, actualFrom);
        var squarePath = (IPath<Square>)(object)path;
        if (!enumerator.CanAttack(squarePath))
        {
            error = "Path is not a valid attack path";
            return false;
        }

        error = "";
        return true;
    }
    public bool TryFindPath<TPos>(IPiece piece, TPos fromPosition, TPos target, IReadOnlyRoom room, out IPath<TPos>? path) where TPos : struct
    {
        path = null;
        if (fromPosition is not Square fromSquare) return false;
        if (target is not Square targetSquare) return false;
        if (targetSquare.Equals(fromSquare)) return false;
        if (room is not ISquarePositionRoom sqRoom) return false;
        if (!sqRoom.IsInside(targetSquare)) return false;
        if (room.TryToGetPiece<Square>(targetSquare, out _)) return false;

        var enumerator = CreateMoveEnumeratorFor(piece, fromSquare);
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
                if (!sqRoom.IsInside(neighbor)) continue;
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
    public ISequenceEnumerator<Square>? CreateEnumeratorFor(IPiece piece, Square fromPosition) => CreateCombinedEnumeratorFor(piece, fromPosition);

    public IReadOnlyList<ShapeCell> GetShape<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room, bool includeOccupied) where TPos : struct
    {
        if (fromPosition is not Square fromSquare) return Array.Empty<ShapeCell>();
        if (room is not ISquarePositionRoom sqRoom) return Array.Empty<ShapeCell>();

        var enumerator = CreateCombinedEnumeratorFor(piece, fromSquare);
        var positions = enumerator.PositionsAll;
        var attackables = enumerator.PositionsAttackable;
        int maxDepth = enumerator.MaxDepth;

        var result = new List<ShapeCell>();
        var visited = new HashSet<Square> { fromSquare };
        var queue = new Queue<(Square cell, int depth)>();
        queue.Enqueue((fromSquare, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth >= maxDepth) continue;

            foreach (var neighbor in enumerator.GetNeighbors(current))
            {
                if (!visited.Add(neighbor)) continue;
                if (!positions.ContainsKey(neighbor)) continue;
                if (!sqRoom.IsInside(neighbor)) continue;

                bool occupied = room.TryToGetPiece<Square>(neighbor, out var targetPiece);
                var attackable = attackables.TryGetValue(neighbor, out var a) ? a : Attackable.NotAttack;

                if (!occupied)
                {
                    result.Add(new ShapeCell(neighbor, positions[neighbor], attackable));
                    if (depth + 1 < maxDepth)
                        queue.Enqueue((neighbor, depth + 1));
                }
                else if (includeOccupied
                         && attackable == Attackable.Attack
                         && targetPiece != null
                         && !targetPiece.Owner.Equals(piece.Owner))
                {
                    result.Add(new ShapeCell(neighbor, Stayable.NotStay, Attackable.Attack));
                }
            }
        }

        return result;
    }
    public IReadOnlyList<TPos> GetValidMoves<TPos>(IPiece piece, TPos fromPosition, IReadOnlyRoom room)
        where TPos : struct
    {
        if (fromPosition is not Square) return Array.Empty<TPos>();

        var shape = GetShape(piece, fromPosition, room, includeOccupied: false);
        var result = new List<TPos>(shape.Count);
        for (int i = 0; i < shape.Count; i++)
        {
            if (shape[i].Stayable != Stayable.Stay) continue;
            result.Add((TPos)(object)shape[i].Cell);
        }
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private sealed class AttackSemanticsEnumerator : ISequenceEnumerator<Square>
    {
        private readonly ISequenceEnumerator<Square> _inner;
        private Dictionary<Square, Attackable>? _attackableCache;

        public AttackSemanticsEnumerator(ISequenceEnumerator<Square> inner)
        {
            _inner = inner;
        }

        public Square StartPosition => _inner.StartPosition;
        public int MaxDepth => _inner.MaxDepth;
        public Dictionary<Square, Stayable> PositionsAll => _inner.PositionsAll;
        public List<Square> PositionsStayables => _inner.PositionsStayables;
        public bool SupportsExplicitAttackable => true;

        public IReadOnlyDictionary<Square, Attackable> PositionsAttackable
        {
            get
            {
                if (_attackableCache != null) return _attackableCache;
                var stayables = _inner.PositionsStayables;
                var dict = new Dictionary<Square, Attackable>(stayables.Count);
                for (int i = 0; i < stayables.Count; i++)
                    dict[stayables[i]] = Attackable.Attack;
                _attackableCache = dict;
                return dict;
            }
        }

        public IEnumerable<Square> GetNeighbors(Square cell) => _inner.GetNeighbors(cell);
        public bool CanMoveTo(Square target) => _inner.CanMoveTo(target);
        public bool CanMoveTo(IPath<Square> path) => _inner.CanMoveTo(path);
        public IPath<Square> CombinePositionsIntoPath(Square[] positions)
            => _inner.CombinePositionsIntoPath(positions);
    }
}