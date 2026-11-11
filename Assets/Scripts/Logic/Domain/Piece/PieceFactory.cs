#nullable enable
using System.Numerics;

public sealed class PieceFactory
{
    private readonly IEventBus _eventBus;
    private readonly ModifierHost _modifierHost;
    private readonly ILogger _logger;

    public PieceFactory(IEventBus eventBus, ModifierHost modifierHost, ILogger logger)
    {
        _eventBus = eventBus;
        _modifierHost = modifierHost ?? throw new System.ArgumentNullException(nameof(modifierHost));
        _logger = logger;
    }

    public IPiece Create(BigInteger index, PieceDefinition definition)
    {
        var effect = definition.AttackEffect ?? new DamageAttackEffect();

        var piece = new Piece(
            index,
            definition.Owner,
            definition.Name,
            definition.Health,
            definition.AttackDamage,
            definition.AttackCharges,
            definition.InitialFacing,
            effect,
            _eventBus,
            _logger);

        if (definition.MovementFactories != null)
            foreach (var factory in definition.MovementFactories)
                piece.AddMovement(factory.CreateFor(_eventBus, _modifierHost, piece));

        if (definition.Abilities != null)
            foreach (var a in definition.Abilities) piece.AddAbility(a);

        if (definition.Modifiers != null && definition.Modifiers.Count > 0)
            _modifierHost.AttachModifiers(piece, definition.Modifiers);

        return piece;
    }
}