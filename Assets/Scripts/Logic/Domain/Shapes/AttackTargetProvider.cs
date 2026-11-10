#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttackTargetProvider : IAttackTargetProvider
{
    private readonly IReadOnlyDictionary<string, IMovementResolver> _resolvers;
    private readonly PieceRegistry _registry;
    private readonly ModifierHost _modifierHost;

    public AttackTargetProvider(
        IReadOnlyDictionary<string, IMovementResolver> resolvers,
        PieceRegistry registry,
        ModifierHost modifierHost)
    {
        _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
    }

    private bool CanAttackInGeneral(PieceModel attacker)
    {
        if (attacker.IsDead) return false;
        if (attacker.HasPromotedThisTurn) return false;
        if (attacker.AttackCharges <= 0) return false;

        var modifiers = _modifierHost.GetModifiers(attacker.PieceId);
        for (int i = 0; i < modifiers.Count; i++)
            if (modifiers[i] is IAttackBlocker blocker && !blocker.CanAttack(attacker))
                return false;
        return true;
    }
    private IReadOnlyList<ShapeCell> GetShapeWithoutMoveGuard(PieceModel attacker, Vector3Int gridSize)
    {
        if (!_resolvers.TryGetValue(attacker.Name, out var resolver))
            return Array.Empty<ShapeCell>();
        return resolver.GetShape(attacker, gridSize, includeOccupied: true);
    }

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

    public bool IsAttackTarget(PieceModel attacker, Vector3Int cell, Vector3Int gridSize)
    {
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));
        if (!CanAttackInGeneral(attacker)) return false;

        var target = _registry.GetAt(cell);
        if (target == null || target.IsDead) return false;
        if (target.Player == attacker.Player) return false;

        var shape = GetShapeWithoutMoveGuard(attacker, gridSize);
        for (int i = 0; i < shape.Count; i++)
        {
            if (shape[i].Cell == cell && shape[i].Attackable == Attackable.Attack)
                return true;
        }
        return false;
    }
    public bool TryFindAttackPath(PieceModel attacker, Vector3Int target, Vector3Int gridSize, out IPath<Square>? path)
    {
        path = null;
        if (attacker == null) return false;
        if (!_resolvers.TryGetValue(attacker.Name, out var resolver)) return false;
        var enumerator = resolver.CreateEnumerator(attacker);
        if (enumerator == null) return false;

        var start = enumerator.StartPosition;
        var targetSquare = new Square { X = target.x, Y = target.y, Z = target.z, W = 0 };
        if (start.Equals(targetSquare)) return false;

        var positions = enumerator.PositionsAll;
        if (!positions.ContainsKey(targetSquare)) return false;

        var visited = new HashSet<Square> { start };
        var parent = new Dictionary<Square, Square>();
        var queue = new Queue<Square>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in enumerator.GetNeighbors(current))
            {
                if (!visited.Add(neighbor)) continue;
                if (!positions.ContainsKey(neighbor)) continue;
                parent[neighbor] = current;
                if (neighbor.Equals(targetSquare))
                {
                    path = BuildPath(parent, start, targetSquare);
                    return true;
                }
                queue.Enqueue(neighbor);
            }
        }
        return false;
    }
    public IReadOnlyList<Vector3Int> GetAttackTargets(PieceModel attacker, Vector3Int gridSize)
    {
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));
        if (!CanAttackInGeneral(attacker)) return Array.Empty<Vector3Int>();

        var shape = GetShapeWithoutMoveGuard(attacker, gridSize);
        var result = new List<Vector3Int>();
        for (int i = 0; i < shape.Count; i++)
        {
            var sc = shape[i];
            if (sc.Attackable != Attackable.Attack) continue;
            var target = _registry.GetAt(sc.Cell);
            if (target == null || target.IsDead) continue;
            if (target.Player == attacker.Player) continue;
            result.Add(sc.Cell);
        }
        return result;
    }

}