#nullable enable

using System.Linq;
using System.Numerics;

public sealed class Decks : RoomT<int>, IIntPositionRoom
{
    public Decks(Seats seats, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger) { }

    protected override void OnPieceAdded(IPiece piece, bool fromAnotherRoom)
    {
        base.OnPieceAdded(piece, fromAnotherRoom);
        EventBus.Publish(new DeckSizeChangedEvent(CountOfPieces, piece.Owner));
    }
    protected override void OnPieceRemoved(IPiece piece)
    {
        base.OnPieceRemoved(piece);
        EventBus.Publish(new DeckSizeChangedEvent(CountOfPieces, piece.Owner));
    }
    protected override bool ValidateAdd(IPiece piece, int position, out string error)
    {
        if (position < 0)
        {
            error = "Позиция не может быть отрицательной";
            return false;
        }
        error = "";
        return true;
    }
    protected override bool ValidateDisplace(IPath<int> path, IPiece piece, out string error)
    {
        var end = path.Positions[^1];
        if (end < 0 || end >= CountOfPieces)
        {
            error = $"Конечная позиция {end} вне диапазона колоды";
            return false;
        }
        error = "";
        return true;
    }
}