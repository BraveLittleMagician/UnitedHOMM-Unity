
#nullable enable

using System.Collections.Generic;

public interface IAttackTargetProvider
{
    bool IsAttackTarget(IPiece attacker, Square cell, IReadOnlyRoom room);
    IReadOnlyList<Square> GetAttackTargets(IPiece attacker, IReadOnlyRoom room);
    bool TryFindAttackPath(IPiece attacker, Square target, IReadOnlyRoom room, out IPath<Square>? path);
}