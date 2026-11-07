#nullable enable

using System.Collections.Generic;

public static class RookRules
{
    public static IReadOnlyList<MoveRule> Create()
    {
        return new[]
        {
            SingleSlide( 1,  0,  0),
            SingleSlide(-1,  0,  0),
            SingleSlide( 0,  1,  0),
            SingleSlide( 0, -1,  0),
            SingleSlide( 0,  0,  1),
            SingleSlide( 0,  0, -1),
        };
    }

    internal static MoveRule SingleSlide(int dx, int dy, int dz) => new (new[]
    {
        new RuleStep(new Square { X = dx, Y = dy, Z = dz }, Stayable.Stay, repeat: -1, attackable: Attackable.Attack),
    });
}