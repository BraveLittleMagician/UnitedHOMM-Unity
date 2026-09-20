#nullable enable

public class StarterOfGame : IGameStarter
{
    private readonly IGameStateManager _stateManager;
    private readonly IRoomInitializer _roomInitializer;
    private readonly IViewInitializer _viewInitializer;
    private readonly IPiecesSpawner _piecesSpawner;
    private bool _started;

    public StarterOfGame(IGameStateManager stateManager, IRoomInitializer roomInitializer, IViewInitializer viewInitializer, IPiecesSpawner testPiecesSpawner) 
    {
        _stateManager = stateManager;
        _roomInitializer = roomInitializer;
        _viewInitializer = viewInitializer;
        _piecesSpawner = testPiecesSpawner;
    }

    public void Start()
    {
        if (_started) return;
        _started = true;

        _stateManager.StartGame();

        _roomInitializer.InitializeRooms();
        _viewInitializer.InitializeView();
        _piecesSpawner.SpawnPieces();
    }
}