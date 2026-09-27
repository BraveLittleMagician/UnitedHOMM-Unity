#nullable enable

using System;

public static class BoardSizeCorrector
{
    public const int MaxBoardSize = 4096;

    public static int CorrectSize(int proposedSize, int? minimalSize = null)
    {
        int correctedSize = proposedSize;
        if (correctedSize % 2 != 0) correctedSize++;

        if (minimalSize.HasValue && correctedSize < minimalSize.Value)
        {
            correctedSize = minimalSize.Value;
            if (correctedSize % 2 != 0) correctedSize++;
        }

        correctedSize = Math.Min(MaxBoardSize, correctedSize);

        return correctedSize;
    }
    public static ConfigOfBoard CorrectConfig(ConfigOfBoard config, int? minimalSize = null)
    {
        int correctedSize = CorrectSize(config.FieldSize, minimalSize);
        return config with { FieldSize = correctedSize };
    }
}