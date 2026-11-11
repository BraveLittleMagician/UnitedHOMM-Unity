#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public sealed class ModifierHost : IDisposable
{
    private sealed class ModifierBucket
    {
        public readonly List<IModifier> Items = new();
    }

    private readonly Dictionary<BigInteger, ModifierBucket> _byIndex = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private bool _disposed;

    public ModifierHost(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
    }

    public ICombatService? CombatService { get; set; }
    public IRegistry? Registry { get; set; }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        foreach (var bucket in _byIndex.Values)
        {
            var items = bucket.Items;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var modifier = items[i];
                if (modifier.MaxDuration < 0) continue;
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

    public void AddModifier(BigInteger index, IModifier modifier, ModifierContext context)
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!_byIndex.TryGetValue(index, out var bucket))
        {
            bucket = new ModifierBucket();
            _byIndex[index] = bucket;
        }
        bucket.Items.Add(modifier);
        modifier.Attach(context);
    }

    public void RemoveAll(BigInteger index)
    {
        if (!_byIndex.TryGetValue(index, out var bucket)) return;
        foreach (var modifier in bucket.Items.ToArray())
            modifier.Detach();
        bucket.Items.Clear();
        _byIndex.Remove(index);
    }

    public void AttachModifiers(IPiece piece, IReadOnlyList<ModifierDto>? dtos)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (dtos == null || dtos.Count == 0) return;

        var seen = new HashSet<string>();
        var parsed = new List<(IModifier Modifier, ModifierDto Dto)>();

        foreach (var dto in dtos)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Name)) continue;
            if (!seen.Add(dto.Name))
            {
                _logger.LogWarning($"[MODIFIER] Duplicate '{dto.Name}' on {piece.Name}#{piece.IndexInHouse}, skipping.");
                continue;
            }
            int resolvedCharges = dto.Charges ?? -1;
            if (resolvedCharges == 0)
                throw new InvalidOperationException(
                    $"Modifier '{dto.Name}' on {piece.Name}#{piece.IndexInHouse} cannot have Charges=0.");
            if (resolvedCharges < -1)
                throw new InvalidOperationException(
                    $"Modifier '{dto.Name}' on {piece.Name}#{piece.IndexInHouse} has invalid Charges={resolvedCharges}.");

            IModifier modifier;
            try
            {
                modifier = ModifierFactory.Create(dto.Name, dto.Args);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"[MODIFIER] {ex.Message} Skipping '{dto.Name}' on {piece.Name}#{piece.IndexInHouse}.");
                continue;
            }

            if (modifier is IActiveModifier && dto.Charges == null)
                throw new InvalidOperationException(
                    $"Active modifier '{dto.Name}' on {piece.Name}#{piece.IndexInHouse} must have Charges specified.");

            parsed.Add((modifier, dto));
        }

        foreach (var (modifier, dto) in parsed)
        {
            if (!ModifierCompatibility.IsCompatibleWithPiece(modifier, piece))
            {
                _logger.LogWarning($"[MODIFIER] '{dto.Name}' not compatible with {piece.Name}#{piece.IndexInHouse}; skipping.");
                continue;
            }
            if (ShapeModifierExtensions.IsPureShape(modifier))
            {
                if (dto.Charges != null)
                    _logger.LogWarning($"[MODIFIER] '{dto.Name}' is pure-shape; Charges ignored.");
                if (dto.Duration != null)
                    _logger.LogWarning($"[MODIFIER] '{dto.Name}' is pure-shape; Duration ignored.");
            }
            var ctx = new ModifierContext(
                piece, _eventBus, this,
                selfRemove: m => RemoveModifier(piece.IndexInHouse, m),
                initialDuration: dto.Duration ?? -1,
                initialCharges: dto.Charges ?? -1);
            AddModifier(piece.IndexInHouse, modifier, ctx);
        }
    }

    public bool HasModifier<T>(BigInteger index) where T : IModifier
    {
        if (!_byIndex.TryGetValue(index, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is T) return true;
        return false;
    }

    public bool RemoveModifier(BigInteger index, IModifier modifier)
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (!_byIndex.TryGetValue(index, out var bucket)) return false;
        if (!bucket.Items.Remove(modifier)) return false;
        modifier.Detach();
        return true;
    }

    public bool TryGetModifier<T>(BigInteger index, [NotNullWhen(true)] out T? modifier) where T : class, IModifier
    {
        modifier = null;
        if (!_byIndex.TryGetValue(index, out var bucket)) return false;
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

    public int GetEffectiveDamageBonus(IPiece piece)
    {
        if (piece == null) return 0;
        if (!_byIndex.TryGetValue(piece.IndexInHouse, out var bucket)) return 0;
        int bonus = 0;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IAttackDamageModifier dmg)
                bonus += dmg.GetAttackDamageBonus(piece);
        return bonus;
    }

    public bool IsMovementBlocked(IPiece piece)
    {
        if (piece == null) return false;
        if (!_byIndex.TryGetValue(piece.IndexInHouse, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IMovementModifier mv && !mv.CanMove(piece)) return true;
        return false;
    }

    public bool IsAttackBlocked(IPiece piece)
    {
        if (piece == null) return false;
        if (!_byIndex.TryGetValue(piece.IndexInHouse, out var bucket)) return false;
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IAttackBlocker blocker && !blocker.CanAttack(piece)) return true;
        return false;
    }

    public IReadOnlyList<IModifier> GetModifiers(BigInteger index)
    {
        if (_byIndex.TryGetValue(index, out var bucket))
            return bucket.Items;
        return Array.Empty<IModifier>();
    }

    public IReadOnlyList<IShapeTransformModifier> GetShapeTransforms(BigInteger index)
    {
        if (!_byIndex.TryGetValue(index, out var bucket))
            return Array.Empty<IShapeTransformModifier>();
        var result = new List<IShapeTransformModifier>();
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IShapeTransformModifier shape)
                result.Add(shape);
        return result;
    }

    public IReadOnlyList<IShapeTransformModifier<TSequence, TPosition>> GetShapeTransforms<TSequence, TPosition>(BigInteger index)
        where TPosition : struct, ISquare<TPosition>
        where TSequence : notnull, ISequence<TPosition, TSequence>
    {
        if (!_byIndex.TryGetValue(index, out var bucket))
            return Array.Empty<IShapeTransformModifier<TSequence, TPosition>>();
        var result = new List<IShapeTransformModifier<TSequence, TPosition>>();
        var items = bucket.Items;
        for (int i = 0; i < items.Count; i++)
            if (items[i] is IShapeTransformModifier<TSequence, TPosition> shape)
                result.Add(shape);
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        foreach (var bucket in _byIndex.Values)
        {
            foreach (var modifier in bucket.Items)
                modifier.Detach();
            bucket.Items.Clear();
        }
        _byIndex.Clear();
    }
}