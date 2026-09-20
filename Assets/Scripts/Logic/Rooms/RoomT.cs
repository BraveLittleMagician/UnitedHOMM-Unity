#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

public abstract class RoomT<TPosition> : Room, IRoomT<TPosition> where TPosition : struct
{
    protected readonly Dictionary<TPosition, IPiece> _pieces = new();

    protected RoomT(Seats seats, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger) { }

    public override int CountOfPieces => _pieces.Count;
    public override string Name => GetType().Name;

    protected IReadOnlyDictionary<TPosition, IPiece> Pieces => _pieces;
    protected abstract bool ValidateAdd(IPiece piece, TPosition position, out string error);
    protected abstract bool ValidateDisplace(IPath<TPosition> path, IPiece piece, out string error);

    public override bool Add<TPos>(IPiece piece, TPos pos, bool fromAnotherRoom, out string error)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        if (pos is not TPosition typedPos)
        {
            error = $"Неверный тип позиции: ожидается {typeof(TPosition).Name}, получен {typeof(TPos).Name}";
            return false;
        }
        if (_pieces.ContainsValue(piece))
        {
            error = $"Фигура {piece} уже находится в комнате {Name}";
            return false;
        }
        if (_pieces.ContainsKey(typedPos))
        {
            error = $"Позиция {typedPos} уже занята";
            return false;
        }
        if (!ValidateAdd(piece, typedPos, out error)) return false;

        _pieces[typedPos] = piece;
        OnPieceAdded(piece, fromAnotherRoom);

        error = "";
        return true;
    }
    public override bool Remove(BigInteger index)
    {
        TPosition? pos = null;
        IPiece? piece = null;
        foreach (var pair in _pieces)
        {
            if (pair.Value.IndexInHouse == index)
            {
                pos = pair.Key;
                piece = pair.Value;
                break;
            }
        }

        if (piece != null && pos.HasValue)
            return Remove(pos.Value, piece.Owner);
        return false;
    }
    public override bool Remove<TPos>(TPos position, IndexOfPlayer player)
    {
        if (position is not TPosition typedPos) return false;
        if (!_pieces.TryGetValue(typedPos, out var piece)) return false;
        if (!piece.Owner.Equals(player)) return false;

        _pieces.Remove(typedPos);
        OnPieceRemoved(piece.IndexInHouse);
        return true;
    }
    public override bool Displace<TPos>(IPath<TPos> path, IndexOfPlayer player, out string error)
    {
        if (path is not IPath<TPosition> typedPath)
        {
            error = $"Неверный тип пути: ожидается {typeof(IPath<TPosition>).Name}";
            return false;
        }

        var start = typedPath.Positions[0];
        var end = typedPath.Positions[^1];

        if (!_pieces.TryGetValue(start, out var piece) || !piece.Owner.Equals(player))
        {
            error = $"На стартовой позиции {start} нет фигуры игрока {player}";
            return false;
        }

        if (!ValidateDisplace(typedPath, piece, out error))
            return false;

        _pieces.Remove(start);
        _pieces[end] = piece;

        error = "";
        return true;
    }
    public override bool TryToGetPiece<TPos>(TPos position, [NotNullWhen(true)] out IPiece? piece)
    {
        if (position is TPosition typedPos && _pieces.TryGetValue(typedPos, out var p) && p != null)
        {
            piece = p;
            return true;
        }
        piece = null;
        return false;
    }
    public override bool TryToGetPiecesForPlayer(IndexOfPlayer player, out IEnumerable<IPiece> pieces)
    {
        var list = _pieces.Values.Where(p => p.Owner.Equals(player)).ToList();
        pieces = list;
        return list.Count > 0;
    }
    public override void Dispose()
    {
        foreach (var piece in _pieces.Values)
            piece.Dispose();
        _pieces.Clear();
        base.Dispose();
    }
    public override IEnumerable<IPiece> GetAllPieces() => _pieces.Values.ToList();
}