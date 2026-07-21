#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public abstract class Room : IRoom, IDisposable
{
    private bool _disposed;

    protected Room(Seats seats, IEventBus eventBus, ILogger logger)
    {
        ActiveSeats = seats;
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected IEventBus EventBus { get; }
    protected ILogger Logger { get; }

    public Seats ActiveSeats { get; }
    public abstract string Name { get; }
    public abstract int CountOfPieces { get; }

    protected virtual void OnPieceAdded(IPiece piece, bool fromAnotherRoom)
    {
        PieceAdded?.Invoke(piece, this, fromAnotherRoom);
    }
    protected virtual void OnPieceRemoved(BigInteger index)
    {
        PieceRemoved?.Invoke(index, this);
    }


    public abstract bool Add<TPos>(IPiece piece, TPos pos, bool fromAnotherRoom, out string error) where TPos : struct;
    public abstract bool Displace<TPos>(IPath<TPos> path, IndexOfPlayer player, out string error) where TPos : struct;
    public abstract bool TryToGetPosition<TPos>(BigInteger index, out TPos pos) where TPos : struct;
    public abstract bool TryToGetPiece<TPos>(TPos position, IndexOfPlayer player, [NotNullWhen(true)] out IPiece? piece) where TPos : struct;
    public abstract bool TryToGetPiecesForPlayer(IndexOfPlayer player, out IEnumerable<IPiece> pieces);
    public abstract bool Remove(BigInteger index);
    public abstract bool Remove<TPos>(TPos position, IndexOfPlayer player) where TPos : struct;
    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        PieceAdded = null;
        PieceRemoved = null;
        GC.SuppressFinalize(this);
    }
    public abstract IEnumerable<IPiece> GetAllPieces();

    public event Action<IPiece, IRoomWithoutRemove, bool>? PieceAdded;
    public event Action<BigInteger, IRoom>? PieceRemoved;
}