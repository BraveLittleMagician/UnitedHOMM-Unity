#nullable enable

using System;

public static class BoardSizeFactory
{
    public static Square FromConfig(ConfigOfBoard config)
    {
        int n = config.FieldSize;
        int wLayers = Math.Max(1, config.W);
        return config.Axes switch
        {
            MultipleAxesFromTwo.Two => new Square { X = n, Y = n, Z = 1, W = 1 },
            MultipleAxesFromTwo.Three => new Square { X = n, Y = n, Z = n, W = 1 },
            MultipleAxesFromTwo.Four => new Square { X = n, Y = n, Z = n, W = wLayers },
            _ => throw new ArgumentOutOfRangeException(nameof(config.Axes), config.Axes, "Неподдерживаемое число осей")
        };
    }
}