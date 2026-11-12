#nullable enable
using System;

public sealed class SceneClickDispatcher
{
    private readonly SelectionController _selectionController;
    private readonly TargetSelectionController _targetSelectionController;
    private readonly CellActionHandler _cellActionHandler;
    private readonly SelectionState _selectionState;

    public SceneClickDispatcher(
        SelectionController selectionController,
        TargetSelectionController targetSelectionController,
        CellActionHandler cellActionHandler,
        SelectionState selectionState)
    {
        _selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        _targetSelectionController = targetSelectionController ?? throw new ArgumentNullException(nameof(targetSelectionController));
        _cellActionHandler = cellActionHandler ?? throw new ArgumentNullException(nameof(cellActionHandler));
        _selectionState = selectionState ?? throw new ArgumentNullException(nameof(selectionState));
    }

    public void Dispatch(SceneTargetInfo target)
    {
        if (_targetSelectionController.IsActive)
        {
            if (target.Kind == InteractionTargetKind.TargetMarker && target.TargetMarker != null)
                _targetSelectionController.HandleTargetClick(target.TargetMarker.Cell);
            else
                _targetSelectionController.Cancel();
            return;
        }

        switch (target.Kind)
        {
            case InteractionTargetKind.TargetMarker:
                if (target.TargetMarker != null)
                    _targetSelectionController.HandleTargetClick(target.TargetMarker.Cell);
                break;

            case InteractionTargetKind.AttackTarget:
                if (target.AttackTarget != null)
                {
                    var actor = _selectionState.SelectedInfo?.Piece;
                    if (actor != null)
                        _cellActionHandler.HandleActionCellClick(actor, target.AttackTarget.Cell);
                }
                break;

            case InteractionTargetKind.MoveTarget:
                if (target.MoveTarget != null)
                    _selectionController.HandleMoveTargetClick(target.MoveTarget);
                break;

            case InteractionTargetKind.Piece:
                if (target.Piece != null)
                    _selectionController.HandlePieceClick(target.Piece);
                break;

            case InteractionTargetKind.None:
            default:
                _selectionController.HandleEmptyClick();
                break;
        }
    }
}