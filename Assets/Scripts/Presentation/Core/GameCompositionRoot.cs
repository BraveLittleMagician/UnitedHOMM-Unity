#nullable enable
using System;
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

    [Header("Debug")]
    [SerializeField] private PieceDebugLogger _pieceDebugLogger = null!;
    [SerializeField] private BoardDebugDumper _boardDebugDumper = null!;

    private ILogger _logger = null!;
    private IEventBus _eventBus = null!;

    private InputContext _input = null!;
    private DomainContext _domain = null!;
    private PresentationContext _presentation = null!;
    private UiContext _ui = null!;
    private DebugContext _debug = null!;

    private void Awake()
    {
        ValidateReferences();

        // Infrastructure
        _logger = new LoggerForUnity();
        _eventBus = new EventBus(_logger);

        // Modules
        _input = InputModule.Initialize(
            _mouseEventer,
            _inputActions,
            _clickLeftHub, _clickRightHub,
            _holdRightHub, _holdMiddleHub,
            _scrollHub, _pointerHub, _deltaHub);

        _domain = DomainModule.Initialize(
            _eventBus, _logger,
            _boardLayoutJson, _pieceLibraryJson);

        _presentation = PresentationModule.Initialize(
            _eventBus, _logger, _domain, gameObject,
            _piecesRoot, _pieceMaterial, _markerMaterial, _layerMaterial,
            _camera, _layerCamera, _cameraWork,
            _cubeCenterMarker, _rotationCenterMarker,
            _selectionHub,
            _clickLeftHub, _pointerHub,
            _holdRightHub, _holdMiddleHub, _deltaHub, _scrollHub,
            _uiPointerProbe);

        _ui = UiModule.Initialize(
            _domain, _presentation,
            _boardUI, _layerPanelUI, _planUI, _promotionUI, _actionChoiceUI,
            _uiPointerProbe, _logger);

        _debug = DebugModule.Initialize(
            _domain, _presentation, _selectionHub,
            _pieceDebugLogger, _boardDebugDumper,
            _camera, _layerCamera,
            _eventBus, _logger);

        _input.Run();
        _presentation.Run();
        _ui.Run();
        _debug.Run();
    }

    private void Start() => StartGame();

    private void OnDestroy()
    {
        _presentation?.Dispose();
        _domain?.Dispose();
    }

    private void StartGame()
    {
        foreach (var placement in _domain.Placements)
        {
            var result = _domain.Flow.AddPiece<Board, Square>(placement.Definition, placement.Position);
            if (result.IsSuccess) continue;
            _logger.LogError($"Failed to spawn '{placement.Definition.Name}' at {placement.Position}: {result.Error}");
        }
        _logger.Log("All pieces spawned");

        _domain.TurnManager.Start();
    }

    private void ValidateReferences()
    {
        if (_piecesRoot == null) throw new ArgumentNullException(nameof(_piecesRoot));
        if (_pieceMaterial == null) throw new ArgumentNullException(nameof(_pieceMaterial));
        if (_boardUI == null) throw new ArgumentNullException(nameof(_boardUI));
        if (_layerPanelUI == null) throw new ArgumentNullException(nameof(_layerPanelUI));
        if (_planUI == null) throw new ArgumentNullException(nameof(_planUI));
        if (_promotionUI == null) throw new ArgumentNullException(nameof(_promotionUI));
        if (_actionChoiceUI == null) throw new ArgumentNullException(nameof(_actionChoiceUI));
        if (_uiPointerProbe == null) throw new ArgumentNullException(nameof(_uiPointerProbe));
        if (_selectionHub == null) throw new ArgumentNullException(nameof(_selectionHub));
        if (_camera == null) throw new ArgumentNullException(nameof(_camera));
        if (_cameraWork == null) throw new ArgumentNullException(nameof(_cameraWork));
        if (_cubeCenterMarker == null) throw new ArgumentNullException(nameof(_cubeCenterMarker));
        if (_rotationCenterMarker == null) throw new ArgumentNullException(nameof(_rotationCenterMarker));
        if (_layerCamera == null) throw new ArgumentNullException(nameof(_layerCamera));
        if (_layerMaterial == null) throw new ArgumentNullException(nameof(_layerMaterial));
        if (_markerMaterial == null) throw new ArgumentNullException(nameof(_markerMaterial));
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
}