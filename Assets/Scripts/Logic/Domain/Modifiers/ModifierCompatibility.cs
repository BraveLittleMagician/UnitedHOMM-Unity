#nullable enable

public static class ModifierCompatibility
{
    public static bool IsCompatibleWithPiece(IModifier modifier, PieceModel model)
    {
        // DoubleMove добавляет смещение (0,2,0) в локальной системе, осмысленно только для Pawn.
        if (modifier is OpeningDoubleMoveModifier && model.Name != "Pawn")
            return false;
        return true;
    }
}