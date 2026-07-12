#nullable enable

using System;

public sealed class Board : RoomT<Square>, ISquarePositionRoom
{
    private AxisAlignedBox _allowedArea;
    private bool _isUpdating;

    public Board(Seats seats, AxisAlignedBox allowedArea, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger)
    { 
        _allowedArea = allowedArea ?? throw new ArgumentNullException(nameof(allowedArea)); 
        eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        var newAxes = (MultipleAxes)e.NewConfig.Axes;
        var newSize = e.NewConfig.FieldSize;
        _allowedArea = new AxisAlignedBox(newAxes, newSize, 0);
        Logger.Log($"Размер доски обновлён: {newSize} по осям {newAxes}");
    }
    protected override bool ValidateAdd(IPiece piece, Square position, out string error)
    {
        if (!_allowedArea.Contains(position))
        {
            error = $"Позиция {position} выходит за границы доски";
            return false;
        }
        error = "";
        return true;
    }
    protected override bool ValidateDisplace(IPath<Square> path, IPiece piece, out string error)
    {
        if (!_allowedArea.Contains(path.Positions[0]))
        {
            error = $"Конечная позиция {path.Positions[0]} выходит за границы доски";
            return false;
        }

        for (int i = 1; i < path.Positions.Count; i++)
        {
            var pos = path.Positions[i];
            if (_pieces.ContainsKey(pos))
            {
                error = $"Позиция {pos} занята";
                return false;
            }
            if (!_allowedArea.Contains(pos))
            {
                error = $"Позиция {pos} выходит за границы доски";
                return false;
            }
        }

        error = "";
        return true;
    }
    public int GetMinimalFieldSize(MultipleAxes axes)
    {
        if (_pieces.Count == 0) return 0;

        int maxCoord = 0;
        foreach (var pos in _pieces.Keys)
        {
            if (axes.HasFlag(Axis.X)) maxCoord = Math.Max(maxCoord, pos.X);
            if (axes.HasFlag(Axis.Y)) maxCoord = Math.Max(maxCoord, pos.Y);
            if (axes.HasFlag(Axis.Z)) maxCoord = Math.Max(maxCoord, pos.Z);
            if (axes.HasFlag(Axis.W)) maxCoord = Math.Max(maxCoord, pos.W);
        }
        return maxCoord + 1;
    }
    public override void Dispose()
    {
        EventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        base.Dispose();
    }
}