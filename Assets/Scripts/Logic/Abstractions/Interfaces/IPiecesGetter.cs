#nullable enable

using System.Collections.Generic;

public interface IPiecesGetter
{
    public IEnumerable<IPiece> GetAllPieces();
}