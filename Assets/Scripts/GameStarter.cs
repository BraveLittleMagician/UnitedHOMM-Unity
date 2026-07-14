#nullable enable

public class GameStarter : IGameStarter
{
    private readonly IRoomInitializer _roomInitializer;
    private readonly IViewInitializer _viewInitializer;
    private readonly IPiecesSpawner _piecesSpawner;
    private bool _started;

    public GameStarter(IRoomInitializer roomInitializer, IViewInitializer viewInitializer, IPiecesSpawner testPiecesSpawner) 
    {
        _roomInitializer = roomInitializer;
        _viewInitializer = viewInitializer;
        _piecesSpawner = testPiecesSpawner;
    }

    public void StartGame()
    {
        if (_started) return;
        _started = true;

        _roomInitializer.InitializeRooms();
        _viewInitializer.InitializeView();
        _piecesSpawner.SpawnPieces();
    }
}