#nullable enable
using System;

public sealed record PresentationContext(
    LoaderOfBoard LoaderOfBoard,
    ViewState ViewState,
    SelectionState SelectionState,
    LayerPresenter LayerPresenter,
    SelectionController SelectionController,
    TargetSelectionController TargetSelectionController,
    CameraWork CameraWork,
    LayerCameraWork LayerCameraWork,
    MoveAnimator MoveAnimator,
    ScenePointer ScenePointer)
{
    public void Run()
    {
        CameraWork.Run();
        LayerPresenter.Run();
        LayerCameraWork.Run();
        ScenePointer.Run();
        SelectionController.Run();
        TargetSelectionController.Run();
    }

    public void Dispose()
    {
        try { LayerCameraWork?.Dispose(); } catch (Exception ex) { UnityEngine.Debug.LogError($"[PresentationContext.Dispose] {ex}"); }
        try { LoaderOfBoard?.Dispose(); } catch (Exception ex) { UnityEngine.Debug.LogError($"[PresentationContext.Dispose] {ex}"); }
    }
}
