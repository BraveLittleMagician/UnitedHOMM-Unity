#nullable enable

public sealed class ViewInitializer : IViewInitializer
{
    private readonly IGameView _gameView;
    private readonly IControllerOfCamera _camera;
    private readonly BoardConfig _boardConfig;
    private readonly Board _board;

    public ViewInitializer(IGameView gameView, IControllerOfCamera camera, BoardConfig boardConfig, Board board)
    {
        _gameView = gameView;
        _camera = camera;
        _boardConfig = boardConfig;
        _board = board;
    }

    public void InitializeView()
    {
        var axes = (MultipleAxes)_boardConfig.Axes;
        var box = new AxisAlignedBox(axes, _boardConfig.FieldSize, _boardConfig.WUp, _boardConfig.WDown);
        _gameView.BuildGrid(box);

        _camera.FitToBoard(_board.Bounds);
    }
}