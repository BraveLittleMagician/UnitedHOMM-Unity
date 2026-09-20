#nullable enable

using System;

public interface IStateOfGame
{
    bool IsGameStarted { get; }
    event Action? GameStarted;
    void StartGame();
}