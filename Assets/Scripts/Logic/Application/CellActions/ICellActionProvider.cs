#nullable enable
using System.Collections.Generic;

public interface ICellActionProvider
{
    IReadOnlyList<CellActionOption> GetActionsForCell(IPiece actor, Square cell);
    IReadOnlyList<Square> GetActionCells(IPiece actor);
}