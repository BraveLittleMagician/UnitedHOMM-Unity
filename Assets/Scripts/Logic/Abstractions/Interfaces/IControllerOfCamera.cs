#nullable enable

using System.Collections.Generic;

public interface IControllerOfCamera
{
    void FitToBoard(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds);
}