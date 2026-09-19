#nullable enable

using System.Collections.Generic;

public interface ICamera
{
    void FitToBoard(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds);
}