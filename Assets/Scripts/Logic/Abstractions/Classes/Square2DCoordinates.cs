#nullable enable

using System.Collections;
using System.Collections.Generic;

public readonly struct Square2DCoordinates : IReadOnlyDictionary<Axis, int>
{
    private readonly Square2D _square;

    public Square2DCoordinates(Square2D square) => _square = square;

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
        }
    }

    public IEnumerable<int> Values
    {
        get
        {
            yield return _square.X;
            yield return _square.Y;
        }
    }

    public int Count => 2;

    public bool ContainsKey(Axis key) => key is Axis.X or Axis.Y;

    public bool TryGetValue(Axis key, out int value) => _square.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<Axis, int>> GetEnumerator()
    {
        yield return new(Axis.X, _square.X);
        yield return new(Axis.Y, _square.Y);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}