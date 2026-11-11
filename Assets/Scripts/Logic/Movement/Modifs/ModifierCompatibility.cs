
#nullable enable


public static class ModifierCompatibility
{
    public static bool IsCompatibleWithPiece(IModifier modifier, IPiece piece)
    {
        // Пока все модификаторы совместимы с любыми фигурами.
        // В фазе 7 добавим проверку для OpeningDoubleMoveModifier (только Pawn).
        return true;
    }
}