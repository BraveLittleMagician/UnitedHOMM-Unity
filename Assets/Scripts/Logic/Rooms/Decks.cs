#nullable enable

using System.Linq;
using System.Numerics;

public sealed class Decks : RoomT<int>, IIntPositionRoom
{
    public Decks(Seats seats, IEventBus eventBus, ILogger logger) : base(seats, eventBus, logger) { }

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

    public override bool Add<TPos>(IPiece piece, TPos pos, bool fromAnotherRoom, out string error)
    {
        if (base.Add(piece, pos, fromAnotherRoom, out error))
        {
            EventBus.Publish(new DeckSizeChangedEvent(CountOfPieces, piece.Owner));
            return true;
        }
        return false;
    }
    public override bool Remove(BigInteger index)
    {
        var piece = Pieces.Values.FirstOrDefault(p => p.IndexInHouse == index);
        if (piece == null) return false;

        if (base.Remove(index))
        {
            EventBus.Publish(new DeckSizeChangedEvent(CountOfPieces, piece.Owner));
            return true;
        }
        return false;
    }
}