#nullable enable

using System.Collections;
using System.Collections.Generic;

public readonly struct SquareCoordinates<TSelf> where TSelf : struct, ISquare<TSelf>
{
    private readonly TSelf _square;

    public SquareCoordinates(TSelf square) => _square = square;

    public int Count => _square.CountOfAxes;
    public KeyValuePair<Axis, int> this[int index] => _square.GetCoordinateAt(index);
    public Enumerator GetEnumerator() => new(_square);

    public struct Enumerator
    {
        private readonly TSelf _square;
        private int _index;

        public Enumerator(TSelf square)
        {
            _square = square;
            _index = -1;
        }

        public readonly KeyValuePair<Axis, int> Current => _square.GetCoordinateAt(_index);
        public bool MoveNext() => ++_index < _square.CountOfAxes;
    }
}