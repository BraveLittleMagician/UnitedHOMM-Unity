#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct RelativeArea<TSquare> : ISequence<TSquare, RelativeArea<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    private readonly int _cachedHash;

    public RelativeArea(MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantee)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length не может быть отрицательным");
        
        ActiveAxes = activeAxes;
        
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
        hash.Add((int)activeAxes);
        foreach (var item in stayables) hash.Add(item);
        _cachedHash = hash.ToHashCode();
    }
    public RelativeArea(MultipleAxes activeAxes, bool isCircle) : this(activeAxes, ImmutableHashSet.Create(0, 1), 1, isCircle, guarantee: true) { }

    public bool GuaranteesAtLeastOneStayable { get; }
    public bool IsCircle { get; }
    public int Length { get; }
    public MultipleAxes ActiveAxes { get; }
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
        return new RelativeArea<TSquare>(ActiveAxes, Stayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithDecreasedRadius()
    {
        if (Length <= 1) return this;
        if (Stayables.Count == 1 && Stayables.Contains(Length)) return this;
        int newLength = Length - 1;
        var newStayables = Stayables.Where(r => r <= newLength).ToImmutableHashSet();
        return new (ActiveAxes, newStayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithAddedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (Stayables.Contains(radius)) return this;
        return new (ActiveAxes, Stayables.Add(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithRemovedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (!Stayables.Contains(radius)) return this; 
        if (Stayables.Count == 1 && Stayables.Contains(radius)) return this;
        return new RelativeArea<TSquare>(ActiveAxes, Stayables.Remove(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TSquare> WithCircle(bool isCircle) => new (ActiveAxes, Stayables, Length, isCircle, GuaranteesAtLeastOneStayable);

    public (MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithIncreasedRadius() => (ActiveAxes, Stayables, Length + 1, IsCircle, GuaranteesAtLeastOneStayable);
    
    public (MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithDecreasedRadius()
    {
        var data = (ActiveAxes, Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (Length < 1) return data;
        int newLength = Length - 1;
        var newStayables = Stayables.Where(r => r <= newLength).ToImmutableHashSet();
        return (ActiveAxes, newStayables, newLength, IsCircle, false);
    }
    public (MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithAddedStayable(int radius)
    {
        var data = (ActiveAxes, Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (radius < 0 || radius > Length) return data;
        if (Stayables.Contains(radius)) return data;
        return (ActiveAxes, Stayables.Add(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public (MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithRemovedStayable(int radius)
    {
        var data = (ActiveAxes, Stayables, Length, IsCircle, GuaranteesAtLeastOneStayable);
        if (radius < 0 || radius > Length) return data;
        if (!Stayables.Contains(radius)) return data;
        return (ActiveAxes, Stayables.Remove(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public (MultipleAxes activeAxes, IImmutableSet<int> stayables, int length, bool isCircle, bool guarantees) DataWithCircle(bool isCircle) => (ActiveAxes, Stayables, Length, isCircle, GuaranteesAtLeastOneStayable);

    public readonly ISequenceEnumerator<TSquare> GetEnumerator(TSquare start) => 
        IsCircle ? 
            new EnumerableCircle(ActiveAxes, BuildStayableList(), start): 
            new EnumerableSquare(ActiveAxes, BuildStayableList(), start);
    public RelativeArea<TSquare> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public bool Equals(RelativeArea<TSquare> other)
    {
        if (ReferenceEquals(Stayables, other.Stayables)) return true;
        if (GuaranteesAtLeastOneStayable != other.GuaranteesAtLeastOneStayable || IsCircle != other.IsCircle || Length != other.Length || ActiveAxes != other.ActiveAxes) return false;
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

        public EnumerableSquare(MultipleAxes activeAxes, List<Stayable> stayables, TSquare startSquare) : base(startSquare)
        {
            ActiveAxes = activeAxes;
            _stayables = stayables;
        }

        public MultipleAxes ActiveAxes { get; }

        protected override List<TSquare> CreateOnlyStayablesPositions => _allStayablesCache ??= FieldExtensions.AdjacentStayables(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions => _allPositionsCache ??= FieldExtensions.AdjacentPositions(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);
        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            foreach (var pathSquare in path.Positions)
                if (!StartPosition.IsWithinSquareRadius(pathSquare, _stayables.Count, ActiveAxes)) return false;
            return true;
        }

        public override bool CanMoveTo(TSquare target)
        {
            if (!StartPosition.IsWithinSquareRadius(target, _stayables.Count, ActiveAxes)) return false;
            IEnumerable<TSquare> adjacentSquares = FieldExtensions.AdjacentStayables(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
    }
    private class EnumerableCircle : SequenceEnumerable<TSquare>
    {
        private readonly List<Stayable> _stayables;
        private List<TSquare>? _allStayablesCache = null;
        private Dictionary<TSquare, Stayable>? _allPositionsCache = null;

        public EnumerableCircle(MultipleAxes activeAxes, List<Stayable> stayables, TSquare startSquare) : base(startSquare)
        {
            ActiveAxes = activeAxes;
            _stayables = stayables;
        }

        public MultipleAxes ActiveAxes { get; }

        protected override List<TSquare> CreateOnlyStayablesPositions => _allStayablesCache ??= FieldExtensions.AdjacentStayablesCircle(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions => _allPositionsCache ??= FieldExtensions.AdjacentPositionsCircle(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);

        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            foreach (var pathSquare in path.Positions)
                if (!StartPosition.IsWithinCircleRadius(pathSquare, _stayables.Count, ActiveAxes)) return false;
            return true;
        }

        public override bool CanMoveTo(TSquare target)
        {
            if (!StartPosition.IsWithinCircleRadius(target, _stayables.Count, ActiveAxes)) return false;
            IEnumerable<TSquare> adjacentSquares = FieldExtensions.AdjacentStayablesCircle(ActiveAxes.ToCountOfDimensions(), StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
    }
}