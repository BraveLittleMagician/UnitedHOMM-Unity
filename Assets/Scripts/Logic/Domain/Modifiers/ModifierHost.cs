#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public sealed class ModifierHost
{
    private sealed class ModifierBucket
    {
        public readonly List<IModifier> Items = new();
        public readonly ReadOnlyCollection<IModifier> ReadOnlyItems;

        public ModifierBucket()
        {
            ReadOnlyItems = Items.AsReadOnly();
        }
    }

    private readonly Dictionary<long, ModifierBucket> _byPieceId = new();
    private readonly PieceRegistry _registry;
    private readonly TurnManager _turnManager;

    public ModifierHost(PieceRegistry registry, TurnManager turnManager)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        _registry.ModelUnregistered += OnModelUnregistered;
        _turnManager.TurnStarted += OnTurnStarted;
    }

    public PieceRegistry Registry => _registry;

    private void OnModelUnregistered(PieceModel model)
    {
        if (model == null) return;
        RemoveAll(model.PieceId);
    }
    private void OnTurnStarted(IdOfPlayer player)
    {
        foreach (var model in _registry.AllLiveModels)
        {
            if (model.Player != player) continue;
            if (!_byPieceId.TryGetValue(model.PieceId, out var bucket)) continue;
            var items = bucket.Items;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var modifier = items[i];
                if (modifier.CurrentDuration < 0) continue;
                int newDuration = modifier.CurrentDuration - 1;
                if (newDuration <= 0)
                {
                    items.RemoveAt(i);
                    modifier.Detach();
                }
                else
                {
                    modifier.RestoreDuration(newDuration);
                }
            }
        }
    }

    public void AddModifier(long pieceId, IModifier modifier, ModifierContext context)
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!_byPieceId.TryGetValue(pieceId, out var bucket))
        {
            bucket = new ModifierBucket();
            _byPieceId[pieceId] = bucket;
        }
        bucket.Items.Add(modifier);
        modifier.Attach(context);
    }
    public void RemoveAll(long pieceId)
    {
        if (!_byPieceId.TryGetValue(pieceId, out var bucket)) return;
        foreach (var modifier in bucket.Items.ToArray())
            modifier.Detach();
        bucket.Items.Clear();
        _byPieceId.Remove(pieceId);
    }
    public void AttachModifiers(PieceModel model, IReadOnlyList<ModifierDto> modifiers, PieceEventBus eventBus)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));
        if (modifiers == null || modifiers.Count == 0) return;

        var seen = new HashSet<string>();
        var parsed = new List<(IModifier Modifier, ModifierDto Dto)>();

        foreach (var dto in modifiers)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Name)) continue;
            if (!seen.Add(dto.Name))
            {
                Debug.LogWarning($"[MODIFIER] Duplicate modifier '{dto.Name}' on {model.Name}#{model.PieceId}, skipping.");
                continue;
            }

            int resolvedCharges = dto.Charges ?? -1;
            if (resolvedCharges == 0)
                throw new InvalidOperationException(
                    $"Modifier '{dto.Name}' on {model.Name}#{model.PieceId} cannot have Charges=0.");
            if (resolvedCharges < -1)
                throw new InvalidOperationException(
                    $"Modifier '{dto.Name}' on {model.Name}#{model.PieceId} has invalid Charges={resolvedCharges}.");

            var modifier = ModifierFactory.Create(dto.Name, dto.Args);

            if (modifier is IActiveModifier && dto.Charges == null)
                throw new InvalidOperationException(
                    $"Active modifier '{dto.Name}' on {model.Name}#{model.PieceId} must have Charges specified.");
            if (modifier is OpeningDoubleMoveModifier && resolvedCharges > 1)
                throw new InvalidOperationException(
                    $"DoubleMove on {model.Name}#{model.PieceId}: Charges={resolvedCharges}, " +
                    $"но DoubleMove списывается только на первом действии за игру — " +
                    $"значения > 1 никогда не будут использованы.");

            parsed.Add((modifier, dto));
        }

        foreach (var (modifier, dto) in parsed)
        {
            if (!ModifierCompatibility.IsCompatibleWithPiece(modifier, model))
            {
                Debug.LogWarning(
                    $"[MODIFIER] '{dto.Name}' is not compatible with piece '{model.Name}'#{model.PieceId}; skipping.");
                continue;
            }

            if (ShapeModifierExtensions.IsPureShape(modifier))
            {
                if (dto.Charges != null)
                    Debug.LogWarning(
                        $"[MODIFIER] '{dto.Name}' on {model.Name}#{model.PieceId} is a pure shape modifier; 'Charges' is ignored.");
                if (dto.Duration != null)
                    Debug.LogWarning(
                        $"[MODIFIER] '{dto.Name}' on {model.Name}#{model.PieceId} is a pure shape modifier; 'Duration' is ignored.");
            }

            var ctx = new ModifierContext(
                model, eventBus, this,
                selfRemove: m => RemoveModifier(model.PieceId, m),
                initialDuration: dto.Duration ?? -1,
                initialCharges: dto.Charges ?? -1);
            AddModifier(model.PieceId, modifier, ctx);
        }
    }
    public void AttachShapeTransform(PieceModel model, IModifier modifier, PieceEventBus eventBus)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));
        var ctx = new ModifierContext(
            model, eventBus, this,
            selfRemove: m => RemoveModifier(model.PieceId, m),
            initialDuration: -1,
            initialCharges: -1);
        AddModifier(model.PieceId, modifier, ctx);
    }
    public bool IsMovementBlocked(PieceModel piece)
    {
        if (piece == null) return false;
        if (!_byPieceId.TryGetValue(piece.PieceId, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IMovementModifier mv && !mv.CanMove(piece)) return true;
        return false;
    }
    public bool IsAttackBlocked(PieceModel piece)
    {
        if (piece == null) return false;
        if (!_byPieceId.TryGetValue(piece.PieceId, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IAttackBlocker blocker && !blocker.CanAttack(piece))
                return true;
        return false;
    }
    public bool HasModifier<T>(long pieceId) where T : IModifier
    {
        if (!_byPieceId.TryGetValue(pieceId, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is T) return true;
        return false;
    }
    public bool RemoveModifier(long pieceId, IModifier modifier)
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (!_byPieceId.TryGetValue(pieceId, out var bucket)) return false;
        if (!bucket.Items.Remove(modifier)) return false;
        modifier.Detach();
        return true;
    }
    public bool TryGetModifier<T>(long pieceId, [NotNullWhen(true)] out T? modifier) where T : class, IModifier
    {
        modifier = null;
        if (!_byPieceId.TryGetValue(pieceId, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is T typed)
            {
                modifier = typed;
                return true;
            }
        }
        return false;
    }
    public int GetEffectiveAttackDamage(PieceModel piece)
    {
        if (piece == null) return 0;
        int total = piece.AttackDamage;
        if (!_byPieceId.TryGetValue(piece.PieceId, out var bucket)) return total;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is IAttackDamageModifier dmg)
                total += dmg.GetAttackDamageBonus(piece);
        }
        return total;
    }
    public IReadOnlyList<IModifier> GetModifiers(long pieceId)
    {
        if (_byPieceId.TryGetValue(pieceId, out var bucket))
            return bucket.ReadOnlyItems;
        return Array.Empty<IModifier>();
    }
    public IReadOnlyList<IShapeTransformModifier> GetShapeTransforms(long pieceId)
    {
        if (!_byPieceId.TryGetValue(pieceId, out var bucket))
            return Array.Empty<IShapeTransformModifier>();
        var result = new List<IShapeTransformModifier>();
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IShapeTransformModifier shape)
                result.Add(shape);
        return result;
    }
    public IReadOnlyList<IShapeTransformModifier<TSequence, TPosition>> GetShapeTransforms<TSequence, TPosition>(long pieceId) where TPosition : struct, ISquare<TPosition> where TSequence : notnull, ISequence<TPosition, TSequence>
    {
        if (!_byPieceId.TryGetValue(pieceId, out var bucket))
            return Array.Empty<IShapeTransformModifier<TSequence, TPosition>>();
        var result = new List<IShapeTransformModifier<TSequence, TPosition>>();
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IShapeTransformModifier<TSequence, TPosition> shape)
                result.Add(shape);
        return result;
    }
}