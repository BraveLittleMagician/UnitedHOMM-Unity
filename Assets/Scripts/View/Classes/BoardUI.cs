#nullable enable

using UnityEngine;

public class BoardUI : MonoBehaviour
{
    private IGameView _gameView = null!;
    private IGameStarter _gameStarter = null!;
    private IGameStateManager _stateManager = null!;
    private IHouse _house = null!;
    private BoardConfig _boardConfig = null!;
    private BoardConfigUpdater _configUpdater = null!;

    public void Initialize(
        IGameView gameView,
        IGameStarter gameStarter,
        IGameStateManager stateManager,
        IHouse house,
        BoardConfig boardConfig,
        BoardConfigUpdater configUpdater)
    {
        _gameView = gameView;
        _gameStarter = gameStarter;
        _stateManager = stateManager;
        _house = house;
        _boardConfig = boardConfig;
        _configUpdater = configUpdater;
    }

    private void OnGUI()
    {
        if (_stateManager == null) return;
    }
}