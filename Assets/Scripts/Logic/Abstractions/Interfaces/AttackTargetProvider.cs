#nullable enable

using System;
using System.Collections.Generic;

public sealed class AttackTargetProvider : IAttackTargetProvider
{
    private readonly ModifierHost _modifierHost;

    public AttackTargetProvider(ModifierHost modifierHost)
    {
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
    }

    public bool IsAttackTarget(IPiece attacker, Square cell, IReadOnlyRoom room)
    {
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));
        if (room == null) throw new ArgumentNullException(nameof(room));
        if (!CanAttackInGeneral(attacker)) return false;
        if (!room.TryToGetPiece(cell, out var target) || target == null) return false;
        if (target.Owner.Equals(attacker.Owner)) return false;
        if (!room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos)) return false;

        foreach (var m in attacker.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            var shape = m.GetShape<Square>(attacker, fromPos, room, includeOccupied: true);
            for (int i = 0; i < shape.Count; i++)
            {
                if (shape[i].Cell.Equals(cell) && shape[i].Attackable == Attackable.Attack)
                    return true;
            }
        }
        return false;
    }

    public IReadOnlyList<Square> GetAttackTargets(IPiece attacker, IReadOnlyRoom room)
    {
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));
        if (room == null) throw new ArgumentNullException(nameof(room));
        if (!CanAttackInGeneral(attacker)) return Array.Empty<Square>();
        if (!room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos))
            return Array.Empty<Square>();

        var result = new List<Square>();
        foreach (var m in attacker.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            var shape = m.GetShape<Square>(attacker, fromPos, room, includeOccupied: true);
            for (int i = 0; i < shape.Count; i++)
            {
                var sc = shape[i];
                if (sc.Attackable != Attackable.Attack) continue;
                if (!room.TryToGetPiece(sc.Cell, out var target) || target == null) continue;
                if (target.Owner.Equals(attacker.Owner)) continue;
                result.Add(sc.Cell);
            }
        }
        return result;
    }

    public bool TryFindAttackPath(IPiece attacker, Square target, IReadOnlyRoom room, out IPath<Square>? path)
    {
        path = null;
        if (attacker == null) return false;
        if (room == null) return false;
        if (!room.TryToGetPosition<Square>(attacker.IndexInHouse, out var fromPos))
            return false;

        foreach (var m in attacker.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            var enumerator = m.CreateEnumeratorFor(attacker, fromPos);
            if (enumerator == null) continue;

            var positions = enumerator.PositionsAll;
            if (!positions.ContainsKey(target)) continue;

            var visited = new HashSet<Square> { fromPos };
            var parent = new Dictionary<Square, Square>();
            var queue = new Queue<Square>();
            queue.Enqueue(fromPos);
            bool found = false;
            while (queue.Count > 0 && !found)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in enumerator.GetNeighbors(current))
                {
                    if (!visited.Add(neighbor)) continue;
                    if (!positions.ContainsKey(neighbor)) continue;
                    parent[neighbor] = current;
                    if (neighbor.Equals(target)) { found = true; break; }
                    queue.Enqueue(neighbor);
                }
            }
            if (!found) continue;

            var positionsList = new List<Square>();
            var node = target;
            while (!node.Equals(fromPos))
            {
                positionsList.Add(node);
                node = parent[node];
            }
            positionsList.Add(fromPos);
            positionsList.Reverse();
            path = new PathSequence<Square>(positionsList.ToArray());
            return true;
        }
        return false;
    }

    private bool CanAttackInGeneral(IPiece attacker)
    {
        if (attacker.IsDead) return false;
        if (attacker.HasPromotedThisTurn) return false;
        if (attacker.AttackCharges <= 0) return false;
        if (_modifierHost.IsAttackBlocked(attacker)) return false;
        return true;
    }
}