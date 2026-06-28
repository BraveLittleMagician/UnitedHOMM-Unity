#nullable enable


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public interface IReadOnlyRoom : IPiecesGetter, IDisposable
{
    public string Name { get; }
    public int CountOfPieces { get; }
    public Seats ActiveSeats { get; }

    public bool TryToGetPiece<TPos>(TPos position, IndexOfPlayer player, [NotNullWhen(true)] out IPiece? piece) where TPos : struct;
    public bool TryToGetPiecesForPlayer(IndexOfPlayer player, out IEnumerable<IPiece> pieces);
}