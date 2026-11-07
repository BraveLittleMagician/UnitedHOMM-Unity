#nullable enable

using System;

public sealed record ModifiersUpdatedEvent(IPiece Piece, Type SequenceType);