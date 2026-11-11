#nullable enable
using System.Numerics;

public sealed record DoubleMoveRecord(
    BigInteger MoverIndex,
    Square From,
    Square Via,
    Square To,
    IdOfPlayer ValidFor);