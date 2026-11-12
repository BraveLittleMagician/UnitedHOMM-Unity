#nullable enable
using System;
using UnityEngine;

public sealed class BoardCommands
{
    private readonly LoaderOfBoard _loaderOfBoard;
    private readonly LayerPresenter _layerPresenter;
    private readonly ViewState _viewState;

    public BoardCommands(LoaderOfBoard loaderOfBoard, LayerPresenter layerPresenter, ViewState viewState)
    {
        _loaderOfBoard = loaderOfBoard ?? throw new ArgumentNullException(nameof(loaderOfBoard));
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
    }

    public void SetAxis(int uiAxis)
    {
        if (uiAxis < 0 || uiAxis >= GridAxes.Count) return;
        int realAxis = GridAxes.FromUIAxis(uiAxis);
        if (realAxis == _viewState.AxisIndex) return;
        _viewState.SetAxis(realAxis);
        int maxLayer = GridAxes.GetGridSize(realAxis, _loaderOfBoard.GridSize) - 1;
        int target = Mathf.Clamp(_viewState.GetDefaultLayerForCurrentAxis(), 0, maxLayer);
        _viewState.SetActiveLayer(target);
    }

    public void SetActiveLayer(int index) => _viewState.SetActiveLayer(index);

    public void StepActiveLayer(int delta)
    {
        int maxLayer = GridAxes.GetGridSize(_viewState.AxisIndex, _loaderOfBoard.GridSize) - 1;
        _viewState.StepActiveLayer(delta, maxLayer);
    }

    public void SetLayerGap(float gap) => _viewState.SetLayerGap(gap);

    public void SetSpacing(float spacing)
    {
        float clamped = Mathf.Max(0f, spacing);
        if (Mathf.Approximately(clamped, _loaderOfBoard.Spacing)) return;
        _loaderOfBoard.SetSpacing(clamped);
        _layerPresenter.ReflowLayers();
    }

    public void SetGridSize(Square newSize)
    {
        newSize = new Square
        {
            X = Mathf.Max(newSize.X, 1),
            Y = Mathf.Max(newSize.Y, 1),
            Z = Mathf.Max(newSize.Z, 1),
            W = Mathf.Max(newSize.W, 1),
        };
        if (SameSize(newSize, _loaderOfBoard.GridSize)) return;
        _loaderOfBoard.SetGridSize(newSize);
        int maxLayer = GridAxes.GetGridSize(_viewState.AxisIndex, newSize) - 1;
        if (_viewState.ActiveLayerIndex > maxLayer)
            _viewState.SetActiveLayer(maxLayer);
        _layerPresenter.Rebuild(_viewState.AxisIndex);
    }

    public void SetReverseView(bool value)
    {
        if (_viewState.IsReverseView == value) return;
        _viewState.SetReverseView(value);
        int maxLayer = GridAxes.GetGridSize(_viewState.AxisIndex, _loaderOfBoard.GridSize) - 1;
        int target = Mathf.Clamp(_viewState.GetDefaultLayerForCurrentAxis(), 0, maxLayer);
        _viewState.SetActiveLayer(target);
    }

    public void FlipView() => _viewState.Flip();

    public void RotateLayerCamera(int deltaDegrees) => _viewState.RotateLayerCamera(deltaDegrees);

    private static bool SameSize(Square a, Square b) =>
        a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
}
