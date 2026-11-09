#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public sealed class PieceMoveValidator : IMovementResolver
{
    private readonly Dictionary<string, IMovementResolver> _resolvers;
    private readonly ModifierHost _modifierHost;

    public PieceMoveValidator(Dictionary<string, IMovementResolver> resolvers, ModifierHost modifierHost)
    {
        _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
    }

    public bool TryFindPath(PieceModel piece, Vector3Int target, Vector3Int gridSize, [NotNullWhen(true)] out IPath<Square>? path)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (piece.HasMovedThisTurn) { path = null; return false; }
        if (_modifierHost.IsMovementBlocked(piece)) { path = null; return false; }
        if (!_resolvers.TryGetValue(piece.Name, out var resolver)) { path = null; return false; }
        return resolver.TryFindPath(piece, target, gridSize, out path);
    }

    public IReadOnlyList<Vector3Int> GetValidMoves(PieceModel piece, Vector3Int gridSize)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (piece.HasMovedThisTurn) return Array.Empty<Vector3Int>();
        if (_modifierHost.IsMovementBlocked(piece)) return Array.Empty<Vector3Int>();
        if (!_resolvers.TryGetValue(piece.Name, out var resolver))
            return Array.Empty<Vector3Int>();
        return resolver.GetValidMoves(piece, gridSize);
    }

    public IReadOnlyList<ShapeCell> GetShape(PieceModel piece, Vector3Int gridSize, bool includeOccupied = false)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (piece.HasMovedThisTurn) return Array.Empty<ShapeCell>();
        if (_modifierHost.IsMovementBlocked(piece)) return Array.Empty<ShapeCell>();
        if (!_resolvers.TryGetValue(piece.Name, out var resolver))
            return Array.Empty<ShapeCell>();
        return resolver.GetShape(piece, gridSize, includeOccupied);
    }

    public ISequenceEnumerator<Square>? CreateEnumerator(PieceModel piece)
    {
        if (piece == null) return null;
        if (!_resolvers.TryGetValue(piece.Name, out var resolver)) return null;
        return resolver.CreateEnumerator(piece);
    }
}