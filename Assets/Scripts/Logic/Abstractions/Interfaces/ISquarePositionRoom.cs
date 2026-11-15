#nullable enable

public interface ISquarePositionRoom : IReadOnlyRoom 
{
    bool IsInside(Square position);
}