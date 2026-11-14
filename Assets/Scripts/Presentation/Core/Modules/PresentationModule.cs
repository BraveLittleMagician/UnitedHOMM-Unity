#nullable enable
using System;
using UnityEngine;

public static class PresentationModule
{
    public static PresentationContext Initialize(
        IEventBus eventBus,
        ILogger logger,
        DomainContext domain,
        GameObject host,
        Transform piecesRoot,
        Material pieceMaterial,
        Material markerMaterial,
        Material layerMaterial,
        Camera camera,
        Camera layerCamera,
        CameraWork cameraWork,
        WorldToScreenMarker cubeCenterMarker,
        WorldToScreenMarker rotationCenterMarker,
        SelectionHub selectionHub,
        IClickLeftHub clickLeftHub,
        IPointerHub pointerHub,
        IHoldRightHub holdRightHub,
        IHoldMiddleHub holdMiddleHub,
        IDeltaHub deltaHub,
        IScrollHub scrollHub,
        UIPointerProbe uiPointerProbe)
    {
        if (domain == null) throw new ArgumentNullException(nameof(domain));
        if (host == null) throw new ArgumentNullException(nameof(host));

        int wwOffset = domain.Board.WWOffset;

        var pieceViewRegistry = new PieceViewRegistry();
        var pieceSpawner = new PieceSpawner(
            piecesRoot,
            pieceMaterial,
            domain.PiecePrefabs,
            domain.Seats.CountOfSides,
            domain.Seats.CountOfPlayersOnSide);

        Square boardSize = domain.Board.Size;

        var loaderOfBoard = host.AddComponent<LoaderOfBoard>();
        loaderOfBoard.Initialize(pieceSpawner, pieceViewRegistry, eventBus, logger, boardSize, wwOffset);
        loaderOfBoard.Run();

        var viewState = new ViewState();
        int initialAxis = GridAxes.X;
        int initialW = 0;
        int maxLayer = GridAxes.GetGridSize(initialAxis, boardSize) - 1;
        int initialLayer = Mathf.Clamp(ViewState.GetDefaultLayerFor(initialAxis, false), 0, maxLayer);
        viewState.Initialize(initialAxis, initialLayer, initialW, wwOffset);

        var selectionState = new SelectionState();
        var hoverState = new HoverState();
        var manipulationState = new CameraManipulationState();

        var layerPresenterGo = new GameObject("LayerPresenter");
        layerPresenterGo.transform.SetParent(host.transform, false);
        var layerPresenter = layerPresenterGo.AddComponent<LayerPresenter>();
        layerPresenter.Initialize(loaderOfBoard, viewState, pieceViewRegistry, layerMaterial, eventBus, logger);

        var layerCameraWork = layerCamera.gameObject.GetComponent<LayerCameraWork>();
        if (layerCameraWork == null)
            layerCameraWork = layerCamera.gameObject.AddComponent<LayerCameraWork>();
        layerCameraWork.Initialize(layerPresenter, loaderOfBoard, viewState, layerCamera);

        var moveAnimatorGo = new GameObject("MoveAnimator");
        moveAnimatorGo.transform.SetParent(host.transform, false);
        var moveAnimator = moveAnimatorGo.AddComponent<MoveAnimator>();
        moveAnimator.Initialize(layerPresenter, loaderOfBoard, viewState);

        var selectionMarkerCoordinator = new SelectionMarkerCoordinator(new SelectionMarkerCoordinatorContext
        {
            ViewState = viewState,
            LayerPresenter = layerPresenter,
            BoardLoader = loaderOfBoard,
            SelectionState = selectionState,
            Registry = domain.Registry,
            PendingModalManager = domain.PendingModalManager,
            MatchState = domain.MatchState,
            MarkerMaterial = markerMaterial,
            CellActionProviders = domain.CellActionProviders,
        });

        var selectionController = host.AddComponent<SelectionController>();
        selectionController.Initialize(new SelectionControllerContext
        {
            Flow = domain.Flow,
            Registry = domain.Registry,
            BoardLoader = loaderOfBoard,
            ViewState = viewState,
            LayerPresenter = layerPresenter,
            SelectionState = selectionState,
            HoverState = hoverState,
            SelectionHub = selectionHub,
            MatchState = domain.MatchState,
            TurnManager = domain.TurnManager,
            PendingModalManager = domain.PendingModalManager,
            MarkerCoordinator = selectionMarkerCoordinator,
            Logger = logger,
            MoveAnimator = moveAnimator,
        });

        var targetSelectionController = host.AddComponent<TargetSelectionController>();
        targetSelectionController.Initialize(new TargetSelectionContext
        {
            Flow = domain.Flow,
            ModifierHost = domain.ModifierHost,
            SelectionState = selectionState,
            SelectionHub = selectionHub,
            MatchState = domain.MatchState,
            BoardLoader = loaderOfBoard,
            LayerPresenter = layerPresenter,
            ViewState = viewState,
            PendingModalManager = domain.PendingModalManager,
            MarkerMaterial = markerMaterial,
            Logger = logger,
        });

        var sceneClickDispatcher = new SceneClickDispatcher(selectionController, targetSelectionController, domain.CellActionHandler, selectionState, viewState);

        var scenePointerGo = new GameObject("ScenePointer");
        scenePointerGo.transform.SetParent(host.transform, false);
        var scenePointer = scenePointerGo.AddComponent<ScenePointer>();
        scenePointer.Initialize(new ScenePointerContext
        {
            ClickLeftHub = clickLeftHub,
            PointerHub = pointerHub,
            UIPointerProbe = uiPointerProbe,
            Camera = camera,
            LayerCamera = layerCamera,
            ViewState = viewState,
            HoverState = hoverState,
            ManipulationState = manipulationState,
            PointerLayers = ~0,
            MaxRayDistance = 1000f,
            WWOffset = wwOffset,
            ClassifyOverride = null,
            DispatchOverride = target => sceneClickDispatcher.Dispatch(target),
            BlockInteractionProvider = () => domain.PendingModalManager.HasPending || moveAnimator.IsAnimating,
        });

        cameraWork.Initialize(new CameraWorkContext
        {
            BoardLoader = loaderOfBoard,
            Camera = camera,
            CubeCenterMarker = cubeCenterMarker,
            RotationCenterMarker = rotationCenterMarker,
            HoldRightHub = holdRightHub,
            HoldMiddleHub = holdMiddleHub,
            DeltaHub = deltaHub,
            ScrollHub = scrollHub,
            ManipulationState = manipulationState,
        });

        return new PresentationContext(
            LoaderOfBoard: loaderOfBoard,
            ViewState: viewState,
            SelectionState: selectionState,
            LayerPresenter: layerPresenter,
            SelectionController: selectionController,
            TargetSelectionController: targetSelectionController,
            CameraWork: cameraWork,
            LayerCameraWork: layerCameraWork,
            MoveAnimator: moveAnimator,
            ScenePointer: scenePointer);
    }
}