#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameCompositionRoot : MonoBehaviour
{
    private const int MaxShapeRepeat = 64;

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
    private PieceEventBus _gameEventBus = null!;
    private PiecePrefabRegistry _piecePrefabs = null!;
    private PieceLibraryDto _pieceLibraryDto = null!;
    private ConfigOfBoard _configOfBoard = null!;
    private IReadOnlyList<StartingPlacement> _placements = null!;
    private Seats _seats;
    private Board _board = null!;
    private IHouse _house = null!;
    private IRegistry _registry = null!;
    private IAbilityService _abilityService = null!;
    private IFlow _flow = null!;
    private PieceRegistry _pieceRegistry = null!;
    private TurnManager _turnManager = null!;
    private MatchState _matchState = null!;
    private Graveyard _graveyard = null!;
    private ModifierHost _modifierHost = null!;
    private IMovementResolver _movementValidator = null!;
    private PieceTemplateRegistry _templateRegistry = null!;
    private Presenter _presenter = null!;

    private void Awake()
    {
        ValidateReferences();
        CreateInfrastructure();
        LoadData();
        CreateLogic();
        CreatePresentation();
        InitializeSceneComponents();
        ComposeTurnHooks();
    }

    private void Start()
    {
        StartGame();
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
        _abilityService?.Dispose();
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

        _pieceLibraryDto = LoaderOfDataFromJSON.LoadFromJson<PieceLibraryDto>(_pieceLibraryJson.text)
            ?? throw new InvalidOperationException(
                "Не удалось загрузить PieceLibraryData как PieceLibraryDto.");

        var templates = StandardPieceTemplates.CreateAll(_eventBus);
        _templateRegistry = new PieceTemplateRegistry(templates);   // ← было локальной переменной

        var layoutResult = BoardLayoutLoader.Load(_boardLayoutJson.text, _templateRegistry);
        _configOfBoard = layoutResult.Config;
        _placements = layoutResult.Placements;

        _logger.Log(
            $"Данные загружены: {_placements.Count} фигур, поле {_configOfBoard.FieldSize}, " +
            $"осей {_configOfBoard.Axes}");
    }

    private void CreateLogic()
    {
        // --- Piece-registry и turn/match (P2) ---
        _pieceRegistry = new PieceRegistry();
        _gameEventBus = new PieceEventBus(_pieceRegistry);
        _matchState = new MatchState();
        _graveyard = new Graveyard(_pieceRegistry);

        var playersInOrder = _placements
            .Select(p => p.Definition.Owner)
            .Distinct()
            .ToList();
        _turnManager = new TurnManager(playersInOrder);

        // --- Modifier host (P2) ---
        _modifierHost = new ModifierHost(_pieceRegistry, _turnManager);

        // --- House / rooms (P1) ---
        _seats = new Seats(_configOfBoard.NumberOfSides, _configOfBoard.NumberOfPlayersOnSide);
        _house = new House(_seats, _eventBus, _logger);
        _registry = new Registry(_house);

        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _board = new Board(_seats, boardSize, _eventBus, _logger);
        _house.AddRoom(_board);
        _house.AddRoom(new Decks(_seats, _eventBus, _logger));

        // --- Movement (Phase 5) ---
        var shapeCatalog = new PieceShapeCatalog();
        RegisterShapes(shapeCatalog);
        var resolvers = BuildResolverDictionary(shapeCatalog, _pieceLibraryDto);
        _movementValidator = new PieceMoveValidator(resolvers, _modifierHost);

        // --- Ability service (P1) ---
        _abilityService = new AbilityService(_eventBus, _logger);

        // --- Flow (P1 + Phase 5) ---
        _flow = new Flow(
            _house,
            _eventBus,
            _logger,
            _movementValidator,
            _abilityService,
            _pieceRegistry,
            () => new Vector3Int(_board.Size.X, _board.Size.Y, _board.Size.Z));
    }

    private static void RegisterShapes(PieceShapeCatalog shapes)
    {
        shapes.RegisterPattern("PawnMove", PawnRules.CreateMoveShape());
        shapes.RegisterPattern("PawnAttack", PawnRules.CreateAttackShape());

        var kingArea = new RelativeArea<Square>(
            MultipleAxes.Three,
            ImmutableHashSet.Create(1),
            length: 1,
            isCircle: false,
            guarantee: true);
        shapes.RegisterArea("KingMove", kingArea);
        shapes.RegisterArea("KingAttack", kingArea);

        shapes.RegisterPathSet("Rook", new PathSetPattern(RookRules.Create(), MaxShapeRepeat));
        shapes.RegisterPathSet("Knight", new PathSetPattern(KnightRules.Create(), MaxShapeRepeat));
        shapes.RegisterPathSet("Bishop", new PathSetPattern(BishopRules.Create(), MaxShapeRepeat));
        shapes.RegisterPathSet("Queen", new PathSetPattern(QueenRules.Create(), MaxShapeRepeat));
    }

    private Dictionary<string, IMovementResolver> BuildResolverDictionary(PieceShapeCatalog shapes, PieceLibraryDto libraryDto)
    {
        var resolvers = new Dictionary<string, IMovementResolver>(StringComparer.Ordinal);
        var missing = new List<string>();

        foreach (var dto in libraryDto.Pieces)
        {
            if (!_templateRegistry.TryGet(dto.Name, out var template) || template is null)
            {
                missing.Add(dto.Name);
                continue;
            }

            resolvers[dto.Name] = shapes.BuildResolver(
                template.MoveShapeName,
                template.AttackShapeName,
                _pieceRegistry,
                _modifierHost);
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Фигуры из PieceLibraryData.json отсутствуют в StandardPieceTemplates: " +
                $"{string.Join(", ", missing)}. " +
                $"Добавьте их в StandardPieceTemplates.CreateAll.");
        }

        return resolvers;
    }

    private void ComposeTurnHooks()
    {
        _turnManager.TurnStarted += player =>
        {
            foreach (var room in _house.Rooms.Values)
            {
                foreach (var piece in room.GetAllPieces())
                {
                    if (piece.Owner.Equals(player))
                        piece.ResetTurnState();
                }
            }
        };
    }

    private void CreatePresentation()
    {
        _presenter = new Presenter(
            _eventBus,
            _logger,
            _selectionHub,
            _uiPointerProbe,
            _piecePrefabs,
            _cameraWork,
            _seats,
            _piecesRoot);
        _presenter.SetMaterial(_pieceMaterial);
    }

    private void InitializeSceneComponents()
    {
        _mouseEventer.Initialize(
            _inputActions, _clickLeftHub, _clickRightHub,
            _holdRightHub, _holdMiddleHub, _scrollHub, _pointerHub, _deltaHub);

        var boardSize = BoardSizeFactory.FromConfig(_configOfBoard);
        _cameraWork.Initialize(
            boardSize.ToWorldCenter(),
            _camera,
            _cubeCenterMarker,
            _rotationCenterMarker,
            _holdRightHub,
            _holdMiddleHub,
            _deltaHub,
            _scrollHub);

        _boardUI.Initialize(_flow, _eventBus, _selectionHub, _uiPointerProbe);
        _deselectOnEmptyClick.Initialize(_selectionHub, _clickLeftHub, _pointerHub, _uiPointerProbe, _camera);

        _mouseEventer.Run();
        _cameraWork.Run();
        _boardUI.Run();
        _deselectOnEmptyClick.Run();
    }

    private void StartGame()
    {
        _turnManager.Start();

        foreach (var placement in _placements)
        {
            var result = _flow.AddPiece<Board, Square>(placement.Definition, placement.Position);
            if (result.IsSuccess) continue;

            _logger.LogError(
                $"Не удалось развернуть '{placement.Definition.Name}' в {placement.Position}: " +
                result.Error);
        }

        _logger.Log("Все фигуры развёрнуты");
    }
}