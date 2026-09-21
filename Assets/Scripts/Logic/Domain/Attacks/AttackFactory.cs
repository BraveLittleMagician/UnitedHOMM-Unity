#nullable enable

public static class AttackFactory
{
    public static IMeleeAttack CreateDamageMelee(int damage)
        => new MeleeAttack(new SubtractOperation(damage));

    public static IRangedAttack CreateDamageRanged(int damage)
        => new RangedAttack(new SubtractOperation(damage));

    public static IMeleeAttack CreateHealMelee(int heal)
        => new MeleeAttack(new RecoverOperation(heal));

}