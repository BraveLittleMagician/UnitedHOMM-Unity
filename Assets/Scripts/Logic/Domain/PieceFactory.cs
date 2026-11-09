#nullable enable
using System.Numerics;
using System.Threading;

public sealed class PieceFactory
{
    private static long _nextPieceId = 1;
    private static long NextPieceId() => Interlocked.Increment(ref _nextPieceId);

    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public PieceFactory(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public IPiece Create(BigInteger index, PieceDefinition definition)
    {
        var pieceId = NextPieceId();
        var piece = new Piece(
            index,
            pieceId,
            definition.Owner,
            definition.Name,
            definition.Health,
            _eventBus,
            _logger);

        if (definition.MeleeAttacks != null) foreach (var a in definition.MeleeAttacks) piece.AddMeleeAttack(a);
        if (definition.RangedAttacks != null) foreach (var a in definition.RangedAttacks) piece.AddRangedAttack(a);
        if (definition.Abilities != null) foreach (var a in definition.Abilities) piece.AddAbility(a);

        return piece;
    }
}