#nullable enable
using System.Numerics;

public sealed record AmbushRecord(
    BigInteger AttackerIndex,
    BigInteger VictimIndex,
    Square VictimCell,
    IdOfPlayer ValidFor);