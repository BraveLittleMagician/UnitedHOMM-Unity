#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private Decks _decks  = null!;
    private LoaderOfConfig _loaderOfConfig = null!;
    private AxisAlignedBox _box = null!;
    private StateOfGame _stateOfGame = null!;
    private AdderOfRoomsToHouse _adderOfRooms = null!;
    private SpawnerOfPieces _spawnerOfPieces = null!;
    private StarterOfGame _starterOfGame = null!;
    private Presenter _presenter = null!;

    private void Awake()
    {
        ValidateReferences();

        CreateSystems();

        Initialize();

        Run();
    }
    private void Start()
    {
        _presenter.Start();
        _starterOfGame.Start();
    }

    private void ValidateReferences()
    {
        if (_view == null) throw new ArgumentNullException(nameof(_view));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
        if (_selectionHub == null) throw new ArgumentNullException(nameof(_selectionHub));
        if (_deselectOnEmptyClick == null) throw new ArgumentNullException(nameof(_deselectOnEmptyClick));
        if (_camera == null) throw new ArgumentNullException(nameof(_camera));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_cubeCenterMarker == null) throw new ArgumentNullException(nameof(_cubeCenterMarker));
        if (_rotationCenterMarker == null) throw new ArgumentNullException(nameof(_rotationCenterMarker));
        if (_mouseEventer == null) throw new ArgumentNullException(nameof(_mouseEventer));
        if (_clickLeftHub == null) throw new ArgumentNullException(nameof(_clickLeftHub));
        if (_clickRightHub == null) throw new ArgumentNullException(nameof(_clickRightHub));
        if (_holdRightHub == null) throw new ArgumentNullException(nameof(_holdRightHub));
        if (_holdMiddleHub == null) throw new ArgumentNullException(nameof(_holdMiddleHub));
        if (_scrollHub == null) throw new ArgumentNullException(nameof(_scrollHub));
        if (_deltaHub == null) throw new ArgumentNullException(nameof(_deltaHub));
        if (_pointerHub == null) throw new ArgumentNullException(nameof(_pointerHub));
    }
    private void CreateSystems()
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

        _stateOfGame = new StateOfGame(_eventBus, _logger);
        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);
        _board = new Board(_seats, _box, _eventBus, _logger);
        _decks = new Decks(_seats, _eventBus, _logger);

        _combatService = new CombatService(_logger, _eventBus);
        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);
        _flow = new Flow(_house, _eventBus, _logger, _combatService, _movementValidator, _abilityService);

        _adderOfRooms = new AdderOfRoomsToHouse(_house, _eventBus, _logger, _seats, _board, _decks);

        _spawnerOfPieces = new SpawnerOfPieces(_flow, _logger, StartingPlacementPreset.CreateStandard());
        _starterOfGame = new StarterOfGame(_stateOfGame, _adderOfRooms, _spawnerOfPieces);

        _presenter = new Presenter(_eventBus, _cameraWork, _configOfBoard);
    }
    private void Initialize()
    {
        _mouseEventer.Initialize(_inputActions, _clickLeftHub, _clickRightHub, _holdRightHub, _holdMiddleHub, _scrollHub, _pointerHub, _deltaHub);
        _cameraWork.Initialize(_camera, _cubeCenterMarker, _rotationCenterMarker, _holdRightHub, _holdMiddleHub, _deltaHub, _scrollHub);
        _boardUI.Initialize(_stateOfGame);
    }
    private void Run() { }
}