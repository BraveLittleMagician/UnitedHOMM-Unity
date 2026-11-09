#nullable enable
using UnityEngine;

public readonly struct ShapeCell
{
    public bool CanStop => Stayable == Stayable.Stay;
    public bool CanAttackFrom => Attackable == Attackable.Attack;
    public Vector3Int Cell { get; }
    public Stayable Stayable { get; }
    public Attackable Attackable { get; }

    public ShapeCell(Vector3Int cell, Stayable stayable, Attackable attackable = Attackable.NotAttack)
    {
        Cell = cell;
        Stayable = stayable;
        Attackable = attackable;
    }

}