#nullable enable

using System;

public class GameState : IGameStateManager
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