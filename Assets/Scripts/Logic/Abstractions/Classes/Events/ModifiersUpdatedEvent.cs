#nullable enable

using System;

public sealed record ModifiersUpdatedEvent(IPiece Piece, bool IsAttack, Type SequenceType);