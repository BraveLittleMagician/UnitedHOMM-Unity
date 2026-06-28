#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

public interface IRoomWithoutRemove : IReadOnlyRoom
{
    public bool Add<TPos>(IPiece piece, TPos pos, bool fromAnotherRoom, [NotNullWhen(false)] out string? error) where TPos : struct;
    public bool Displace<TPos>(IPath<TPos> posits, IndexOfPlayer index, [NotNullWhen(false)] out string? error) where TPos : struct;
    
    public event Action<IPiece, IRoomWithoutRemove, bool>? PieceAdded;
}