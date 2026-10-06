#nullable enable

using System.Collections.Generic;

public sealed record BoardLayoutResult(ConfigOfBoard Config, IReadOnlyList<StartingPlacement> Placements);