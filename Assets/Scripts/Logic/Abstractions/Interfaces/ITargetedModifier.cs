#nullable enable

using System.Collections.Generic;

public interface ITargetedModifier : IActiveModifier
{
    IReadOnlyList<Square> GetTargetCells(IPiece owner, Square ownerPosition, Square gridSize);
    void Activate(ModifierActivationContext context, Square targetCell);
}