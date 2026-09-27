#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct RelativeArea<TSquare> : ISequence<TSquare, RelativeArea<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    private readonly int _cachedHash;

    public RelativeArea(IImmutableSet<int> stayables, int length, bool isCircle, bool guarantee)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length не может быть отрицательным");

        if (guarantee)
        {
            length = Math.Max(length, 1);
            stayables = stayables.Add(1).Where(i => i > 0 && i <= length).ToImmutableHashSet();
        }

        Length = length;
        IsCircle = isCircle;
        Stayables = stayables;
        GuaranteesAtLeastOneStayable = guarantee;

        var hash = new HashCode();
        hash.Add(GuaranteesAtLeastOneStayable);
        hash.Add(IsCircle);
        hash.Add(length);
        foreach (var item in stayables) hash.Add(item);
        _cachedHash = hash.ToHashCode();
    }
    public RelativeArea(bool isCircle) : this(ImmutableHashSet.Create(0, 1), 1, isCircle, guarantee: true) { }

    public bool GuaranteesAtLeastOneStayable { get; }
    public bool IsCircle { get; }
    public int Length { get; }
    public IImmutableSet<int> Stayables { get; }

    private readonly List<Stayable> BuildStayableList()
    {
        var result = new List<Stayable>(Length);
        for (int r = 1; r <= Length; r++)
            result.Add(Stayables.Contains(r) ? Stayable.Stay : Stayable.NotStay);
        return result;
    }

    public RelativeArea<TSquare> WithIncreasedRadius()
    {
        int newLength = Length + 1;
        return new RelativeArea<TSquare>(Stayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithDecreasedRadius()
    {
        if (Length <= 1) return this;
        if (Stayables.Count == 1 && Stayables.Contains(Length)) return this;
        int newLength = Length - 1;
        var newStayables = Stayables.Where(r => r <= newLength).ToImmutableHashSet();
        return new (newStayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithAddedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (Stayables.Contains(radius)) return this;
        return new (Stayables.Add(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithRemovedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (!Stayables.Contains(radius)) return this; 
        if (Stayables.Count == 1 && Stayables.Contains(radius)) return this;
        return new RelativeArea<TSquare>(Stayables.Remove(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithCircle(bool isCircle) => new (Stayables, Length, isCircle, GuaranteesAtLeastOneStayable);

    public (IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithIncreasedRadius()
    {
        int newLength = Length + 1;
        return (Stayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public (IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithDecreasedRadius()
    {
        var data = (Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (Length < 1) return data;
        int newLength = Length - 1;
        var newStayables = Stayables.Where(r => r <= newLength).ToImmutableHashSet();
        return (newStayables, newLength, IsCircle, false);
    }
    public (IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithAddedStayable(int radius)
    {
        var data = (Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (radius < 0 || radius > Length) return data;
        if (Stayables.Contains(radius)) return data;
        return (Stayables.Add(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public (IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithRemovedStayable(int radius)
    {
        var data = (Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (radius < 0 || radius > Length) return data;
        if (!Stayables.Contains(radius)) return data;
        return (Stayables.Remove(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public (IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithCircle(bool isCircle) => (Stayables, Length, isCircle, GuaranteesAtLeastOneStayable);

    public readonly ISequenceEnumerator<TSquare> GetEnumerator(TSquare start) => 
        IsCircle ? 
            new EnumerableCircle(BuildStayableList(), start): 
            new EnumerableSquare(BuildStayableList(), start);
    public RelativeArea<TSquare> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public bool Equals(RelativeArea<TSquare> other)
    {
        if (ReferenceEquals(Stayables, other.Stayables)) return true;
        if (GuaranteesAtLeastOneStayable != other.GuaranteesAtLeastOneStayable || IsCircle != other.IsCircle || Length != other.Length) return false;
        if (Stayables == null && other.Stayables == null) return true;
        if (Stayables == null || other.Stayables == null) return false;
        return Stayables.SetEquals(other.Stayables);
    }
    public override bool Equals(object? obj) => obj is RelativeArea<TSquare> other && Equals(other);
    public override int GetHashCode() => _cachedHash;

    public static bool operator ==(RelativeArea<TSquare> left, RelativeArea<TSquare> right) => left.Equals(right);
    public static bool operator !=(RelativeArea<TSquare> left, RelativeArea<TSquare> right) => !(left == right);

    private class EnumerableSquare : SequenceEnumerable<TSquare>
    {
        private readonly List<Stayable> _stayables;
        private List<TSquare>? _allStayablesCache = null;
        private Dictionary<TSquare, Stayable>? _allPositionsCache = null;

        public EnumerableSquare(List<Stayable> stayables, TSquare startSquare) : base(startSquare) => _stayables = stayables;

        protected override List<TSquare> CreateOnlyStayablesPositions
        {
            get
            {
                return _allStayablesCache ??= FieldExtensions.AdjacentStayables(StartPosition.ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
            }
        }
        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions
        {
            get
            {
                return _allPositionsCache ??= FieldExtensions.AdjacentPositions(StartPosition.ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);
            }
        }

        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            TSquare start = StartPosition;
            foreach (var pathSquare in path.Positions)
                if (!start.IsWithinSquareRadius(pathSquare, _stayables.Count)) return false;
            return true;
        }
        public override bool CanMoveTo(TSquare target)
        {
            if (!StartPosition.IsWithinSquareRadius(target, _stayables.Count)) return false;
            IEnumerable<TSquare> adjacentSquares = FieldExtensions.AdjacentStayables(StartPosition.ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
    }
    private class EnumerableCircle : SequenceEnumerable<TSquare>
    {
        private readonly List<Stayable> _stayables;
        private List<TSquare>? _allStayablesCache = null;
        private Dictionary<TSquare, Stayable>? _allPositionsCache = null;

        public EnumerableCircle(List<Stayable> stayables, TSquare startSquare) : base(startSquare) => _stayables = stayables;

        protected override List<TSquare> CreateOnlyStayablesPositions
        {
            get
            {
                return _allStayablesCache ??= FieldExtensions.AdjacentStayablesCircle(StartPosition.ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
            }
        }
        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions
        {
            get
            {
                return _allPositionsCache ??= FieldExtensions.AdjacentPositionsCircle(StartPosition.ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);
            }
        }

        public override bool CanMoveTo(TSquare target)
        {
            if (!StartPosition.IsWithinCircleRadius(target, _stayables.Count)) return false;
            CountOfDimensions countOfDimensions = StartPosition.ActiveAxes.ToCountOfDimensions();
            IEnumerable<TSquare> adjacentSquares = FieldExtensions.AdjacentStayablesCircle(countOfDimensions, StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            foreach (var pathSquare in path.Positions)
                if (!StartPosition.IsWithinCircleRadius(pathSquare, _stayables.Count)) return false;
            return true;
        }
    }
}