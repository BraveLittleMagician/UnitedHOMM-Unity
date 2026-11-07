#nullable enable
using System;
using System.Collections.Generic;

public sealed class MoveRule
{
    public IReadOnlyList<RuleStep> Steps { get; }

    public MoveRule(IReadOnlyList<RuleStep> steps)
    {
        if (steps == null || steps.Count == 0)
            throw new ArgumentException("Rule must have at least one step.", nameof(steps));

        for (int i = 0; i < steps.Count - 1; i++)
        {
            if (steps[i].Repeat == -1)
                throw new ArgumentException(
                    $"Only the last step can have Repeat=-1. Step {i} violates this.");
        }

        if (steps[^1].Stayable != Stayable.Stay)
            throw new ArgumentException(
                "The last step of a rule must be Stayable.Stay (guarantee of stopping).");

        Steps = steps;
    }
}