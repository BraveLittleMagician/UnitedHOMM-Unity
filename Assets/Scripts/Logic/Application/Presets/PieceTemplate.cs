#nullable enable

using System;
using System.Collections.Generic;

public sealed record PieceTemplate
{
    public PieceTemplate(string name, int health, IReadOnlyList<IMovementFactory> movementFactories, IReadOnlyList<IMeleeAttack> meleeAttacks, IReadOnlyList<IRangedAttack> rangedAttacks, IReadOnlyList<IAbility> abilities)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя шаблона не может быть пустым", nameof(name));

        if (health <= 0)
            throw new ArgumentOutOfRangeException(nameof(health), "Здоровье должно быть > 0");

        Name = name;
        Health = health;
        MovementFactories = movementFactories ?? throw new ArgumentNullException(nameof(movementFactories));
        MeleeAttacks = meleeAttacks ?? throw new ArgumentNullException(nameof(meleeAttacks));
        RangedAttacks = rangedAttacks ?? throw new ArgumentNullException(nameof(rangedAttacks));
        Abilities = abilities ?? throw new ArgumentNullException(nameof(abilities));
    }

    public string Name { get; }
    public int Health { get; }
    public IReadOnlyList<IMovementFactory> MovementFactories { get; }
    public IReadOnlyList<IMeleeAttack> MeleeAttacks { get; }
    public IReadOnlyList<IRangedAttack> RangedAttacks { get; }
    public IReadOnlyList<IAbility> Abilities { get; }
}