#nullable enable

using System;
using UnityEngine;

public sealed class GameCompositionRoot : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private GlobalDeselector _globalDeselector = null!;
    [SerializeField] private MouseEventer _clickEventer = null!;
    [SerializeField] private CameraWork _cameraWork = null!;
    [SerializeField] private View _gameView = null!;
    [SerializeField] private BoardUI _boardUI = null!;
    [SerializeField] private GameObject _layerPrefab = null!;
    [SerializeField] private bool _autoStart = true;

    private IEventBus _eventBus = null!;
    private ILogger _logger = null!;
    private IHouse _house = null!;
    private IRegistry _registry = null!;
    private ICombatService _combatService = null!;
    private IMovementValidator _movementValidator = null!;
    private IAbilityService _abilityService = null!;
    private IFlow _flow = null!;
    private Seats _seats; 
    private ConfigOfBoard _configOfBoard = null!;
    private LoaderOfConfig _configLoader = null!;
    private AxisAlignedBox _box = null!;
    private StateOfGame _stateManager = null!;
    private Board _board = null!;
    private BoardConfigUpdater _configUpdater = null!;
    private AdderOfRoomsToHouse _roomInitializer = null!;
    private PiecesSpawner _piecesSpawner = null!;
    private StarterOfGame _starterOfGame = null!;
    private Presenter _presenter = null!;

    private void ValidateReferences()
    {
        if (_globalDeselector == null) throw new ArgumentNullException(nameof(_globalDeselector));
        if (_clickEventer == null) throw new ArgumentNullException(nameof(_clickEventer));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_gameView == null) throw new ArgumentNullException(nameof(_gameView));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
    }
    private void Awake()
    {
        ValidateReferences();
        BuildServices();
        Initialize();
    }
    private void Start()
    {
        _presenter.Start();
        if (_autoStart) _starterOfGame.StartGame();
    }
    private void BuildServices()
    {
        _configLoader = new LoaderOfConfig();
        _configOfBoard = _configLoader.LoadBoardConfig();
        _seats = new Seats(_configOfBoard.NumberOfSides, _configOfBoard.NumberOfPlayersOnSide);
        _box = new AxisAlignedBox(
            (MultipleAxes)_configOfBoard.Axes,
            _configOfBoard.FieldSize,
            _configOfBoard.WUp,
            _configOfBoard.WDown);

        _logger = new LoggerForUnity();
        _eventBus = new EventBus(_logger);

        _stateManager = new StateOfGame();
        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);
        _board = new Board(_seats, _box, _eventBus, _logger);
        _configUpdater = new BoardConfigUpdater(_board, _eventBus, _logger, _configOfBoard);

        _combatService = new CombatService(_logger, _eventBus);
        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);
        _flow = new Flow(_house, _eventBus, _logger, _combatService, _movementValidator, _abilityService, _registry);

        _roomInitializer = new AdderOfRoomsToHouse(_house, _eventBus, _logger, _seats, _board);

        _piecesSpawner = new PiecesSpawner(_flow, _logger, StartingPlacementPreset.CreateStandard());
        _starterOfGame = new StarterOfGame(_stateManager, _roomInitializer, _piecesSpawner);

        _presenter = new Presenter(_eventBus, _cameraWork, _configOfBoard);
    }

    private void Initialize()
    {
        _cameraWork.Initialize();
        _boardUI.Initialize(_stateManager);
    }
}