#nullable enable

using System.Numerics;

public sealed class PieceFactory
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public PieceFactory(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public IPiece Create(BigInteger index, PieceDefinition definition)
    {
        var piece = new Piece(index, definition.Owner, definition.Name, definition.Health, _eventBus, _logger);

        if (definition.MovementFactories != null)
            foreach (var factory in definition.MovementFactories)
                piece.AddMovement(factory.CreateFor(_eventBus, piece));

        if (definition.MeleeAttacks != null)
            foreach (var a in definition.MeleeAttacks) piece.AddMeleeAttack(a);

        if (definition.RangedAttacks != null)
            foreach (var a in definition.RangedAttacks) piece.AddRangedAttack(a);

        if (definition.Abilities != null)
            foreach (var a in definition.Abilities) piece.AddAbility(a);

        return piece;
    }
}
