#nullable enable

public static class AttackFactory
{
    public static IMeleeAttack CreateDamageMelee(int damage)
        => new MeleeAttack(new SubtractOperation(damage));

    public static IRangedAttack CreateDamageRanged(int damage, int range)
        => new RangedAttack(new SubtractOperation(damage), range);

    public static IMeleeAttack CreateHealMelee(int heal)
        => new MeleeAttack(new RecoverOperation(heal));

}