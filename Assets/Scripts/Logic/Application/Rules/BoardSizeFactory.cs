#nullable enable

using System;

public static class BoardSizeFactory
{
    public static Square FromConfig(ConfigOfBoard config)
    {
        int n = config.FieldSize;
        int wLayers = 1 + (config.WDown ? 1 : 0) + (config.WUp ? 1 : 0);

        return config.Axes switch
        {
            MultipleAxesFromTwo.Two => new Square { X = n, Y = n, Z = 1, W = 1 },
            MultipleAxesFromTwo.Three => new Square { X = n, Y = n, Z = n, W = 1 },
            MultipleAxesFromTwo.Four => new Square { X = n, Y = n, Z = n, W = wLayers },
            _ => throw new ArgumentOutOfRangeException(nameof(config.Axes), config.Axes, "Неподдерживаемое число осей")
        };
    }
}