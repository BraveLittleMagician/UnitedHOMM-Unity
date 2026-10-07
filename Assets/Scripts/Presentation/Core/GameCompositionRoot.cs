#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameCompositionRoot : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private Transform _piecesRoot = null!;
    [SerializeField] private GameObject _gridLayerPrefab = null!;
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

    // Infrastructure
    private ILogger _logger = null!;
    private IEventBus _eventBus = null!;

    // Data (из JSON)
    private PiecePrefabRegistry _piecePrefabs = null!;
    private ConfigOfBoard _configOfBoard = null!;
    private IReadOnlyList<StartingPlacement> _placements = null!;

    // Logic
    private Seats _seats;
    private Board _board = null!;
    private IHouse _house = null!;
    private IRegistry _registry = null!;
    private ICombatService _combatService = null!;
    private IMovementValidator _movementValidator = null!;
    private IAbilityService _abilityService = null!;
    private IFlow _flow = null!;

    private DeckSequenceProvider _deckSequences = null!;
    private Presenter _presenter = null!;

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
        _presenter?.Dispose();
        _deckSequences?.Dispose();
        _abilityService?.Dispose();
        _house?.Dispose();
    }

    private void ValidateReferences()
    {
        if (_piecesRoot == null) throw new ArgumentNullException(nameof(_piecesRoot));
        if (_gridLayerPrefab == null) throw new ArgumentNullException(nameof(_gridLayerPrefab));
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
        var templateRegistry = new PieceTemplateRegistry(templates);

        var layoutResult = BoardLayoutLoader.Load(_boardLayoutJson.text, templateRegistry);
        _configOfBoard = layoutResult.Config;
        _placements = layoutResult.Placements;

        _logger.Log($"Данные загружены: {_placements.Count} фигур, поле {_configOfBoard.FieldSize}, осей {_configOfBoard.Axes}");
    }

    private void CreateLogic()
    {
        _seats = new Seats(_configOfBoard.NumberOfSides, _configOfBoard.NumberOfPlayersOnSide);

        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);

        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _board = new Board(_seats, boardSize, _eventBus, _logger);
        _house.AddRoom(_board);
        _house.AddRoom(new Decks(_seats, _eventBus, _logger));

        _combatService = new CombatService(_logger, _eventBus);
        _movementValidator = new MovementValidator(_logger);
        _abilityService = new AbilityService(_eventBus, _logger);

        _flow = new Flow(_house, _eventBus, _logger, _combatService, _movementValidator, _abilityService);
    }

    private void CreatePresentation()
    {
        _presenter = new Presenter(_eventBus, _logger, _selectionHub, _uiPointerProbe, _piecePrefabs, _piecesRoot);
        _presenter.SetMaterial(_pieceMaterial);
    }

    private void InitializeSceneComponents()
    {
        _mouseEventer.Initialize(_inputActions, _clickLeftHub, _clickRightHub, _holdRightHub, _holdMiddleHub, _scrollHub, _pointerHub, _deltaHub);

        _cameraWork.Initialize(_piecesRoot, _camera, _cubeCenterMarker, _rotationCenterMarker, _holdRightHub, _holdMiddleHub, _deltaHub, _scrollHub);

        _boardUI.Initialize(_flow, _eventBus, _uiPointerProbe);

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
    }
}