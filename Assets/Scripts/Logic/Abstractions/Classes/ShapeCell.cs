
#nullable enable

public readonly struct ShapeCell
{
    public Square Cell { get; }
    public Stayable Stayable { get; }
    public Attackable Attackable { get; }

    public bool CanStop => Stayable == Stayable.Stay;
    public bool CanAttackFrom => Attackable == Attackable.Attack;

    public ShapeCell(Square cell, Stayable stayable, Attackable attackable = Attackable.NotAttack)
    {
        Cell = cell;
        Stayable = stayable;
        Attackable = attackable;
    }
}