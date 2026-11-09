#nullable enable

using System.Collections.Generic;
using System.Linq;

public class AdderAttackableOfStepBudget : ModificatorOfStepBudget
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<Square> Squares { get; }
    public AdderAttackableOfStepBudget(IEnumerable<Square> squares) => Squares = squares.ToHashSet();

    public override StepBudgetPattern Apply(StepBudgetPattern sequence)
        => sequence.WithAddedAttackable(Squares, Attackable.Attack);
}