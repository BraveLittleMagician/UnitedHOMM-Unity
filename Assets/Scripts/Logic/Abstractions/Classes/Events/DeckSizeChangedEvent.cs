#nullable enable

public sealed record DeckSizeChangedEvent(int NewSize, IndexOfPlayer Owner);