#nullable enable

using System;
using UnityEngine;

public sealed class AttackAction : IGameAction
{
    private readonly IPiece _attacker;
    private readonly IPath<Square> _attackPath;
    private readonly IFlow _flow;
    private readonly AttackKind _kind;

    public AttackAction(IPiece attacker, IPath<Square> attackPath, IFlow flow, AttackKind kind)
    {
        _attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
        _attackPath = attackPath ?? throw new ArgumentNullException(nameof(attackPath));
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _kind = kind;
    }

    public IdOfPlayer Owner => _attacker.Owner;
    public IPiece? FocusPiece => _attacker;
    public IPath<Square> AttackPath => _attackPath;
    public AttackKind Kind => _kind;

    public Vector3Int TargetCell
    {
        get
        {
            var s = _attackPath.Positions[^1];
            return new Vector3Int(s.X, s.Y, s.Z);
        }
    }

    public bool CanExecute()
    {
        if (_attacker.Health <= 0) return false;
        if (_attackPath.Positions.Count < 2) return false;
        return true;
    }

    public void Execute()
    {
        var result = _kind == AttackKind.Melee
            ? _flow.MeleeAttack<Board, Square>(_attacker.Owner, _attackPath)
            : _flow.RangedAttack<Board, Square>(_attacker.Owner, _attackPath);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"AttackAction.Execute failed ({_kind}): {result.Error}");
    }
}