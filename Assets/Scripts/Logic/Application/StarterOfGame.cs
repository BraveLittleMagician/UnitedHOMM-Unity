#nullable enable

public class StarterOfGame : IStarterOfGame
{
    private readonly IStateOfGame _stateManager;
    private readonly IRoomInitializer _roomInitializer;
    private readonly IViewInitializer _viewInitializer;
    private readonly IPiecesSpawner _piecesSpawner;
    private bool _started;

    public StarterOfGame(IStateOfGame stateManager, IRoomInitializer roomInitializer, IViewInitializer viewInitializer, IPiecesSpawner testPiecesSpawner) 
    {
        _stateManager = stateManager;
        _roomInitializer = roomInitializer;
        _viewInitializer = viewInitializer;
        _piecesSpawner = testPiecesSpawner;
    }

    public void StartGame()
    {
        if (_started) return;
        _started = true;

        _stateManager.StartGame();

        _roomInitializer.InitializeRooms();
        _viewInitializer.InitializeView();
        _piecesSpawner.SpawnPieces();
    }
}