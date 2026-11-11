#nullable enable

using System;
using System.Collections.Generic;

public sealed class Movement<TSequence, TPosition> : IMovement where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct, ISquare<TPosition>
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;
    private readonly TSequence _baseSequence;
    private TSequence _modifiedSequence;
    private bool _disposed;

    public Movement(TSequence sequence, IEventBus eventBus, IPiece owner)
    {
        _baseSequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        Modifiers = new ModifiersOfMovement<TSequence, TPosition>(_eventBus, _owner);
        _eventBus.Subscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        _modifiedSequence = ApplyModifiers();
    }

    public ModifiersOfMovement<TSequence, TPosition> Modifiers { get; }

    private void OnModifiersUpdated(ModifiersUpdatedEvent e)
    {
        if (e.Piece == _owner && e.SequenceType == typeof(TSequence))
            _modifiedSequence = ApplyModifiers();
    }

    private TSequence ApplyModifiers()
    {
        var result = _baseSequence;
        foreach (var m in Modifiers.All)
            result = m.Apply(result);
        return result;
    }

    public bool CanMove<TPos>(IPath<TPos> path) where TPos : struct
    {
        if (path is not IPath<TPosition> typedPath) return false;

        var enumerator = _modifiedSequence.GetEnumerator(typedPath.Positions[0]);
        return enumerator.CanMoveTo(typedPath);
    }

    public bool IsApplicableToRoom(IReadOnlyRoom room)
    {
        if (typeof(TPosition) == typeof(int))
            return room is IIntPositionRoom;

        if (typeof(ISquarePosition).IsAssignableFrom(typeof(TPosition)))
            return room is ISquarePositionRoom;

        return false;
    }

    public List<TPos> GetStayables<TPos>(TPos position) where TPos : struct
    {
        if (position is TPosition typed)
        {
            var result = _modifiedSequence.GetEnumerator(typed).PositionsStayables;
            if (result is List<TPos> typedResult) return typedResult;
        }
        return new List<TPos>();
    }

    public Dictionary<TPos, Stayable> GetPositions<TPos>(TPos position) where TPos : struct
    {
        if (position is TPosition typed)
        {
            var result = _modifiedSequence.GetEnumerator(typed).PositionsAll;
            if (result is Dictionary<TPos, Stayable> typedResult) return typedResult;
        }
        return new Dictionary<TPos, Stayable>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _eventBus.Unsubscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        Modifiers.Dispose();
    }
}