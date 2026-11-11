#nullable enable
using System;
using System.Collections.Generic;

public sealed record PieceTemplate
{
    public PieceTemplate(
        string name,
        int health,
        int attackDamage,
        int attackCharges,
        FacingDirection initialFacing,
        IReadOnlyList<IMovementFactory> movementFactories,
        IReadOnlyList<IAbility> abilities,
        IReadOnlyList<ModifierDto>? modifiers = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя шаблона не может быть пустым", nameof(name));
        if (health <= 0)
            throw new ArgumentOutOfRangeException(nameof(health), "Здоровье должно быть > 0");
        if (attackDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(attackDamage), "AttackDamage должен быть >= 0");
        if (attackCharges < 0)
            throw new ArgumentOutOfRangeException(nameof(attackCharges), "AttackCharges должен быть >= 0");

        Name = name;
        Health = health;
        AttackDamage = attackDamage;
        AttackCharges = attackCharges;
        InitialFacing = initialFacing;
        MovementFactories = movementFactories ?? throw new ArgumentNullException(nameof(movementFactories));
        Abilities = abilities ?? throw new ArgumentNullException(nameof(abilities));
        Modifiers = modifiers;
    }

    public string Name { get; }
    public int Health { get; }
    public int AttackDamage { get; }
    public int AttackCharges { get; }
    public FacingDirection InitialFacing { get; }
    public IReadOnlyList<IMovementFactory> MovementFactories { get; }
    public IReadOnlyList<IAbility> Abilities { get; }
    public IReadOnlyList<ModifierDto>? Modifiers { get; }
}