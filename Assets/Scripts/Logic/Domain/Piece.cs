#nullable enable


using System;
using System.Collections.Generic;
using System.Numerics;

public sealed class Piece : IPiece
{
    private readonly Properties _properties;
    private readonly List<IMovement> _movements = new();
    private readonly List<IAttack> _meleeAttacks = new();
    private readonly List<IAttack> _rangedAttacks = new();
    private readonly List<IAbility> _abilities = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private bool _disposed;

    public Piece(BigInteger indexInHouse, IndexOfPlayer owner, string name, int health, IEventBus eventBus, ILogger logger)
    {
        IndexInHouse = indexInHouse;
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
    public IndexOfPlayer Owner { get; }
    public string Name { get; }
    public int Health => _properties.TryToGet<Health>(out var health) ? health.Value : 0;
    public int HealthNormal => _properties.TryToGet<Health>(out var health) ? health.NormalValue : 0;
    public IReadOnlyList<IMovement> Movements => _movements;
    public IReadOnlyList<IAttack> MeleeAttacks => _meleeAttacks;
    public IReadOnlyList<IAttack> RangedAttacks => _rangedAttacks;
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

    public void AddMovement(IMovement movement) => _movements.Add(movement);
    public void AddMeleeAttack(IMeleeAttack attack) => _meleeAttacks.Add(attack);
    public void AddRangedAttack(IRangedAttack attack) => _rangedAttacks.Add(attack);
    public void AddAbility(IAbility ability) => _abilities.Add(ability);
    public void ApplyOperation(IOperation operation) => _properties.Apply(operation);
    public void ActivateAbilities()
    {
        foreach (var ability in _abilities)
            ability.Activate(this);
    }
    public void DeactivateAbilities()
    {
        foreach (var ability in _abilities)
            ability.Deactivate();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var mov in _movements) (mov as IDisposable)?.Dispose();
        foreach (var atk in _meleeAttacks) (atk as IDisposable)?.Dispose();
        foreach (var atk in _rangedAttacks) (atk as IDisposable)?.Dispose();
        foreach (var ab in _abilities) { ab.Deactivate(); ab.Dispose(); }

        _movements.Clear();
        _meleeAttacks.Clear();
        _rangedAttacks.Clear();
        _abilities.Clear();

        _logger.Log($"Фигура {this} уничтожена");
    }
    public override string ToString() => $"[{IndexInHouse}] {Owner} {Name} ({Health} hp)";
}