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
    [SerializeField] private UIPointerProbe _uiPointerProbe = null!;

    [Header("Selection")]
    [SerializeField] private SelectionHub _selectionHub = null!;
    [SerializeField] private DeselectOnEmptyClick _deselectOnEmptyClick = null!;

    [Header("Camera")]
    [SerializeField] private Camera _camera = null!;
    [SerializeField] private CameraWork _cameraWork = null!;
    [SerializeField] private WorldToScreenMarker _cubeCenterMarker = null!;
    [SerializeField] private WorldToScreenMarker _rotationCenterMarker = null!;

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
        if (_deselectOnEmptyClick == null) throw new ArgumentNullException(nameof(_deselectOnEmptyClick));
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
    }
    private void InitializeSceneComponents()
    {
        _mouseEventer.Initialize(_inputActions, _clickLeftHub, _clickRightHub, _holdRightHub, _holdMiddleHub, _scrollHub, _pointerHub, _deltaHub);
        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _cameraWork.Initialize(boardSize.ToWorldCenter(), _camera, _cubeCenterMarker, _rotationCenterMarker, _holdRightHub, _holdMiddleHub, _deltaHub, _scrollHub);

        _boardUI.Initialize(_flow, _eventBus, _selectionHub, _uiPointerProbe);

        _deselectOnEmptyClick.Initialize(_selectionHub, _clickLeftHub, _pointerHub, _uiPointerProbe, _camera);

        _mouseEventer.Run();
        _cameraWork.Run();
        _boardUI.Run();
        _deselectOnEmptyClick.Run();
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