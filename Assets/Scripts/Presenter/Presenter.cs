#nullable enable

using System;

public class Presenter : IDisposable
{
    private readonly IFlow _flow;
    private readonly IGameView _view;
    private readonly IEventBus _eventBus;
    private readonly ICamera _camera;
    private readonly IRegistry _registry;
    private readonly BoardConfigUpdater _configUpdater;
    private BoardConfig _currentConfig;

    public Presenter(IFlow flow, IGameView view, IEventBus eventBus, ICamera camera, IRegistry registry, BoardConfigUpdater configUpdater, BoardConfig initialConfig)
    {
        _flow = flow;
        _view = view;
        _eventBus = eventBus;
        _camera = camera;
        _registry = registry;
        _configUpdater = configUpdater;
        _currentConfig = initialConfig;

        _view.BoardSizeInputChanged += OnBoardSizeInputChanged;
        
        _eventBus.Subscribe<PieceSelectedEvent<Square>>(OnPieceSelectedEvent);
        _eventBus.Subscribe<PieceSelectedEvent<int>>(OnPieceSelectedEvent);
        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }


    private void OnBoardSizeInputChanged(int newSize)
    {
        if (newSize <= 0) return;
        var newConfig = _currentConfig with { FieldSize = newSize };
        _configUpdater.UpdateConfig(newConfig);
    }
    private void OnPieceSelectedEvent(PieceSelectedEvent<Square> e) { }
    private void OnPieceSelectedEvent(PieceSelectedEvent<int> e) { }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        _currentConfig = e.NewConfig;
        _view.SetBoardSize(e.NewConfig.FieldSize);
        var axes = (MultipleAxes)_currentConfig.Axes;
        var box = new AxisAlignedBox(axes, _currentConfig.FieldSize, e.NewConfig.WUp, e.NewConfig.WDown);
        _camera.FitToBoard(box.Bounds);
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e) { }
    private void OnPieceDeployed(PieceDeployedEvent<int> e) { }
    private void OnPieceMoved(PieceMovedEvent<Square> e) { }
    private void OnPieceMoved(PieceMovedEvent<int> e) { }
    private void OnPieceDied(PieceDiedEvent e) { }

    public void Start()
    {
        var axes = (MultipleAxes)_currentConfig.Axes;
        var box = new AxisAlignedBox(axes, _currentConfig.FieldSize, _currentConfig.WUp, _currentConfig.WDown);
        _camera.FitToBoard(box.Bounds);
    }
    public void Dispose()
    {
        _view.BoardSizeInputChanged -= OnBoardSizeInputChanged;
        _eventBus.Unsubscribe<PieceSelectedEvent<Square>>(OnPieceSelectedEvent);
        _eventBus.Unsubscribe<PieceSelectedEvent<int>>(OnPieceSelectedEvent);
        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }
}