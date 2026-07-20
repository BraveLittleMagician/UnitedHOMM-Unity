#nullable enable

public static class BoardSizeCorrector
{
    public static int CorrectSize(int proposedSize, int? minimalSize = null)
    {
        int correctedSize = proposedSize;
        if (correctedSize % 2 != 0) correctedSize++;

        if (minimalSize.HasValue && correctedSize < minimalSize.Value)
        {
            correctedSize = minimalSize.Value;
            if (correctedSize % 2 != 0) correctedSize++;
        }

        return correctedSize;
    }

    public static BoardConfig CorrectConfig(BoardConfig config, int? minimalSize = null)
    {
        int correctedSize = CorrectSize(config.FieldSize, minimalSize);
        return config with { FieldSize = correctedSize };
    }
}