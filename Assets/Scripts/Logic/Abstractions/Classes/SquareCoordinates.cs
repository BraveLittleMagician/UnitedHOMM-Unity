#nullable enable

using System.Collections;
using System.Collections.Generic;

public readonly struct SquareCoordinates : IReadOnlyDictionary<Axis, int>
{
    private readonly Square _square;

    public SquareCoordinates(Square square) => _square = square;

    public int this[Axis key] =>
        _square.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"Axis {key} отсутствует в Square");

    public IEnumerable<Axis> Keys
    {
        get
        {
            yield return Axis.X;
            yield return Axis.Y;
            yield return Axis.Z;
            yield return Axis.W;
        }
    }

    public IEnumerable<int> Values
    {
        get
        {
            yield return _square.X;
            yield return _square.Y;
            yield return _square.Z;
            yield return _square.W;
        }
    }

    public int Count => 4;

    public bool ContainsKey(Axis key) => key is Axis.X or Axis.Y or Axis.Z or Axis.W;

    public bool TryGetValue(Axis key, out int value) => _square.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<Axis, int>> GetEnumerator()
    {
        yield return new(Axis.X, _square.X);
        yield return new(Axis.Y, _square.Y);
        yield return new(Axis.Z, _square.Z);
        yield return new(Axis.W, _square.W);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}