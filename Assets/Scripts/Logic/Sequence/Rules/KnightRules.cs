#nullable enable

using System.Collections.Generic;

public static class KnightRules
{
    public static IReadOnlyList<MoveRule> Create()
    {
        return new[]
        {
            KnightMove( 0,  1, -1,  1,  2,  0),
            KnightMove( 0,  1, -1, -1,  2,  0),
            KnightMove( 0, -1, -1,  1, -2,  0),
            KnightMove( 0, -1, -1, -1, -2,  0),
            KnightMove( 1,  0, -1,  2, -1,  0),
            KnightMove( 1,  0, -1,  2,  1,  0),
            KnightMove(-1,  0, -1, -2, -1,  0),
            KnightMove(-1,  0, -1, -2,  1,  0),
            KnightMove( 0,  1,  1,  1,  2,  0),
            KnightMove( 0,  1,  1, -1,  2,  0),
            KnightMove( 0, -1,  1,  1, -2,  0),
            KnightMove( 0, -1,  1, -1, -2,  0),
            KnightMove( 1,  0,  1,  2, -1,  0),
            KnightMove( 1,  0,  1,  2,  1,  0),
            KnightMove(-1,  0,  1, -2, -1,  0),
            KnightMove(-1,  0,  1, -2,  1,  0),
            KnightMove(-1, -1,  0, -2,  0,  1),
            KnightMove(-1, -1,  0, -2,  0, -1),
            KnightMove( 1, -1,  0,  2,  0,  1),
            KnightMove( 1, -1,  0,  2,  0, -1),
            KnightMove( 0, -1, -1, -1,  0, -2),
            KnightMove( 0, -1, -1,  1,  0, -2),
            KnightMove( 0, -1,  1, -1,  0,  2),
            KnightMove( 0, -1,  1,  1,  0,  2),
            KnightMove(-1,  1,  0, -2,  0,  1),
            KnightMove(-1,  1,  0, -2,  0, -1),
            KnightMove( 1,  1,  0,  2,  0,  1),
            KnightMove( 1,  1,  0,  2,  0, -1),
            KnightMove( 0,  1, -1, -1,  0, -2),
            KnightMove( 0,  1, -1,  1,  0, -2),
            KnightMove( 0,  1,  1, -1,  0,  2),
            KnightMove( 0,  1,  1,  1,  0,  2),
            KnightMove( 1,  0,  1,  0,  1,  2),
            KnightMove( 1,  0,  1,  0, -1,  2),
            KnightMove( 1,  0, -1,  0,  1, -2),
            KnightMove( 1,  0, -1,  0, -1, -2),
            KnightMove( 1,  1,  0,  0,  2, -1),
            KnightMove( 1,  1,  0,  0,  2,  1),
            KnightMove( 1, -1,  0,  0, -2, -1),
            KnightMove( 1, -1,  0,  0, -2,  1),
            KnightMove(-1,  0,  1,  0,  1,  2),
            KnightMove(-1,  0,  1,  0, -1,  2),
            KnightMove(-1,  0, -1,  0,  1, -2),
            KnightMove(-1,  0, -1,  0, -1, -2),
            KnightMove(-1,  1,  0,  0,  2, -1),
            KnightMove(-1,  1,  0,  0,  2,  1),
            KnightMove(-1, -1,  0,  0, -2, -1),
            KnightMove(-1, -1,  0,  0, -2,  1),
        };
    }
    private static MoveRule KnightMove(int wx, int wy, int wz, int dx, int dy, int dz)
    {
        return new MoveRule(new[]
        {
        new RuleStep(new Square { X = wx, Y = wy, Z = wz }, Stayable.NotStay, repeat: 1),
        new RuleStep(
            new Square { X = dx - wx, Y = dy - wy, Z = dz - wz },
            Stayable.Stay, repeat: 1, attackable: Attackable.Attack),
    });
    }
}