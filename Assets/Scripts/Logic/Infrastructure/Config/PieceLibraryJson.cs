#nullable enable

public sealed record PieceLibraryJson(PieceEntryJson[] Pieces);

public sealed record PieceEntryJson(string Name, string PathToPrefab);