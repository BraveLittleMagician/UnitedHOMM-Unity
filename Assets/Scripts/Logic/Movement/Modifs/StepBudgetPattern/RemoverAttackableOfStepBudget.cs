#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed class RemoverAttackableOfStepBudget : ModificatorOfStepBudget
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<Square> Squares { get; }

    public RemoverAttackableOfStepBudget(IEnumerable<Square> squares)
    {
        Squares = squares.ToHashSet();
    }

    public override StepBudgetPattern Apply(StepBudgetPattern sequence)
        => sequence.WithRemovedAttackable(Squares);
}