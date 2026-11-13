#nullable enable
using System.Collections.Generic;
using System.Numerics;

internal sealed class WorldDelta : IUndoableOperation
{
    private readonly WorldState _before;
    private readonly WorldState _after;
    private readonly IRegistry _registry;
    private readonly ModifierHost _modifierHost;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public WorldDelta(
        WorldState before, WorldState after,
        IRegistry registry, ModifierHost modifierHost,
        IEventBus eventBus, ILogger logger)
    {
        _before = before;
        _after = after;
        _registry = registry;
        _modifierHost = modifierHost;
        _eventBus = eventBus;
        _logger = logger;
    }

    public bool Undo() => Restore(_before);
    public bool Redo() => Restore(_after);

    private bool Restore(WorldState target)
    {
        var targetEntries = target.Entries;
        var savedPieces = new Dictionary<BigInteger, IPiece>(targetEntries.Count);
        foreach (var (index, _) in targetEntries)
        {
            if (_registry.TryToGetPiece(index, out var piece) && piece != null)
                savedPieces[index] = piece;
        }

        foreach (var (index, entry) in targetEntries)
        {
            if (!savedPieces.ContainsKey(index)) continue;
            if (!NeedsRoomChange(index, entry)) continue;
            if (_registry.TryToGetRoom(index, out var currentRoom) && currentRoom != null)
                currentRoom.Remove(index);
        }

        foreach (var (index, entry) in targetEntries)
        {
            if (!savedPieces.TryGetValue(index, out var piece)) continue;
            RestorePiece(piece, entry);
        }

        bool ok = true;
        foreach (var (index, entry) in targetEntries)
        {
            if (entry.Room == null || !entry.Position.HasValue) continue;
            if (!savedPieces.TryGetValue(index, out var piece)) continue;

            var room = entry.Room;
            var pos = entry.Position.Value;

            if (_registry.TryToGetRoom(index, out var currentRoom)
                && ReferenceEquals(currentRoom, room)
                && currentRoom is ISquarePositionRoom sq
                && sq.TryToGetPosition<Square>(index, out var currentPos)
                && currentPos.Equals(pos))
            {
                continue;
            }

            if (!room.Add(piece, pos, true, out var err))
            {
                _logger.LogError($"[UNDO] Failed to restore {piece} into {room.Name} at {pos}: {err}");
                ok = false;
                continue;
            }
            _eventBus.Publish(new PieceDeployedEvent<Square>(piece, pos, room));
        }

        _logger.LogDebug($"[UNDO] World restored, {(ok ? "OK" : "with errors")}");
        return ok;
    }


    private bool NeedsRoomChange(BigInteger index, PieceStateEntry target)
    {
        if (target.Room == null || !target.Position.HasValue)
            return _registry.TryToGetRoom(index, out _);

        if (!_registry.TryToGetRoom(index, out var currentRoom) || currentRoom == null)
            return false;

        if (!ReferenceEquals(currentRoom, target.Room))
            return true;

        if (currentRoom is ISquarePositionRoom sqRoom
            && sqRoom.TryToGetPosition<Square>(index, out var currentPos))
        {
            return !currentPos.Equals(target.Position.Value);
        }
        return false;
    }

    private void RestorePiece(IPiece piece, PieceStateEntry entry)
    {
        piece.RestoreCombat(entry.Health, entry.AttackCharges);
        piece.RestoreFacing(entry.Facing);
        piece.RestoreMovedThisTurn(entry.HasMovedThisTurn);
        piece.RestoreFirstActionOfGameTaken(entry.HasTakenFirstActionOfGame);
        piece.RestorePromotedThisTurn(entry.HasPromotedThisTurn);
        piece.RestoreTotalRotationDegrees(entry.TotalRotationDegreesThisTurn);
        RestoreModifiers(piece, entry.Modifiers);
    }

    private void RestoreModifiers(IPiece piece, IReadOnlyList<ModifierState> saved)
    {
        var current = _modifierHost.GetModifiers(piece.IndexInHouse);
        var currentStateful = new List<IModifier>(current.Count);
        for (int i = 0; i < current.Count; i++)
            if (!ShapeModifierExtensions.IsPureShape(current[i]))
                currentStateful.Add(current[i]);

        bool identityMatches = currentStateful.Count == saved.Count;
        if (identityMatches)
        {
            for (int i = 0; i < currentStateful.Count; i++)
            {
                if (currentStateful[i].ModifierName != saved[i].Name) { identityMatches = false; break; }
            }
        }

        if (!identityMatches)
        {
            for (int i = 0; i < currentStateful.Count; i++)
                _modifierHost.RemoveModifier(piece.IndexInHouse, currentStateful[i]);

            if (saved.Count > 0)
            {
                var dtos = new ModifierDto[saved.Count];
                for (int i = 0; i < saved.Count; i++)
                {
                    dtos[i] = new ModifierDto(
                        saved[i].Name,
                        Duration: saved[i].MaxDuration >= 0 ? saved[i].MaxDuration : (int?)null,
                        Charges: saved[i].MaxCharges >= 0 ? saved[i].MaxCharges : (int?)null);
                }
                _modifierHost.AttachModifiers(piece, dtos);
            }
        }

        var final = _modifierHost.GetModifiers(piece.IndexInHouse);
        var finalStateful = new List<IModifier>(final.Count);
        for (int i = 0; i < final.Count; i++)
            if (!ShapeModifierExtensions.IsPureShape(final[i]))
                finalStateful.Add(final[i]);

        int count = System.Math.Min(finalStateful.Count, saved.Count);
        for (int i = 0; i < count; i++)
        {
            finalStateful[i].RestoreCharges(saved[i].CurrentCharges);
            finalStateful[i].RestoreDuration(saved[i].CurrentDuration);
        }
    }
}