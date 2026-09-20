#nullable enable

using System;

public interface IGameState
{
    bool IsGameStarted { get; }
    event Action? GameStarted;
    void StartGame();
}