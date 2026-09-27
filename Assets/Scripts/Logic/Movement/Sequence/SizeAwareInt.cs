#nullable enable

using System;
using System.Collections.Generic;

public sealed class SizeAwareInt : ISequence<int, SizeAwareInt>, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IndexOfPlayer _owner;
    private int _count;
    private bool _disposed;

    public SizeAwareInt(IEventBus eventBus, IndexOfPlayer owner, int initialCount = 0)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _owner = owner;
        _count = initialCount;

        _eventBus.Subscribe<DeckSizeChangedEvent>(OnDeckSizeChanged);
    }

    public bool GuaranteesAtLeastOneStayable => true;

    public ISequenceEnumerator<int> GetEnumerator(int start) => new Enumerator(start, () => _count);

    public SizeAwareInt Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;

    private void OnDeckSizeChanged(DeckSizeChangedEvent e)
    {
        if (e.Owner == _owner)
            _count = e.NewSize;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _eventBus.Unsubscribe<DeckSizeChangedEvent>(OnDeckSizeChanged);
    }

    public bool Equals(SizeAwareInt? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _owner == other._owner && _eventBus == other._eventBus;
    }

    public override bool Equals(object? obj) => obj is SizeAwareInt other && Equals(other);
    public override int GetHashCode() => _owner.GetHashCode();

    public static bool operator ==(SizeAwareInt? left, SizeAwareInt? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(SizeAwareInt? left, SizeAwareInt? right) => !(left == right);

    private sealed class Enumerator : SequenceEnumerable<int>
    {
        private readonly Func<int> _countProvider;
        private List<int>? _cachedPositions;
        private int _cachedCount = -1;

        public Enumerator(int start, Func<int> countProvider) : base(start)
        {
            _countProvider = countProvider;
        }

        private List<int> GetPositions()
        {
            int count = _countProvider();
            if (_cachedPositions != null && _cachedCount == count)
                return _cachedPositions;

            var result = new List<int>(count > 1 ? count - 1 : 0);
            for (int i = 0; i < count; i++)
            {
                if (i != StartPosition)
                    result.Add(i);
            }

            _cachedPositions = result;
            _cachedCount = count;
            return result;
        }

        protected override List<int> CreateOnlyStayablesPositions => GetPositions();

        protected override Dictionary<int, Stayable> CreateAllPossiblePositions
        {
            get
            {
                var positions = GetPositions();
                var dict = new Dictionary<int, Stayable>(positions.Count);
                foreach (var p in positions) dict[p] = Stayable.Stay;
                return dict;
            }
        }

        public override bool CanMoveTo(int target)
        {
            int count = _countProvider();
            return target >= 0 && target < count && target != StartPosition;
        }

        protected override bool ProtectedCanMoveTo(IPath<int> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;

            for (int i = 1; i < positions.Count; i++)
            {
                if (!CanMoveTo(positions[i])) return false;
                if (Math.Abs(positions[i] - positions[i - 1]) != 1) return false;
            }
            return true;
        }
    }
}