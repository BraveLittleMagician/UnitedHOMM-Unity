#nullable enable

public sealed class Board : RoomT<Square>, ISquarePositionRoom
{
    public Square Size { get; private set; }
    public int WWOffset { get; private set; }

    public Board(Seats seats, Square size, int wwOffset, IEventBus eventBus, ILogger logger)
        : base(seats, eventBus, logger)
    {
        Size = size;
        WWOffset = wwOffset;
        eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        Size = BoardSizeFactory.FromConfig(e.NewConfig);
        WWOffset = e.NewConfig.WWOffset;
        Logger.Log($"Размер доски обновлён: {Size}, WWOffset={WWOffset}");
    }

    public bool IsInside(Square pos) =>
        pos.X >= 0 && pos.X < Size.X &&
        pos.Y >= 0 && pos.Y < Size.Y &&
        pos.Z >= 0 && pos.Z < Size.Z &&
        pos.W >= WWOffset && pos.W < WWOffset + Size.W;

    protected override bool ValidateAdd(IPiece piece, Square position, out string error)
    {
        if (!IsInside(position))
        {
            error = $"Позиция {position} выходит за границы доски {Size} (WWOffset={WWOffset})";
            return false;
        }
        error = "";
        return true;
    }

    protected override bool ValidateDisplace(IPath<Square> path, IPiece piece, out string error)
    {
        var start = path.Positions[0];
        if (!IsInside(start))
        {
            error = $"Начальная позиция {start} выходит за границы доски {Size}";
            return false;
        }
        var end = path.Positions[^1];
        if (!IsInside(end))
        {
            error = $"Конечная позиция {end} выходит за границы доски {Size}";
            return false;
        }
        error = "";
        return true;
    }

    public override void Dispose()
    {
        EventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        base.Dispose();
    }
}