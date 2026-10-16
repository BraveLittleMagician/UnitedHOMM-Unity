#nullable enable

public sealed record DeckSizeChangedEvent(int NewSize, IdOfPlayer Owner);