#nullable enable

using System;
using UnityEngine;

public sealed class GameCompositionRoot : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private View _view = null!;

    [Header("UI")]
    [SerializeField] private BoardUI _boardUI = null!;

    [Header("Selection")]
    [SerializeField] private SelectionHub _selectionHub = null!;
    [SerializeField] private DeselectOnEmptyClick _deselectOnEmptyClick = null!;

    [Header("Camera")]
    [SerializeField] private CameraWork _cameraWork = null!;

    [Header("Input")]
    [SerializeField] private MouseEventer _mouseEventer = null!;

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
    private Board _board = null!;
    private LoaderOfConfig _loaderOfConfig = null!;
    private AxisAlignedBox _box = null!;
    private StateOfGame _stateOfGame = null!;
    private AdderOfRoomsToHouse _adderOfRooms = null!;
    private SpawnerOfPieces _spawnerOfPieces = null!;
    private StarterOfGame _starterOfGame = null!;
    private Presenter _presenter = null!;

    private void ValidateReferences()
    {
        if (_deselectOnEmptyClick == null) throw new ArgumentNullException(nameof(_deselectOnEmptyClick));
        if (_mouseEventer == null) throw new ArgumentNullException(nameof(_mouseEventer));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_view == null) throw new ArgumentNullException(nameof(_view));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
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
        _starterOfGame.StartGame();
    }
    private void BuildServices()
    {
        _loaderOfConfig = new LoaderOfConfig();
        _configOfBoard = _loaderOfConfig.LoadBoardConfig();
        _seats = new Seats(_configOfBoard.NumberOfSides, _configOfBoard.NumberOfPlayersOnSide);
        _box = new AxisAlignedBox(
            (MultipleAxes)_configOfBoard.Axes,
            _configOfBoard.FieldSize,
            _configOfBoard.WUp,
            _configOfBoard.WDown);

        _logger = new LoggerForUnity();
        _eventBus = new EventBus(_logger);

        _stateOfGame = new StateOfGame();
        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);
        _board = new Board(_seats, _box, _eventBus, _logger);

        _combatService = new CombatService(_logger, _eventBus);
        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);
        _flow = new Flow(_house, _eventBus, _logger, _combatService, _movementValidator, _abilityService, _registry);

        _adderOfRooms = new AdderOfRoomsToHouse(_house, _eventBus, _logger, _seats, _board);

        _spawnerOfPieces = new SpawnerOfPieces(_flow, _logger, StartingPlacementPreset.CreateStandard());
        _starterOfGame = new StarterOfGame(_stateOfGame, _adderOfRooms, _spawnerOfPieces);

        _presenter = new Presenter(_eventBus, _cameraWork, _configOfBoard);
    }

    private void Initialize()
    {
        _cameraWork.Initialize();
        _boardUI.Initialize(_stateOfGame);
    }
}