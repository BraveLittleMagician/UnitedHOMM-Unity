#nullable enable

public sealed class Board : RoomT<Square>, ISquarePositionRoom
{
    public Square Size { get; private set; }

    public Board(Seats seats, Square size, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger)
    {
        Size = size;
        eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        Size = BoardSizeFactory.FromConfig(e.NewConfig);
        Logger.Log($"Размер доски обновлён: {Size}");
    }

    protected override bool ValidateAdd(IPiece piece, Square position, out string error)
    {
        if (!Size.Contains(position))
        {
            error = $"Позиция {position} выходит за границы доски";
            return false;
        }
        error = "";
        return true;
    }
    protected override bool ValidateDisplace(IPath<Square> path, IPiece piece, out string error)
    {
        if (!Size.Contains(path.Positions[0]))
        {
            error = $"Начальная позиция {path.Positions[0]} выходит за границы доски";
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
            if (!Size.Contains(pos))
            {
                error = $"Позиция {pos} выходит за границы доски";
                return false;
            }
        }

        error = "";
        return true;
    }
    public int GetMinimalFieldSize()
    {
        if (_pieces.Count == 0) return 2;

        int maxCoord = 0;
        foreach (var pos in _pieces.Keys)
        {
            if (pos.X > maxCoord) maxCoord = pos.X;
            if (pos.Y > maxCoord) maxCoord = pos.Y;
            if (pos.Z > maxCoord) maxCoord = pos.Z;
        }
        return maxCoord + 1;
    }
    public override void Dispose()
    {
        EventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        base.Dispose();
    }
}