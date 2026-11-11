#nullable enable

public static class ModifierCompatibility
{
    public static bool IsCompatibleWithPiece(IModifier modifier, IPiece piece)
    {
        if (modifier is OpeningDoubleMoveModifier && piece.Name != "Pawn")
            return false;
        return true;
    }
}