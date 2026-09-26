#nullable enable

public class StarterOfGame
{
    private readonly StateOfGame _stateOfGame;
    private readonly AdderOfRoomsToHouse _adderRoomsToHouse;
    private readonly SpawnerOfPieces _spawnerOfPieces;
    private bool _started;

    public StarterOfGame(StateOfGame stateManager, AdderOfRoomsToHouse roomInitializer, SpawnerOfPieces testPiecesSpawner) 
    {
        _stateOfGame = stateManager;
        _adderRoomsToHouse = roomInitializer;
        _spawnerOfPieces = testPiecesSpawner;
    }

    public bool StartGame()
    {
        if (_started) return false;
        _started = true;
        _stateOfGame.StartGame();
        _adderRoomsToHouse.AddNecessaryRooms();
        _spawnerOfPieces.SpawnPieces();
        return true;
    }
}