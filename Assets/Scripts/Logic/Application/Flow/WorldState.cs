#nullable enable

using System.Collections.Generic;
using System.Numerics;

internal sealed class WorldState
{
    private readonly Dictionary<BigInteger, PieceStateEntry> _entries;

    private WorldState(Dictionary<BigInteger, PieceStateEntry> entries)
    {
        _entries = entries;
    }
    
    private static IReadOnlyList<ModifierState> CaptureModifiers(IPiece piece, ModifierHost host)
    {
        var modifiers = host.GetModifiers(piece.IndexInHouse);
        var result = new List<ModifierState>(modifiers.Count);
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (ShapeModifierExtensions.IsPureShape(modifiers[i])) continue;
            var m = modifiers[i];
            result.Add(new ModifierState(m.ModifierName, m.MaxDuration, m.CurrentDuration, m.MaxCharges, m.CurrentCharges));
        }
        return result;
    }

    public bool HasChanges(WorldState after)
    {
        if (_entries.Count != after._entries.Count) return true;
        foreach (var (index, before) in _entries)
        {
            if (!after._entries.TryGetValue(index, out var afterEntry)) return true;
            if (!before.EqualsState(afterEntry)) return true;
        }
        return false;
    }
    public IReadOnlyDictionary<BigInteger, PieceStateEntry> Entries => _entries;
    public static WorldState Capture(IRegistry registry, ModifierHost modifierHost)
    {
        var dict = new Dictionary<BigInteger, PieceStateEntry>();
        foreach (var piece in registry.AllPieces)
        {
            var modifiers = CaptureModifiers(piece, modifierHost);

            IRoom? room = null;
            Square? pos = null;
            if (registry.TryToGetRoom(piece.IndexInHouse, out var r) && r is ISquarePositionRoom sqRoom && sqRoom.TryToGetPosition<Square>(piece.IndexInHouse, out var p))
            {
                room = r;
                pos = p;
            }

            dict[piece.IndexInHouse] = new PieceStateEntry(
                piece.Health, piece.AttackCharges, piece.Facing,
                piece.HasMovedThisTurn, piece.HasTakenFirstActionOfGame,
                piece.HasPromotedThisTurn, piece.TotalRotationDegreesThisTurn,
                modifiers, room, pos);
        }
        return new WorldState(dict);
    }
}