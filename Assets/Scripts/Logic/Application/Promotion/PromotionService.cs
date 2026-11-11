#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;

public sealed class PromotionService
{
    private readonly IRegistry _registry;
    private readonly IHouse _house;
    private readonly PieceTemplateRegistry _templateRegistry;
    private readonly PieceFactory _pieceFactory;
    private readonly ModifierHost _modifierHost;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public PromotionService(
        IRegistry registry,
        IHouse house,
        PieceTemplateRegistry templateRegistry,
        PieceFactory pieceFactory,
        ModifierHost modifierHost,
        IEventBus eventBus,
        ILogger logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _templateRegistry = templateRegistry ?? throw new ArgumentNullException(nameof(templateRegistry));
        _pieceFactory = pieceFactory ?? throw new ArgumentNullException(nameof(pieceFactory));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<PromotionOption> GetOptions(IPiece piece)
    {
        if (piece == null || piece.IsDead) return Array.Empty<PromotionOption>();
        if (piece.Name != "Pawn") return Array.Empty<PromotionOption>();

        if (!_registry.TryToGetRoom(piece.IndexInHouse, out var room)) return Array.Empty<PromotionOption>();
        if (room is not Board board) return Array.Empty<PromotionOption>();
        if (!board.TryToGetPosition<Square>(piece.IndexInHouse, out var pos)) return Array.Empty<PromotionOption>();

        if (!IsOnFarEdge(pos, piece.Facing, board.Size)) return Array.Empty<PromotionOption>();

        var result = new List<PromotionOption>();
        var seen = new HashSet<BigInteger>();

        foreach (var other in _registry.AllPieces)
        {
            if (other.IndexInHouse == piece.IndexInHouse) continue;
            if (!other.Owner.Equals(piece.Owner)) continue;
            if (other.Name == "Pawn") continue;
            if (other.Name == "King") continue;
            if (!seen.Add(other.IndexInHouse)) continue;
            result.Add(BuildOption(other));
        }

        return result;
    }

    public bool Apply(IPiece pawn, PromotionOption option)
    {
        if (pawn == null) throw new ArgumentNullException(nameof(pawn));
        if (option == null) throw new ArgumentNullException(nameof(option));
        if (pawn.IsDead) return false;
        if (pawn.Name != "Pawn") return false;

        if (!_registry.TryToGetRoom(pawn.IndexInHouse, out var room) || room is not Board board) return false;
        if (!board.TryToGetPosition<Square>(pawn.IndexInHouse, out var pos)) return false;

        PieceTemplate template;
        try { template = _templateRegistry.Get(option.Name); }
        catch (Exception ex) { _logger.LogError($"Promotion: template '{option.Name}' not found: {ex.Message}"); return false; }

        var definition = new PieceDefinition(
            option.Name,
            pawn.Owner,
            option.MaxHp,
            option.AttackDamage,
            option.MaxAttackCharges,
            pawn.Facing,
            template.MovementFactories,
            template.Abilities,
            AttackEffect: null,
            Modifiers: null);

        var oldIndex = pawn.IndexInHouse;

        // Capture snapshot of stateful modifiers from template piece BEFORE removing anything
        var templateStateful = new List<ModifierState>();
        if (_registry.TryToGetPiece(option.TemplateIndex, out var templatePiece) && templatePiece != null)
        {
            foreach (var m in _modifierHost.GetModifiers(templatePiece.IndexInHouse))
            {
                if (ShapeModifierExtensions.IsPureShape(m)) continue;
                templateStateful.Add(new ModifierState(
                    m.ModifierName, m.MaxDuration, m.CurrentDuration, m.MaxCharges, m.CurrentCharges));
            }
        }

        // Remove old pawn
        if (!board.Remove(oldIndex))
        {
            _logger.LogError($"Promotion: failed to remove pawn from board at {pos}");
            return false;
        }
        _modifierHost.RemoveAll(oldIndex);

        // Create new piece with same IndexInHouse
        var newPiece = _pieceFactory.Create(oldIndex, definition);

        if (!board.Add(newPiece, pos, true, out var addErr))
        {
            _logger.LogError($"Promotion: failed to add {option.Name} at {pos}: {addErr}");
            return false;
        }

        // Copy stateful modifiers from template
        foreach (var ms in templateStateful)
        {
            IModifier copy;
            try { copy = ModifierFactory.Create(ms.Name); }
            catch (Exception ex) { _logger.LogWarning($"Promotion: cannot clone modifier '{ms.Name}': {ex.Message}"); continue; }
            if (!ModifierCompatibility.IsCompatibleWithPiece(copy, newPiece)) continue;

            var ctx = new ModifierContext(
                newPiece, _eventBus, _modifierHost,
                selfRemove: m => _modifierHost.RemoveModifier(newPiece.IndexInHouse, m),
                initialDuration: ms.MaxDuration,
                initialCharges: ms.MaxCharges);
            _modifierHost.AddModifier(newPiece.IndexInHouse, copy, ctx);
            copy.RestoreCharges(ms.CurrentCharges);
            copy.RestoreDuration(ms.CurrentDuration);
        }

        _eventBus.Publish(new PieceDeployedEvent<Square>(newPiece, pos, board));
        _logger.Log($"Promotion: {pawn.Name}#{oldIndex} -> {option.Name} at {pos}");

        try { pawn.Dispose(); } catch { /* best-effort cleanup */ }
        return true;
    }

    private static bool IsOnFarEdge(Square pos, FacingDirection facing, Square gridSize) => facing switch
    {
        FacingDirection.PlusX => pos.X == gridSize.X - 1,
        FacingDirection.MinusX => pos.X == 0,
        FacingDirection.PlusY => pos.Y == gridSize.Y - 1,
        FacingDirection.MinusY => pos.Y == 0,
        FacingDirection.PlusZ => pos.Z == gridSize.Z - 1,
        FacingDirection.MinusZ => pos.Z == 0,
        _ => false,
    };

    private PromotionOption BuildOption(IPiece template)
    {
        var modifiers = new List<ModifierState>();
        foreach (var m in _modifierHost.GetModifiers(template.IndexInHouse))
        {
            if (ShapeModifierExtensions.IsPureShape(m)) continue;
            modifiers.Add(new ModifierState(
                m.ModifierName, m.MaxDuration, m.CurrentDuration, m.MaxCharges, m.CurrentCharges));
        }

        return new PromotionOption(
            template.IndexInHouse,
            template.Name,
            template.AttackDamage,
            template.HealthNormal,
            template.MaxAttackCharges,
            modifiers);
    }
}