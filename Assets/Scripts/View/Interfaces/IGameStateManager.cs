#nullable enable

using System;

public interface IGameStateManager
{
    bool IsGameStarted { get; }
    event Action? GameStarted;
    void StartGame();
}