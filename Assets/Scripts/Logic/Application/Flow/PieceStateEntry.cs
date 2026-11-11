#nullable enable

using System;
using System.Collections.Generic;

internal readonly struct PieceStateEntry
{
    public int Health { get; }
    public int AttackCharges { get; }
    public FacingDirection Facing { get; }
    public bool HasMovedThisTurn { get; }
    public bool HasTakenFirstActionOfGame { get; }
    public bool HasPromotedThisTurn { get; }
    public int TotalRotationDegreesThisTurn { get; }
    public IReadOnlyList<ModifierState> Modifiers { get; }
    public IRoom? Room { get; }
    public Square? Position { get; }

    public PieceStateEntry(
        int health, int attackCharges, FacingDirection facing,
        bool hasMovedThisTurn, bool hasTakenFirstActionOfGame,
        bool hasPromotedThisTurn, int totalRotationDegreesThisTurn,
        IReadOnlyList<ModifierState> modifiers,
        IRoom? room, Square? position)
    {
        Health = health;
        AttackCharges = attackCharges;
        Facing = facing;
        HasMovedThisTurn = hasMovedThisTurn;
        HasTakenFirstActionOfGame = hasTakenFirstActionOfGame;
        HasPromotedThisTurn = hasPromotedThisTurn;
        TotalRotationDegreesThisTurn = totalRotationDegreesThisTurn;
        Modifiers = modifiers;
        Room = room;
        Position = position;
    }

    public bool EqualsState(PieceStateEntry other) =>
        Health == other.Health &&
        AttackCharges == other.AttackCharges &&
        Facing == other.Facing &&
        HasMovedThisTurn == other.HasMovedThisTurn &&
        HasTakenFirstActionOfGame == other.HasTakenFirstActionOfGame &&
        HasPromotedThisTurn == other.HasPromotedThisTurn &&
        TotalRotationDegreesThisTurn == other.TotalRotationDegreesThisTurn &&
        ReferenceEquals(Room, other.Room) &&
        Nullable.Equals(Position, other.Position) &&
        ModifiersEqual(Modifiers, other.Modifiers);

    private static bool ModifiersEqual(IReadOnlyList<ModifierState> a, IReadOnlyList<ModifierState> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Name != b[i].Name) return false;
            if (a[i].CurrentCharges != b[i].CurrentCharges) return false;
            if (a[i].CurrentDuration != b[i].CurrentDuration) return false;
        }
        return true;
    }
}
