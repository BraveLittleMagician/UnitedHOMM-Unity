#nullable enable

public interface IMovementFactory
{
    IMovement CreateFor(IEventBus eventBus, ModifierHost modifierHost, IPiece owner);
}