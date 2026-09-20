#nullable enable

using System.Collections.Generic;

public interface ICameraWork
{
    void FitToBoard(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds);
}