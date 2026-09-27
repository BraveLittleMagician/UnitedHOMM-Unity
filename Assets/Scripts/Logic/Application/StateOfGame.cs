#nullable enable

using System;

public class StateOfGame
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public StateOfGame(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsGameStarted { get; private set; }

    public void StartGame()
    {
        if (IsGameStarted) return;
        IsGameStarted = true;
        GameStarted?.Invoke();
        _eventBus.Publish(new GameStartedEvent());
        _logger.Log("Игра началась");
    }

    public event Action? GameStarted;
}