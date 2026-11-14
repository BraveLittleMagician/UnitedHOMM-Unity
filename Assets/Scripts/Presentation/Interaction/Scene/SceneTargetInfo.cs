#nullable enable

public readonly struct SceneTargetInfo
{
    public InteractionTargetKind Kind { get; }
    public InfoOfPiece? Piece { get; }
    public MoveTargetMarker? MoveTarget { get; }
    public AttackMarker? AttackTarget { get; }
    public TargetTargetMarker? TargetMarker { get; }
    public int? WLayerIndex { get; }

    public SceneTargetInfo(
        InteractionTargetKind kind,
        InfoOfPiece? piece,
        MoveTargetMarker? moveTarget,
        AttackMarker? attackTarget,
        TargetTargetMarker? targetMarker,
        int? wLayerIndex = null)
    {
        Kind = kind;
        Piece = piece;
        MoveTarget = moveTarget;
        AttackTarget = attackTarget;
        TargetMarker = targetMarker;
        WLayerIndex = wLayerIndex;
    }

    public static SceneTargetInfo None => new(InteractionTargetKind.None, null, null, null, null);
}