#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Board : RoomT<Square>, ISquarePositionRoom
{
    public AxisAlignedBox Box { get; private set; }

    public Board(Seats seats, AxisAlignedBox allowedArea, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger)
    {
        Box = allowedArea ?? throw new ArgumentNullException(nameof(allowedArea));
        eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        Box = AxisAlignedBox.FromConfig(e.NewConfig);
        Logger.Log($"Размер доски обновлён: {Box}");
    }

    protected override bool ValidateAdd(IPiece piece, Square position, out string error)
    {
        if (!Box.Contains(position))
        {
            error = $"Позиция {position} выходит за границы доски";
            return false;
        }
        error = "";
        return true;
    }
    protected override bool ValidateDisplace(IPath<Square> path, IPiece piece, out string error)
    {
        if (!Box.Contains(path.Positions[0]))
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
            if (!Box.Contains(pos))
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
        var activeAxes = axes.GetSeparatedAxes().ToList();
        int maxCoord = 0;
        foreach (var pos in _pieces.Keys)
        {
            if (activeAxes.Contains(Axis.X)) maxCoord = Math.Max(maxCoord, pos.X);
            if (activeAxes.Contains(Axis.Y)) maxCoord = Math.Max(maxCoord, pos.Y);
            if (activeAxes.Contains(Axis.Z)) maxCoord = Math.Max(maxCoord, pos.Z);
            if (activeAxes.Contains(Axis.W)) maxCoord = Math.Max(maxCoord, pos.W);
        }
        return maxCoord + 1;
    }
    public override void Dispose()
    {
        EventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        base.Dispose();
    }
}