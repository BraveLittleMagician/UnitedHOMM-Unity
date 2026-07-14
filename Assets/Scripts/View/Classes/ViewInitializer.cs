#nullable enable

public sealed class ViewInitializer : IViewInitializer
{
    private readonly IGameView _gameView;
    private readonly IControllerOfCamera _camera;
    private readonly IEventBus _eventBus;
    private readonly BoardConfig _boardConfig;
    private readonly Board _board;

    public ViewInitializer(IGameView gameView, IControllerOfCamera camera, IEventBus eventBus, BoardConfig boardConfig, Board board)
    {
        _gameView = gameView;
        _camera = camera;
        _eventBus = eventBus;
        _boardConfig = boardConfig;
        _board = board;
    }

    public void InitializeView()
    {
        if (_gameView is GameView view)
        {
            var axes = (MultipleAxes)_boardConfig.Axes;
            var box = new AxisAlignedBox(axes, _boardConfig.FieldSize, 0);
            view.BuildGrid(box, axes);
        }

        _camera.FitToBoard(_board.Bounds);
    }
}