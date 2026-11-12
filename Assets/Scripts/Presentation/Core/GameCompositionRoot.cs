#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameCompositionRoot : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private Transform _piecesRoot = null!;
    [SerializeField] private Material _pieceMaterial = null!;

    [Header("UI")]
    [SerializeField] private BoardUI _boardUI = null!;
    [SerializeField] private LayerPanelUI _layerPanelUI = null!;
    [SerializeField] private PlanUI _planUI = null!;
    [SerializeField] private PromotionUI _promotionUI = null!;
    [SerializeField] private ActionChoiceUI _actionChoiceUI = null!;
    [SerializeField] private UIPointerProbe _uiPointerProbe = null!;

    [Header("Selection")]
    [SerializeField] private SelectionHub _selectionHub = null!;

    [Header("Camera")]
    [SerializeField] private Camera _camera = null!;
    [SerializeField] private CameraWork _cameraWork = null!;
    [SerializeField] private WorldToScreenMarker _cubeCenterMarker = null!;
    [SerializeField] private WorldToScreenMarker _rotationCenterMarker = null!;
    [SerializeField] private Camera _layerCamera = null!;
    [SerializeField] private Material _layerMaterial = null!;
    [SerializeField] private Material _markerMaterial = null!;

    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActions = null!;
    [SerializeField] private MouseEventer _mouseEventer = null!;
    [SerializeField] private ClickLeft _clickLeftHub = null!;
    [SerializeField] private ClickRight _clickRightHub = null!;
    [SerializeField] private HoldRight _holdRightHub = null!;
    [SerializeField] private HoldMiddle _holdMiddleHub = null!;
    [SerializeField] private Scroll _scrollHub = null!;
    [SerializeField] private Pointer _pointerHub = null!;
    [SerializeField] private Delta _deltaHub = null!;

    [Header("Config")]
    [SerializeField] private TextAsset _boardLayoutJson = null!;
    [SerializeField] private TextAsset _pieceLibraryJson = null!;

    private ILogger _logger = null!;
    private IEventBus _eventBus = null!;
    private PiecePrefabRegistry _piecePrefabs = null!;
    private ConfigOfBoard _configOfBoard = null!;
    private IReadOnlyList<StartingPlacement> _placements = null!;
    private Seats _seats;
    private Board _board = null!;
    private IHouse _house = null!;
    private IRegistry _registry = null!;
    private TurnManager _turnManager = null!;
    private MatchState _matchState = null!;
    private MatchEndChecker _matchEndChecker = null!;
    private IMovementValidator _movementValidator = null!;
    private IAbilityService _abilityService = null!;
    private IFlow _flow = null!;
    private DeckSequenceProvider _deckSequences = null!;
    private ModifierHost _modifierHost = null!;
    private LoaderOfBoard _loaderOfBoard = null!;
    private ViewState _viewState = null!;
    private CameraManipulationState _manipulationState = null!;
    private HoverState _hoverState = null!;
    private SelectionState _selectionState = null!;
    private ScenePointer _scenePointer = null!;
    private LayerPresenter _layerPresenter = null!;
    private LayerCameraWork _layerCameraWork = null!;
    private PieceSpawner _pieceSpawner = null!;
    private PieceViewRegistry _pieceViewRegistry = null!;
    private EnPassantService _enPassantService = null!;
    private AmbushService _ambushService = null!;
    private ReactiveActionRegistry _reactiveRegistry = null!;
    private PieceTemplateRegistry _pieceTemplateRegistry = null!;
    private PromotionService _promotionService = null!;
    private Graveyard[] _graveyards = Array.Empty<Graveyard>();
    private CellActionHandler _cellActionHandler = null!;
    private ModifierActivationHandler _modifierActivationHandler = null!;
    private PromotionChoiceHandler _promotionChoiceHandler = null!;
    private PendingModalManager _pendingModalManager = null!;
    private IReadOnlyList<ICellActionProvider> _cellActionProviders = null!;
    private IAttackTargetProvider _attackTargetProvider = null!;
    private SelectionController _selectionController = null!;
    private TargetSelectionController _targetSelectionController = null!;
    private SceneClickDispatcher _sceneClickDispatcher = null!;
    private SelectionMarkerCoordinator _selectionMarkerCoordinator = null!;
    private BoardCommands _boardCommands = null!;

    private void Awake()
    {
        ValidateReferences();
        CreateInfrastructure();
        LoadData();
        CreateLogic();
        CreatePresentation();
        InitializeSceneComponents();
    }
    private void Start()
    {
        StartGame();
    }
    private void OnDestroy()
    {
        _layerCameraWork?.Dispose();
        _loaderOfBoard?.Dispose();
        _reactiveRegistry?.Dispose();
        _deckSequences?.Dispose();
        _abilityService?.Dispose();
        _modifierHost?.Dispose();
        _house?.Dispose();
    }

    private void ValidateReferences()
    {
        if (_piecesRoot == null) throw new ArgumentNullException(nameof(_piecesRoot));
        if (_pieceMaterial == null) throw new ArgumentNullException(nameof(_pieceMaterial));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
        if (_uiPointerProbe == null) throw new ArgumentNullException(nameof(_uiPointerProbe));
        if (_selectionHub == null) throw new ArgumentNullException(nameof(_selectionHub));
        if (_camera == null) throw new ArgumentNullException(nameof(_camera));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_cubeCenterMarker == null) throw new ArgumentNullException(nameof(_cubeCenterMarker));
        if (_rotationCenterMarker == null) throw new ArgumentNullException(nameof(_rotationCenterMarker));
        if (_inputActions == null) throw new ArgumentNullException(nameof(_inputActions));
        if (_mouseEventer == null) throw new ArgumentNullException(nameof(_mouseEventer));
        if (_clickLeftHub == null) throw new ArgumentNullException(nameof(_clickLeftHub));
        if (_clickRightHub == null) throw new ArgumentNullException(nameof(_clickRightHub));
        if (_holdRightHub == null) throw new ArgumentNullException(nameof(_holdRightHub));
        if (_holdMiddleHub == null) throw new ArgumentNullException(nameof(_holdMiddleHub));
        if (_scrollHub == null) throw new ArgumentNullException(nameof(_scrollHub));
        if (_pointerHub == null) throw new ArgumentNullException(nameof(_pointerHub));
        if (_deltaHub == null) throw new ArgumentNullException(nameof(_deltaHub));
        if (_boardLayoutJson == null) throw new ArgumentNullException(nameof(_boardLayoutJson));
        if (_pieceLibraryJson == null) throw new ArgumentNullException(nameof(_pieceLibraryJson));
        if (_layerCamera == null) throw new ArgumentNullException(nameof(_layerCamera));
        if (_layerMaterial == null) throw new ArgumentNullException(nameof(_layerMaterial));
        if (_markerMaterial == null) throw new ArgumentNullException(nameof(_markerMaterial));
        if (_layerPanelUI == null) throw new ArgumentNullException(nameof(_layerPanelUI));
        if (_planUI == null) throw new ArgumentNullException(nameof(_planUI));
        if (_promotionUI == null) throw new ArgumentNullException(nameof(_promotionUI));
        if (_actionChoiceUI == null) throw new ArgumentNullException(nameof(_actionChoiceUI));
    }
    private void CreateInfrastructure()
    {
        _logger = new LoggerForUnity();
        _eventBus = new EventBus(_logger);
    }
    private void LoadData()
    {
        _piecePrefabs = PieceLibraryLoader.Load(_pieceLibraryJson.text);
        _deckSequences = new DeckSequenceProvider(_eventBus);

        var templates = StandardPieceTemplates.CreateAll(_deckSequences, _eventBus);
        _pieceTemplateRegistry = new PieceTemplateRegistry(templates);

        var layoutResult = BoardLayoutLoader.Load(_boardLayoutJson.text, _pieceTemplateRegistry);
        _configOfBoard = layoutResult.Config;
        _placements = layoutResult.Placements;

        _logger.Log($"Данные загружены: {_placements.Count} фигур, поле {_configOfBoard.FieldSize}, осей {_configOfBoard.Axes}");
    }
    private void CreateLogic()
    {
        _seats = new Seats(_configOfBoard.NumberOfSides, _configOfBoard.NumberOfPlayersOnSide);
        _modifierHost = new ModifierHost(_eventBus, _logger);

        var pieceFactory = new PieceFactory(_eventBus, _modifierHost, _logger);
        _house = new House(_seats, _eventBus, _logger, pieceFactory);
        _registry = new Registry(_house);

        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _board = new Board(_seats, boardSize, _eventBus, _logger);
        _house.AddRoom(_board);
        _house.AddRoom(new Decks(_seats, _eventBus, _logger));

        var players = new HashSet<IdOfPlayer>();
        foreach (var placement in _placements) players.Add(placement.Definition.Owner);
        var graveyards = new List<Graveyard>();
        foreach (var player in players)
        {
            var graveyard = new Graveyard(player, _seats, _eventBus, _logger);
            _house.AddRoom($"Graveyard_{player.IndexOfSide}_{player.IndexOfPlayerOnSide}", graveyard);
            graveyards.Add(graveyard);
        }
        _graveyards = graveyards.ToArray();

        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);

        _attackTargetProvider = new AttackTargetProvider(_modifierHost);
        var combatService = new CombatService(_logger, _eventBus, _attackTargetProvider, _modifierHost);

        _modifierHost.Registry = _registry;

        _turnManager = new TurnManager(_placements, _eventBus);
        _matchState = new MatchState();
        _matchEndChecker = new MatchEndChecker(_registry, _turnManager, _matchState);
        _enPassantService = new EnPassantService(_registry, _turnManager, _eventBus, _modifierHost);
        _ambushService = new AmbushService(_registry, _turnManager, _eventBus, _modifierHost);
        _reactiveRegistry = new ReactiveActionRegistry();
        _reactiveRegistry.Register(_enPassantService);
        _reactiveRegistry.Register(_ambushService);
        _promotionService = new PromotionService(_registry, _house, _pieceTemplateRegistry, pieceFactory, _modifierHost, _eventBus, _logger);
        _flow = new Flow(
            _house,
            _registry,
            _eventBus,
            _logger,
            _movementValidator,
            _abilityService,
            combatService,
            _modifierHost,
            _enPassantService,
            _ambushService,
            onCommitted: () =>
            {
                bool gameOver = _matchEndChecker.CheckAfterCommit();
                if (!gameOver) _turnManager.EndTurn();
            });

        _pendingModalManager = new PendingModalManager();

        var attackProvider = new AttackCellActionProvider(_flow, _attackTargetProvider, _registry, _modifierHost);
        var enPassantProvider = new EnPassantActionProvider(_flow);
        var ambushProvider = new AmbushActionProvider(_flow);
        _cellActionProviders = new ICellActionProvider[] { attackProvider, enPassantProvider, ambushProvider };

        _cellActionHandler = new CellActionHandler(new CellActionHandlerContext
        {
            Flow = _flow,
            PendingModalManager = _pendingModalManager,
            MatchState = _matchState,
            CellActionProviders = _cellActionProviders,
            Logger = _logger,
        });

        _modifierActivationHandler = new ModifierActivationHandler(new ModifierActivationHandlerContext
        {
            Flow = _flow,
            PendingModalManager = _pendingModalManager,
            MatchState = _matchState,
            ModifierHost = _modifierHost,
            Registry = _registry,
            EventBus = _eventBus,
            Logger = _logger,
        });

        _promotionChoiceHandler = new PromotionChoiceHandler(new PromotionChoiceHandlerContext
        {
            PendingModalManager = _pendingModalManager,
            MatchState = _matchState,
            PromotionService = _promotionService,
            Logger = _logger,
        });
    }
    private void CreatePresentation()
    {
        _manipulationState = new CameraManipulationState();
        _hoverState = new HoverState();
        _selectionState = new SelectionState();
        _pieceViewRegistry = new PieceViewRegistry();
        _pieceSpawner = new PieceSpawner(
            _piecesRoot,
            _pieceMaterial,
            _piecePrefabs,
            _seats.CountOfSides,
            _seats.CountOfPlayersOnSide);

        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _loaderOfBoard = gameObject.AddComponent<LoaderOfBoard>();
        _loaderOfBoard.Initialize(_pieceSpawner, _pieceViewRegistry, _eventBus, _logger, boardSize);
        _loaderOfBoard.Run();

        _viewState = new ViewState();
        int initialAxis;
        if (boardSize.W > 1) initialAxis = GridAxes.W;
        else if (boardSize.Z > 1) initialAxis = GridAxes.Z;
        else if (boardSize.Y > 1) initialAxis = GridAxes.Y;
        else initialAxis = GridAxes.X;
        int maxLayer = GridAxes.GetGridSize(initialAxis, boardSize) - 1;
        int initialLayer = Mathf.Clamp(ViewState.GetDefaultLayerFor(initialAxis, false), 0, maxLayer);
        _viewState.Initialize(initialAxis, initialLayer);

        var layerPresenterGo = new GameObject("LayerPresenter");
        layerPresenterGo.transform.SetParent(transform, false);
        _layerPresenter = layerPresenterGo.AddComponent<LayerPresenter>();
        _layerPresenter.Initialize(_loaderOfBoard, _viewState, _pieceViewRegistry, _layerMaterial, _eventBus, _logger);

        _layerCameraWork = _layerCamera.gameObject.GetComponent<LayerCameraWork>();
        if (_layerCameraWork == null)
            _layerCameraWork = _layerCamera.gameObject.AddComponent<LayerCameraWork>();
        _layerCameraWork.Initialize(_layerPresenter, _loaderOfBoard, _viewState, _layerCamera);
    }
    private void InitializeSceneComponents()
    {
        _mouseEventer.Initialize(_inputActions, _clickLeftHub, _clickRightHub, _holdRightHub, _holdMiddleHub, _scrollHub, _pointerHub, _deltaHub);
        _cameraWork.Initialize(new CameraWorkContext
        {
            BoardLoader = _loaderOfBoard,
            Camera = _camera,
            CubeCenterMarker = _cubeCenterMarker,
            RotationCenterMarker = _rotationCenterMarker,
            HoldRightHub = _holdRightHub,
            HoldMiddleHub = _holdMiddleHub,
            DeltaHub = _deltaHub,
            ScrollHub = _scrollHub,
            ManipulationState = _manipulationState,
        });
        _boardCommands = new BoardCommands(_loaderOfBoard, _layerPresenter, _viewState);
        _boardUI.Initialize(_loaderOfBoard, _viewState, _selectionState, _boardCommands, _uiPointerProbe);
        _layerPanelUI.Initialize(_viewState, _boardCommands, _uiPointerProbe);

        _selectionMarkerCoordinator = new SelectionMarkerCoordinator(new SelectionMarkerCoordinatorContext
        {
            ViewState = _viewState,
            LayerPresenter = _layerPresenter,
            BoardLoader = _loaderOfBoard,
            SelectionState = _selectionState,
            Registry = _registry,
            PendingModalManager = _pendingModalManager,
            MatchState = _matchState,
            MarkerMaterial = _markerMaterial,
            CellActionProviders = _cellActionProviders,
        });

        _selectionController = gameObject.AddComponent<SelectionController>();
        _selectionController.Initialize(new SelectionControllerContext
        {
            Flow = _flow,
            Registry = _registry,
            BoardLoader = _loaderOfBoard,
            ViewState = _viewState,
            LayerPresenter = _layerPresenter,
            SelectionState = _selectionState,
            HoverState = _hoverState,
            SelectionHub = _selectionHub,
            MatchState = _matchState,
            TurnManager = _turnManager,
            PendingModalManager = _pendingModalManager,
            MarkerCoordinator = _selectionMarkerCoordinator,
            Logger = _logger,
        });

        _targetSelectionController = gameObject.AddComponent<TargetSelectionController>();
        _targetSelectionController.Initialize(new TargetSelectionContext
        {
            Flow = _flow,
            ModifierHost = _modifierHost,
            SelectionState = _selectionState,
            SelectionHub = _selectionHub,
            MatchState = _matchState,
            BoardLoader = _loaderOfBoard,
            LayerPresenter = _layerPresenter,
            ViewState = _viewState,
            PendingModalManager = _pendingModalManager,
            MarkerMaterial = _markerMaterial,
            Logger = _logger,
        });
        _planUI.Initialize(
            _flow,
            _turnManager,
            _selectionState,
            _selectionController,
            _targetSelectionController,
            _modifierActivationHandler,
            _pendingModalManager,
            _uiPointerProbe,
            _matchState,
            _modifierHost,
            _logger);

        _promotionUI.Initialize(_promotionChoiceHandler, _pendingModalManager, _uiPointerProbe);
        _actionChoiceUI.Initialize(_cellActionHandler, _pendingModalManager, _uiPointerProbe);
        _sceneClickDispatcher = new SceneClickDispatcher(
            _selectionController, _targetSelectionController, _cellActionHandler, _selectionState);

        var scenePointerGo = new GameObject("ScenePointer");
        scenePointerGo.transform.SetParent(transform, false);
        _scenePointer = scenePointerGo.AddComponent<ScenePointer>();
        _scenePointer.Initialize(new ScenePointerContext
        {
            ClickLeftHub = _clickLeftHub,
            PointerHub = _pointerHub,
            UIPointerProbe = _uiPointerProbe,
            Camera = _camera,
            LayerCamera = _layerCamera,
            ViewState = _viewState,
            HoverState = _hoverState,
            ManipulationState = _manipulationState,
            PointerLayers = ~0,
            MaxRayDistance = 1000f,
            ClassifyOverride = null,
            DispatchOverride = target => _sceneClickDispatcher.Dispatch(target),
            BlockInteractionProvider = () => _pendingModalManager.HasPending,
        });

        _mouseEventer.Run();
        _cameraWork.Run();
        _layerPresenter.Run();
        _layerCameraWork.Run();
        _boardUI.Run();
        _layerPanelUI.Run();
        _planUI.Run();
        _scenePointer.Run();
        _selectionController.Run();
        _targetSelectionController.Run();
    }
    private void StartGame()
    {
        foreach (var placement in _placements)
        {
            var result = _flow.AddPiece<Board, Square>(placement.Definition, placement.Position);
            if (result.IsSuccess)
                continue;
            _logger.LogError($"Не удалось развернуть '{placement.Definition.Name}' в {placement.Position}: {result.Error}");
        }
        _logger.Log("Все фигуры развёрнуты");

        _turnManager.Start();
    }
}