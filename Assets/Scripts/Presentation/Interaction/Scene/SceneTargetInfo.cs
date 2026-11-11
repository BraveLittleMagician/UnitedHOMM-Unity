
#nullable enable

public readonly struct SceneTargetInfo
{
    public InteractionTargetKind Kind { get; }
    public InfoOfPiece? Piece { get; }
    public MoveTargetMarker? MoveTarget { get; }
    public AttackMarker? AttackTarget { get; }
    public TargetTargetMarker? TargetMarker { get; }

    public SceneTargetInfo(
        InteractionTargetKind kind,
        InfoOfPiece? piece,
        MoveTargetMarker? moveTarget,
        AttackMarker? attackTarget,
        TargetTargetMarker? targetMarker)
    {
        Kind = kind;
        Piece = piece;
        MoveTarget = moveTarget;
        AttackTarget = attackTarget;
        TargetMarker = targetMarker;
    }

    public static SceneTargetInfo None => new(InteractionTargetKind.None, null, null, null, null);
}