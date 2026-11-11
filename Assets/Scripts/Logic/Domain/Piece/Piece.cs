#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

public sealed class Piece : IPiece
{
    private readonly Properties _properties;
    private readonly List<IMovement> _movements = new();
    private readonly List<IMeleeAttack> _meleeAttacks = new();
    private readonly List<IRangedAttack> _rangedAttacks = new();
    private readonly List<IAbility> _abilities = new();
    private readonly IAttackEffect _attackEffect;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private bool _disposed;

    public Piece(BigInteger indexInHouse, IdOfPlayer owner, string name, int health, int attackDamage, int attackCharges, FacingDirection facing, IAttackEffect attackEffect, IEventBus eventBus, ILogger logger)
    {
        IndexInHouse = indexInHouse;
        Owner = owner;
        Name = name;
        Facing = facing;
        _attackEffect = attackEffect ?? throw new ArgumentNullException(nameof(attackEffect));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var healthProp = new Health(health);
        var damageProp = new AttackDamageProperty(attackDamage);
        var chargesProp = new AttackChargesProperty(attackCharges);

        _properties = new Properties();
        _properties.Add(healthProp);
        _properties.Add(damageProp);
        _properties.Add(chargesProp);

        healthProp.Changed += OnHealthChanged;
    }

    public BigInteger IndexInHouse { get; }
    public IdOfPlayer Owner { get; }
    public string Name { get; }

    public int Health => _properties.TryToGet<Health>(out var h) ? h.Value : 0;
    public int HealthNormal => _properties.TryToGet<Health>(out var h) ? h.NormalValue : 0;
    public int AttackDamage => _properties.TryToGet<AttackDamageProperty>(out var p) ? p.Value : 0;
    public int AttackCharges => _properties.TryToGet<AttackChargesProperty>(out var p) ? p.Value : 0;
    public int MaxAttackCharges => _properties.TryToGet<AttackChargesProperty>(out var p) ? p.NormalValue : 0;
    public bool IsDead => Health <= 0;

    public FacingDirection Facing { get; private set; }
    public IAttackEffect AttackEffect => _attackEffect;

    public bool HasTakenFirstActionOfGame { get; private set; }
    public bool HasMovedThisTurn { get; private set; }
    public bool HasPromotedThisTurn { get; private set; }
    public int TotalRotationDegreesThisTurn { get; private set; }

    public Properties Properties => _properties;

    public IReadOnlyList<IMovement> Movements => _movements;
    public IReadOnlyList<IMeleeAttack> MeleeAttacks => _meleeAttacks;
    public IReadOnlyList<IRangedAttack> RangedAttacks => _rangedAttacks;
    public IReadOnlyList<IAbility> Abilities => _abilities;

    private void OnHealthChanged(Property property, int oldValue)
    {
        var health = (Health)property;
        if (health.Value == 0 && oldValue > 0)
        {
            _logger.Log($"Фигура {this} погибла");
            _eventBus.Publish(new PieceDiedEvent(this));
        }
        else if (health.Value > 0 && oldValue == 0)
        {
            _logger.Log($"Фигура {this} ожила");
            _eventBus.Publish(new PieceRevivedEvent(this));
        }
    }

    public void AddMovement(IMovement movement) => _movements.Add(movement);
    public void AddMeleeAttack(IMeleeAttack attack) => _meleeAttacks.Add(attack);
    public void AddRangedAttack(IRangedAttack attack) => _rangedAttacks.Add(attack);
    public void AddAbility(IAbility ability) => _abilities.Add(ability);

    public void ApplyOperation(IOperation operation) => _properties.Apply(operation);

    public bool TakeDamage(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
        if (amount == 0 || IsDead) return false;
        if (!_properties.TryToGet<Health>(out var health)) return false;
        health.Subtract(amount);
        return IsDead;
    }

    public void RecoverHp(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Heal cannot be negative.");
        if (amount == 0) return;
        if (!_properties.TryToGet<Health>(out var health)) return;
        health.Recover(amount);
    }

    public bool TryConsumeAttackCharge()
    {
        if (!_properties.TryToGet<AttackChargesProperty>(out var charges)) return false;
        return charges.TryConsume();
    }

    public void RestoreCombat(int hp, int attackCharges)
    {
        if (hp < 0 || hp > HealthNormal)
            throw new ArgumentOutOfRangeException(nameof(hp), $"HP must be in [0..{HealthNormal}].");
        if (attackCharges < 0 || attackCharges > MaxAttackCharges)
            throw new ArgumentOutOfRangeException(nameof(attackCharges),
                $"Charges must be in [0..{MaxAttackCharges}].");
        if (_properties.TryToGet<Health>(out var health)) health.Set(hp);
        if (_properties.TryToGet<AttackChargesProperty>(out var charges)) charges.Restore(attackCharges);
    }

    public void RestoreFacing(FacingDirection facing)
    {
        if (Facing == facing) return;
        var old = Facing;
        Facing = facing;
        _eventBus.Publish(new FacingChangedEvent(this, old, facing));
    }

    public void MarkFirstActionOfGameTaken(Activity activity)
    {
        if (HasTakenFirstActionOfGame) return;
        HasTakenFirstActionOfGame = true;
        _eventBus.Publish(new PieceStartedActionEvent(this, activity ?? EmptyActivity.Instance));
    }

    public void MarkMovedThisTurn() => HasMovedThisTurn = true;

    public void AddRotationDegreesThisTurn(int degrees)
    {
        if (degrees < 0) throw new ArgumentOutOfRangeException(nameof(degrees), "Degrees must be non-negative.");
        TotalRotationDegreesThisTurn += degrees;
    }

    public void MarkPromotedThisTurn() => HasPromotedThisTurn = true;

    public void ResetTurnState()
    {
        HasMovedThisTurn = false;
        TotalRotationDegreesThisTurn = 0;
        HasPromotedThisTurn = false;
        // HasTakenFirstActionOfGame не сбрасывается: это «первое действие за игру».
    }

    public void RestoreFirstActionOfGameTaken(bool value) => HasTakenFirstActionOfGame = value;
    public void RestoreMovedThisTurn(bool value) => HasMovedThisTurn = value;
    public void RestorePromotedThisTurn(bool value) => HasPromotedThisTurn = value;

    public void RestoreTotalRotationDegrees(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
        TotalRotationDegreesThisTurn = value;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _properties.Dispose();
        foreach (var mov in _movements) (mov as IDisposable)?.Dispose();
        foreach (var ab in _abilities) { ab.Deactivate(); ab.Dispose(); }
        foreach (var ma in _meleeAttacks) (ma as IDisposable)?.Dispose();
        foreach (var ra in _rangedAttacks) (ra as IDisposable)?.Dispose();
        _movements.Clear();
        _meleeAttacks.Clear();
        _rangedAttacks.Clear();
        _abilities.Clear();
        _logger.Log($"Фигура {this} уничтожена");
    }

    public override string ToString() =>
        $"[{IndexInHouse}] {Owner} {Name} ({Health} hp, {AttackDamage} dmg, {AttackCharges}/{MaxAttackCharges} ch)";
}