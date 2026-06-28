#nullable enable


using System;

public sealed class Board : RoomT<Square>, ISquarePositionRoom
{
    private readonly AxisAlignedBox _allowedArea;

    public Board(Seats seats, AxisAlignedBox allowedArea, IEventBus eventBus, ILogger logger)
        : base(seats, eventBus, logger)
    {
        _allowedArea = allowedArea ?? throw new ArgumentNullException(nameof(allowedArea));
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
}