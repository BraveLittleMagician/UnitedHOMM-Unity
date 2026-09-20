#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Stayables : IEquatable<Stayables>
{
    private readonly HashSet<int> _stayables = new();

    public Stayables() : this(0) { }
    public Stayables(int length) : this(length, Array.Empty<int>()) { }
    public Stayables(int length, IEnumerable<int> values)
    {
        if (length < 0) throw new InvalidDataException($"Переменная {nameof(length)} не может быть отрицательной");
        LastRelativeIndex = length;
        _stayables = values.Where(v => v >= 0 && v <= length).ToHashSet();
    }

    public int LastRelativeIndex { get; private set; }
    public int Count => _stayables.Count;
    public IReadOnlyCollection<int> All => _stayables;

    public void ExpandTo(int newLastIndex)
    {
        if (newLastIndex > LastRelativeIndex)
            LastRelativeIndex = newLastIndex;
    }
    public bool AddStayable(int offset)
    {
        if (offset < 0 || offset > LastRelativeIndex) return false;
        return _stayables.Add(offset);
    }
    public bool RemoveStayable(int offset)
    {
        if (offset < 0 || offset > LastRelativeIndex) return false;
        return _stayables.Remove(offset);
    }
    public bool Contains(int offset) => _stayables.Contains(offset);

    public bool Equals(Stayables? other)
    {
        if (other is null) return false;
        if (!LastRelativeIndex.Equals(other.LastRelativeIndex)) return false;
        return _stayables.SetEquals(other._stayables);
    }
    public override string ToString() => $"[{LastRelativeIndex}] {{{string.Join(",", _stayables.OrderBy(i => i))}}}";
    public override bool Equals(object? obj)
    {
        if (obj is not Stayables other) return false;
        return Equals(other);
    }
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(LastRelativeIndex);
        foreach (var v in _stayables.OrderBy(x => x))
            hash.Add(v);
        return hash.ToHashCode();
    }

    public static bool operator ==(Stayables? left, Stayables? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(Stayables? left, Stayables? right) => !(left == right);
}