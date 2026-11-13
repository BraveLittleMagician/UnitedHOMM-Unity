#nullable enable
using System;
using System.Collections;
using UnityEngine;

public sealed class MoveAnimator : MonoBehaviour
{
    [SerializeField] private float _stepDelay = 0.15f;

    private LayerPresenter _layerPresenter = null!;
    private LoaderOfBoard _boardLoader = null!;
    private ViewState _viewState = null!;

    public bool IsAnimating { get; private set; }

    public void Initialize(LayerPresenter layerPresenter, LoaderOfBoard boardLoader, ViewState viewState)
    {
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
    }

    public void Play(InfoOfPiece piece, IPath<Square> path)
    {
        if (piece == null || path == null) return;
        if (IsAnimating) return;
        if (path.Positions.Count < 2) return;

        StartCoroutine(RunMove(piece, path));
    }

    private IEnumerator RunMove(InfoOfPiece piece, IPath<Square> path)
    {
        IsAnimating = true;
        try
        {
            var positions = path.Positions;

            AttachToPiece(piece, positions[0]);
            _viewState.SetActiveLayer(GridAxes.GetLayerIndex(_viewState.AxisIndex, positions[0], _boardLoader.WWOffset));
            yield return null;

            var wait = new WaitForSeconds(_stepDelay);

            for (int i = 1; i < positions.Count; i++)
            {
                var cell = positions[i];
                AttachToPiece(piece, cell);
                _viewState.SetActiveLayer(GridAxes.GetLayerIndex(_viewState.AxisIndex, cell, _boardLoader.WWOffset));
                yield return wait;
            }
        }
        finally
        {
            IsAnimating = false;
        }
    }

    private void AttachToPiece(InfoOfPiece piece, Square cell)
    {
        int axis = _viewState.AxisIndex;
        int layerIndex = GridAxes.GetLayerIndex(axis, cell, _boardLoader.WWOffset);

        LayerContainer? container = null;
        foreach (var c in _layerPresenter.Containers)
        {
            if (c.LayerIndex == layerIndex)
            {
                container = c;
                break;
            }
        }
        if (container == null) return;

        piece.transform.SetParent(container.transform, false);

        Vector3 worldLocal = GridAxes.GridToWorldLocal(
            cell,
            _boardLoader.GridSize,
            _boardLoader.Step,
            _boardLoader.WSpread,
            _boardLoader.WWOffset);
        Vector3 local = worldLocal - container.TargetLocalPosition;

        if (axis != GridAxes.W)
        {
            int worldComp = GridAxes.GetWorldComponent(axis);
            if (worldComp >= 0 && worldComp < 3) local[worldComp] = 0f;
        }

        piece.transform.localPosition = local;
    }
}