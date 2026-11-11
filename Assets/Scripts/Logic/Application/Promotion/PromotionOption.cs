#nullable enable
using System.Collections.Generic;
using System.Numerics;

public sealed record PromotionOption(
    BigInteger TemplateIndex,
    string Name,
    int AttackDamage,
    int MaxHp,
    int MaxAttackCharges,
    IReadOnlyList<ModifierState> Modifiers);