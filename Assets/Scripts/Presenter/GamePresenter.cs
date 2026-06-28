using VContainer.Unity;

public class GamePresenter : IStartable
{
    private readonly IGameController _controller;
    private readonly IGameView _view;
    private readonly IEventBus _eventBus;

    public GamePresenter(IGameController controller, IGameView view, IEventBus eventBus)
    {
        _controller = controller;
        _view = view;
        _eventBus = eventBus;

        _eventBus.Subscribe<PieceDeployedEvent>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
    }

    private void OnPieceDeployed(PieceDeployedEvent e) => _view.ShowPiece(e.Piece);
    private void OnPieceDied(PieceDiedEvent e) => _view.HidePiece(e.Piece);

    public void Start()
    {
    }
}