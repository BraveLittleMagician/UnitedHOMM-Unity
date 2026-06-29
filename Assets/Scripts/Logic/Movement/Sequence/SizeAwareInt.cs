#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public readonly  struct SizeAwareInt : ISequence<int, SizeAwareInt>
{
    private readonly IEventBus _eventBus;
    private readonly IndexOfPlayer _owner;
    private readonly int _cachedHash;

    public SizeAwareInt(IEventBus eventBus, IndexOfPlayer owner)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _owner = owner;
        GuaranteesAtLeastOneStayable = true;
        var hash = new HashCode();
        hash.Add(GuaranteesAtLeastOneStayable);
        hash.Add(_owner);
        hash.Add(_eventBus);
        _cachedHash = hash.ToHashCode();
    }

    public bool GuaranteesAtLeastOneStayable { get; }

    public ISequenceEnumerator<int> GetEnumerator(int start) => new Enumerable(start, _eventBus, _owner);
    public SizeAwareInt Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public bool Equals(SizeAwareInt other)
    {
        return ReferenceEquals(_eventBus, other._eventBus) && _owner == other._owner && GuaranteesAtLeastOneStayable;
    }
    public override bool Equals(object? obj) => obj is Square other && Equals(other);
    public override int GetHashCode() => _cachedHash;

    public static bool operator ==(SizeAwareInt left, SizeAwareInt right) => left.Equals(right);
    public static bool operator !=(SizeAwareInt left, SizeAwareInt right) => !(left == right);

    private class Enumerable : SequenceEnumerable<int>
    {
        private readonly IEventBus _eventBus;
        private readonly IndexOfPlayer _owner;
        private bool _disposed = false;
        private int _count = 0;
        private (int, List<int>?) _allStayablesCache = (0, null);

        public Enumerable(int start, IEventBus eventBus, IndexOfPlayer owner) : base(start)
        {
            _eventBus = eventBus;
            _owner = owner;
            _eventBus.Subscribe<DeckSizeChangedEvent>(OnDeckSizeChanged);
        }

        private (int, List<int>) Create
        {
            get
            {
                if (_allStayablesCache.Item2 != null && _allStayablesCache.Item1 == _count)
                    return (_allStayablesCache.Item1, _allStayablesCache.Item2);
                else
                {
                    _allStayablesCache = createPositions();
                    if (_allStayablesCache.Item2 != null && _allStayablesCache.Item1 == _count)
                        return (_allStayablesCache.Item1, _allStayablesCache.Item2);
                    throw new Exception(nameof(_allStayablesCache));
                }

                (int, List<int>) createPositions()
                {
                    List<int> result = new();
                    for (int i = 0; i < _count; i++)
                    {
                        if (i != StartPosition)
                            result.Add(i);
                    }
                    return (_count, result);
                }
            }
        }
        protected override List<int> CreateOnlyStayablesPositions => Create.Item2;
        protected override Dictionary<int, Stayable> CreateAllPossiblePositions => Create.Item2.ToDictionary(e => e, v => Stayable.Stay);

        private void OnDeckSizeChanged(DeckSizeChangedEvent e)
        {
            if (e.Owner == _owner)
                _count = e.NewSize;
        }
        public override bool CanMoveTo(int target)
        {
            return target >= 0 && target < _count && target != StartPosition;
        }
        protected override bool ProtectedCanMoveTo(IPath<int> path)
        {
            var positions = path.Positions;
            if (positions.Count == 0) return false;

            for (int i = 1; i < positions.Count; i++)
            {
                if (!CanMoveTo(positions[i])) return false;
                if (Math.Abs(positions[i] - positions[i - 1]) != 1) return false;
            }
            return true;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _eventBus.Unsubscribe<DeckSizeChangedEvent>(OnDeckSizeChanged);
        }
    }
}