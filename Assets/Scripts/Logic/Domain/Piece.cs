#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public sealed class Piece : IPiece
{
    private readonly Properties _properties;
    private readonly List<IMeleeAttack> _meleeAttacks = new();
    private readonly List<IRangedAttack> _rangedAttacks = new();
    private readonly List<IAbility> _abilities = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private bool _disposed;

    public Piece(
        BigInteger indexInHouse,
        long pieceId,
        IdOfPlayer owner,
        string name,
        int health,
        IEventBus eventBus,
        ILogger logger)
    {
        IndexInHouse = indexInHouse;
        PieceId = pieceId;
        Owner = owner;
        Name = name;
        _eventBus = eventBus;
        _logger = logger;

        var healthProp = new Health(health);
        _properties = new Properties();
        _properties.Add(healthProp);
        healthProp.Changed += OnHealthChanged;
    }

    public BigInteger IndexInHouse { get; }
    public long PieceId { get; }
    public IdOfPlayer Owner { get; }
    public string Name { get; }

    public int Health => _properties.TryToGet<Health>(out var health) ? health.Value : 0;
    public int HealthNormal => _properties.TryToGet<Health>(out var health) ? health.NormalValue : 0;

    public Vector3Int PositionInGrid { get; private set; }
    public FacingDirection Facing { get; private set; } = FacingDirection.PlusY;
    public bool HasMovedThisTurn { get; private set; }
    public bool HasPromotedThisTurn { get; private set; }

    public IReadOnlyList<IMeleeAttack> MeleeAttacks => _meleeAttacks;
    public IReadOnlyList<IRangedAttack> RangedAttacks => _rangedAttacks;
    public IReadOnlyList<IAbility> Abilities => _abilities;

    private void OnHealthChanged(Property property, int oldValue)
    {
        var health = (Health)property;
        if (health.Value == 0)
        {
            _logger.Log($"Фигура {this} погибла");
            _eventBus.Publish(new PieceDiedEvent(this));
        }
    }

    public void AddMeleeAttack(IMeleeAttack attack) => _meleeAttacks.Add(attack);
    public void AddRangedAttack(IRangedAttack attack) => _rangedAttacks.Add(attack);
    public void AddAbility(IAbility ability) => _abilities.Add(ability);
    public void ApplyOperation(IOperation operation) => _properties.Apply(operation);

    public void SetPosition(Vector3Int pos) => PositionInGrid = pos;
    public void SetFacing(FacingDirection facing) => Facing = facing;
    public void MarkMovedThisTurn() => HasMovedThisTurn = true;
    public void MarkPromotedThisTurn() => HasPromotedThisTurn = true;

    public void ResetTurnState()
    {
        HasMovedThisTurn = false;
        HasPromotedThisTurn = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _properties.Dispose();
        foreach (var ab in _abilities) { ab.Deactivate(); ab.Dispose(); }
        foreach (var ma in _meleeAttacks) (ma as IDisposable)?.Dispose();
        foreach (var ra in _rangedAttacks) (ra as IDisposable)?.Dispose();
        _meleeAttacks.Clear();
        _rangedAttacks.Clear();
        _abilities.Clear();
        _logger.Log($"Фигура {this} уничтожена");
    }

    public override string ToString() =>
        $"[{IndexInHouse}] {Owner} {Name} ({Health}/{HealthNormal} hp) " +
        $"@{PositionInGrid} facing={Facing} moved={HasMovedThisTurn} promoted={HasPromotedThisTurn}";
}