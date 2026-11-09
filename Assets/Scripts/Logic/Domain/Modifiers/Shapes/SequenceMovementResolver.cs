#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public sealed class SequenceMovementResolver : IMovementResolver
{
    private readonly PieceRegistry _registry;
    private readonly Func<PieceModel, ISequenceEnumerator<Square>> _enumeratorFactory;

    private SequenceMovementResolver(
        PieceRegistry registry,
        Func<PieceModel, ISequenceEnumerator<Square>> enumeratorFactory)
    {
        _registry = registry;
        _enumeratorFactory = enumeratorFactory;
    }

    private (Square start, Dictionary<Square, Stayable> reachable, Dictionary<Square, Attackable> reachableAttackable, Dictionary<Square, Square> parent) WalkBfs(ISequenceEnumerator<Square> enumerator, Vector3Int gridSize)
    {
        var start = enumerator.StartPosition;
        var positions = enumerator.PositionsAll;
        var attackable = enumerator.PositionsAttackable;
        int maxDepth = enumerator.MaxDepth;

        var reachable = new Dictionary<Square, Stayable>();
        var reachableAttackable = new Dictionary<Square, Attackable>();
        var parent = new Dictionary<Square, Square>();
        var visited = new HashSet<Square> { start };
        var queue = new Queue<(Square pos, int depth)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            foreach (var neighbor in enumerator.GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;
                if (!positions.ContainsKey(neighbor)) continue;
                if (!IsInsideGrid(neighbor, gridSize)) continue;

                int nextDepth = depth + 1;
                if (nextDepth > maxDepth) continue;

                visited.Add(neighbor);
                reachable[neighbor] = positions[neighbor];
                if (attackable.TryGetValue(neighbor, out var attack))
                    reachableAttackable[neighbor] = attack;
                parent[neighbor] = current;

                if (_registry.IsOccupied(ToVector3Int(neighbor)))
                    continue;

                if (nextDepth < maxDepth)
                    queue.Enqueue((neighbor, nextDepth));
            }
        }

        return (start, reachable, reachableAttackable, parent);
    }
    private static bool IsInsideGrid(Square s, Vector3Int gridSize) =>
        s.X >= 0 && s.X < gridSize.x &&
        s.Y >= 0 && s.Y < gridSize.y &&
        s.Z >= 0 && s.Z < gridSize.z;
    private static Square ToSquare(Vector3Int v) => new() { X = v.x, Y = v.y, Z = v.z, W = 0 };
    private static Vector3Int ToVector3Int(Square s) => new(s.X, s.Y, s.Z);
    private static IPath<Square> BuildPath(Dictionary<Square, Square> parent, Square start, Square target)
    {
        var positions = new List<Square>();
        var node = target;
        while (!node.Equals(start))
        {
            positions.Add(node);
            node = parent[node];
        }
        positions.Add(start);
        positions.Reverse();
        return new PathSequence<Square>(positions.ToArray());
    }

    public bool TryFindPath(PieceModel piece, Vector3Int target, Vector3Int gridSize, [NotNullWhen(true)] out IPath<Square>? path)
    {
        path = null;
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        var startSquare = ToSquare(piece.PositionInGrid);
        var targetSquare = ToSquare(target);
        if (targetSquare.Equals(startSquare)) return false;
        if (!IsInsideGrid(targetSquare, gridSize)) return false;
        if (_registry.IsOccupied(target)) return false;

        var enumerator = _enumeratorFactory(piece);

        if (!enumerator.PositionsAll.TryGetValue(targetSquare, out var targetFlag)
            || targetFlag != Stayable.Stay)
            return false;

        var (start, reachable, _, parent) = WalkBfs(enumerator, gridSize);
        if (!reachable.ContainsKey(targetSquare)) return false;

        var builtPath = BuildPath(parent, start, targetSquare);
        if (!enumerator.CanMoveTo(builtPath)) return false;

        path = builtPath;
        return true;
    }
    public IReadOnlyList<Vector3Int> GetValidMoves(PieceModel piece, Vector3Int gridSize)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        var enumerator = _enumeratorFactory(piece);
        var (_, reachable, _, _) = WalkBfs(enumerator, gridSize);

        var result = new List<Vector3Int>();
        foreach (var (cell, stayable) in reachable)
        {
            if (stayable != Stayable.Stay) continue;
            if (_registry.IsOccupied(ToVector3Int(cell))) continue;
            result.Add(ToVector3Int(cell));
        }
        return result;
    }
    public IReadOnlyList<ShapeCell> GetShape(PieceModel piece, Vector3Int gridSize, bool includeOccupied = false)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        var enumerator = _enumeratorFactory(piece);
        var (_, reachable, reachableAttackable, _) = WalkBfs(enumerator, gridSize);

        var result = new List<ShapeCell>(reachable.Count);
        foreach (var (cell, stayable) in reachable)
        {
            if (!includeOccupied && _registry.IsOccupied(ToVector3Int(cell))) continue;
            reachableAttackable.TryGetValue(cell, out var attack);
            result.Add(new ShapeCell(ToVector3Int(cell), stayable, attack));
        }
        return result;
    }
    public ISequenceEnumerator<Square>? CreateEnumerator(PieceModel piece)
    {
        if (piece == null) return null;
        return _enumeratorFactory(piece);
    }
    public static SequenceMovementResolver ForShape<TSelf>(PieceRegistry registry,ModifierHost? modifierHost,TSelf baseShape)where TSelf : ISequence<Square, TSelf>
    {
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        return new SequenceMovementResolver(registry, piece =>
        {
            var shape = baseShape;
            if (modifierHost != null)
            {
                var transforms = modifierHost.GetShapeTransforms<TSelf, Square>(piece.PieceId);
                for (int i = 0; i < transforms.Count; i++)
                    shape = transforms[i].Apply(shape);
            }
            var startLocal = FacingRotation.Unrotate(piece.PositionInGrid, piece.Facing);
            var inner = shape.GetEnumerator(new Square
            {
                X = startLocal.x,
                Y = startLocal.y,
                Z = startLocal.z,
                W = 0
            });
            return new FacingEnumerator(inner, piece.Facing, new Square
            {
                X = piece.PositionInGrid.x,
                Y = piece.PositionInGrid.y,
                Z = piece.PositionInGrid.z,
                W = 0
            });
        });
    }
    public static SequenceMovementResolver ForCombined<TSelf>(PieceRegistry registry, ModifierHost? modifierHost, TSelf moveBase, TSelf attackBase) where TSelf : ISequence<Square, TSelf>
    {
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        return new SequenceMovementResolver(registry, piece =>
        {
            var moveShape = moveBase;
            var attackShape = attackBase;

            if (modifierHost != null)
            {
                var transforms = modifierHost.GetShapeTransforms<TSelf, Square>(piece.PieceId);
                for (int i = 0; i < transforms.Count; i++)
                {
                    var t = transforms[i];
                    if (t.Target.HasFlag(ShapeTarget.Movement))
                        moveShape = t.Apply(moveShape);
                    if (t.Target.HasFlag(ShapeTarget.Attack))
                        attackShape = t.Apply(attackShape);
                }
            }

            var startLocal = FacingRotation.Unrotate(piece.PositionInGrid, piece.Facing);
            var startSquare = new Square
            {
                X = startLocal.x,
                Y = startLocal.y,
                Z = startLocal.z,
                W = 0
            };

            var moveEnum = moveShape.GetEnumerator(startSquare);
            var attackEnum = attackShape.GetEnumerator(startSquare);
            var combined = new CombinedEnumerator(moveEnum, attackEnum);

            return new FacingEnumerator(combined, piece.Facing, new Square
            {
                X = piece.PositionInGrid.x,
                Y = piece.PositionInGrid.y,
                Z = piece.PositionInGrid.z,
                W = 0
            });
        });
    }
}