#nullable enable

public interface IMovementFactory
{
    IMovement CreateFor(IEventBus eventBus, IPiece owner);
}