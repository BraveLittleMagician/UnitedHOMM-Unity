#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CompositionRoot : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private GlobalDeselector _globalDeselector = null!;
    [SerializeField] private MouseEventer _clickEventer = null!;
    [SerializeField] private CameraWork _cameraWork = null!;
    [SerializeField] private GameView _gameView = null!;
    [SerializeField] private BoardUI _boardUI = null!;
    [SerializeField] private GridRenderer _gridRenderer = null!;
    [SerializeField] private GameObject _layerPrefab = null!;
    [SerializeField] private bool _autoStart = true;

    private Seats _seats; 
    private IGameConfigLoader _configLoader = null!;
    private BoardConfig _boardConfig = null!;
    private AxisAlignedBox _box = null!;
    private IEventBus _eventBus = null!;
    private ILogger _logger = null!;
    private IStateOfGame _stateManager = null!;
    private IHouse _house = null!;
    private IRegistry _registry = null!;
    private Board _board = null!;
    private BoardConfigUpdater _configUpdater = null!;
    private ICombatService _combatService = null!;
    private IMovementValidator _movementValidator = null!;
    private IAbilityService _abilityService = null!;
    private IFlow _flow = null!;
    private IGridBuilder _gridBuilder = null!;
    private IRoomInitializer _roomInitializer = null!;
    private IViewInitializer _viewInitializer = null!;
    private IPiecesSpawner _piecesSpawner = null!;
    private IStarterOfGame _gameStarter = null!;
    private GamePresenter _presenter = null!;

    private void ValidateReferences()
    {
        if (_globalDeselector == null) throw new ArgumentNullException(nameof(_globalDeselector));
        if (_clickEventer == null) throw new ArgumentNullException(nameof(_clickEventer));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_gameView == null) throw new ArgumentNullException(nameof(_gameView));
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
    }
    private void Awake()
    {
        ValidateReferences();
        BuildServices();
        WireMonoBehaviours();
    }
    private void Start()
    {
        _presenter.Start();
        if (_autoStart) _gameStarter.Start();
    }
    private void BuildServices()
    {
        _configLoader = new GameConfigLoader();
        _boardConfig = _configLoader.LoadBoardConfig();
        _seats = new Seats(_boardConfig.NumberOfSides, _boardConfig.NumberOfPlayersOnSide);
        _box = new AxisAlignedBox(
            (MultipleAxes)_boardConfig.Axes,
            _boardConfig.FieldSize,
            _boardConfig.WUp,
            _boardConfig.WDown);

        _eventBus = new EventBus();
        _logger = new LoggerForUnity();

        _stateManager = new StateOfGame();
        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);
        _board = new Board(_seats, _box, _eventBus, _logger);
        _configUpdater = new BoardConfigUpdater(_board, _eventBus, _logger, _boardConfig);

        _combatService = new CombatService(_logger, _eventBus);
        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);
        _flow = new Flow(_house, _eventBus, _logger, _combatService, _movementValidator, _abilityService, _registry);

        _gridBuilder = new GridBuilder(_layerPrefab);
        _roomInitializer = new RoomInitializer(_house, _eventBus, _logger, _seats, _board);
        _viewInitializer = new ViewInitializer(_gameView, _cameraWork, _board);

        _piecesSpawner = new PiecesSpawner(_flow, _logger, StartingPlacementPreset.CreateStandard());
        _gameStarter = new StarterOfGame(_stateManager, _roomInitializer, _viewInitializer, _piecesSpawner);

        _presenter = new GamePresenter(_flow, _gameView, _eventBus, _cameraWork, _registry, _configUpdater, _boardConfig);
    }

    private void WireMonoBehaviours()
    {
        _gridRenderer.Initialize(_eventBus, _stateManager, _gridBuilder);
        _cameraWork.Initialize();
        _boardUI.Initialize(_gameView, _gameStarter, _stateManager, _house, _boardConfig, _configUpdater);
    }
}