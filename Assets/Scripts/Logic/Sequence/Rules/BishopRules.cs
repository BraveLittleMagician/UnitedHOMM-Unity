#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;

public static class BishopRules
{
    public static IReadOnlyList<MoveRule> Create()
    {
        return new[]
        {
            RookRules.SingleSlide( 1,  1,  0),
            RookRules.SingleSlide( 1, -1,  0),
            RookRules.SingleSlide(-1,  1,  0),
            RookRules.SingleSlide(-1, -1,  0),
            RookRules.SingleSlide( 1,  0,  1),
            RookRules.SingleSlide( 1,  0, -1),
            RookRules.SingleSlide(-1,  0,  1),
            RookRules.SingleSlide(-1,  0, -1),
            RookRules.SingleSlide( 0,  1,  1),
            RookRules.SingleSlide( 0,  1, -1),
            RookRules.SingleSlide( 0, -1,  1),
            RookRules.SingleSlide( 0, -1, -1),
        };
    }
}