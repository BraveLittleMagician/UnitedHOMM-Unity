#nullable enable

using System.Collections.Generic;

public interface IGameView
{
    void BuildGrid(IReadOnlyDictionary<Axis, (int Min, int Max)> box);
}