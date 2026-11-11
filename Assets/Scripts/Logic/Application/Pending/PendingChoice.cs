#nullable enable

using System.Collections.Generic;

public sealed record PendingChoice(
    IPiece Actor,
    Square Cell,
    IReadOnlyList<CellActionOption> Options) : PendingModal;