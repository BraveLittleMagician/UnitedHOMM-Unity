#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct RelativeArea<TAxes> : ISequence<Square, RelativeArea<TAxes>> where TAxes : struct, IAxes
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

    public RelativeArea<TAxes> WithIncreasedRadius()
    {
        int newLength = Length + 1;
        return new RelativeArea<TAxes>(Stayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TAxes> WithDecreasedRadius()
    {
        if (Length <= 1) return this;
        int oldLength = Length;
        if (Stayables.Contains(oldLength) && Stayables.Count == 1) return this;
        int newLength = Length - 1;
        var newStayables = Stayables.Where(r => r <= newLength).ToImmutableHashSet();
        return new (newStayables, newLength, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TAxes> WithAddedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (Stayables.Contains(radius)) return this;
        return new (Stayables.Add(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TAxes> WithRemovedStayable(int radius)
    {
        if (radius < 0 || radius > Length) return this;
        if (!Stayables.Contains(radius)) return this; 
        if (Stayables.Count == 1 && Stayables.Contains(radius)) return this;
        return new RelativeArea<TAxes>(Stayables.Remove(radius), Length, IsCircle, GuaranteesAtLeastOneStayable);
    }
    public RelativeArea<TAxes> WithCircle(bool isCircle) => new (Stayables, Length, isCircle, GuaranteesAtLeastOneStayable);

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

    public readonly ISequenceEnumerator<Square> GetEnumerator(Square start) => 
        IsCircle ? 
            new EnumerableCircle(BuildStayableList(), start, default): 
            new EnumerableSquare(BuildStayableList(), start, default);
    public RelativeArea<TAxes> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public bool Equals(RelativeArea<TAxes> other)
    {
        if (ReferenceEquals(Stayables, other.Stayables)) return true;
        if (GuaranteesAtLeastOneStayable != other.GuaranteesAtLeastOneStayable || IsCircle != other.IsCircle || Length != other.Length) return false;
        if (Stayables == null && other.Stayables == null) return true;
        if (Stayables == null || other.Stayables == null) return false;
        return Stayables.SetEquals(other.Stayables);
    }
    public override bool Equals(object? obj) => obj is RelativeArea<TAxes> other && Equals(other);
    public override int GetHashCode() => _cachedHash;

    public static bool operator ==(RelativeArea<TAxes> left, RelativeArea<TAxes> right) => left.Equals(right);
    public static bool operator !=(RelativeArea<TAxes> left, RelativeArea<TAxes> right) => !(left == right);


    private class EnumerableSquare : SequenceEnumerableSquareWithAxes<TAxes>
    {
        private readonly List<Stayable> _stayables;
        private List<Square>? _allStayablesCache = null;
        private Dictionary<Square, Stayable>? _allPositionsCache = null;

        public EnumerableSquare(List<Stayable> stayables, Square startSquare, TAxes axes) : base(startSquare, axes) => _stayables = stayables;

        private static bool IsWithinRadius(Square start, Square target, int radius)
        {
            return Math.Abs(target.X - start.X) <= radius &&
                   Math.Abs(target.Y - start.Y) <= radius &&
                   Math.Abs(target.Z - start.Z) <= radius &&
                   Math.Abs(target.W - start.W) <= radius;
        }
        protected override List<Square> CreateOnlyStayablesPositions
        {
            get
            {
                return _allStayablesCache ??= FieldExtensions.AdjacentStayables(Axes.Active.ToCountOfDimensions(), StartPosition, _stayables);
            }
        }
        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                return _allPositionsCache ??= FieldExtensions.AdjacentPositions(Axes.Active.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);
            }
        }

        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            Square start = StartPosition;
            foreach (var pathSquare in path.Positions)
                if (!IsWithinRadius(start, pathSquare, _stayables.Count)) return false;
            return true;
        }
        public override bool CanMoveTo(Square target)
        {
            if (!IsWithinRadius(StartPosition, target, _stayables.Count)) return false;
            IEnumerable<Square> adjacentSquares = FieldExtensions.AdjacentStayables(Axes.Active.ToCountOfDimensions(), StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
    }
    private class EnumerableCircle : SequenceEnumerableSquareWithAxes<TAxes>
    {
        private readonly List<Stayable> _stayables;
        private List<Square>? _allStayablesCache = null;
        private Dictionary<Square, Stayable>? _allPositionsCache = null;

        public EnumerableCircle(List<Stayable> stayables, Square startSquare, TAxes axes) : base(startSquare, axes) => _stayables = stayables;

        private bool IsWithinRadius(Square start, Square target, int maxRadius)
        {
            long dx = target.X - start.X;
            long dy = target.Y - start.Y;
            long dz = target.Z - start.Z;
            long dw = target.W - start.W;
            long distSq = dx * dx + dy * dy;
            if (Axes is Axes3D or Axes4D)
                distSq += dz * dz;
            if (Axes is Axes4D)
                distSq += dw * dw;
            return distSq <= (long)maxRadius * maxRadius;
        }
        protected override List<Square> CreateOnlyStayablesPositions
        {
            get
            {
                return _allStayablesCache ??= FieldExtensions.AdjacentStayablesCircle(Axes.Active.ToCountOfDimensions(), StartPosition, _stayables);
            }
        }
        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                return _allPositionsCache ??= FieldExtensions.AdjacentPositionsCircle(Axes.Active.ToCountOfDimensions(), StartPosition, _stayables.Count, _stayables);
            }
        }

        public override bool CanMoveTo(Square target)
        {
            if (!IsWithinRadius(target, StartPosition, _stayables.Count)) return false;
            CountOfDimensions countOfDimensions = Axes.Active.ToCountOfDimensions();
            IEnumerable<Square> adjacentSquares = FieldExtensions.AdjacentStayables(countOfDimensions, StartPosition, _stayables);
            if (!adjacentSquares.Contains(target)) return false;
            return true;
        }
        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
            if (path.Positions.Count - 1 > _stayables.Count) return false;
            Square start = StartPosition;
            foreach (var pathSquare in path.Positions)
                if (!IsWithinRadius(pathSquare, start, _stayables.Count)) return false;
            return true;
        }
    }
}