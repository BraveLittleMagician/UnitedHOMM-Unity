#nullable enable

using System;

public class GameState : IGameState
{
    public bool IsGameStarted { get; private set; }

    public void StartGame()
    {
        if (IsGameStarted) return;
        IsGameStarted = true;
        GameStarted?.Invoke();
    }

    public event Action? GameStarted;
}