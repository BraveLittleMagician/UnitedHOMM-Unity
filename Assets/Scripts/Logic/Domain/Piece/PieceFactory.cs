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

    /// <summary>
    /// Создаёт фигуру. Бросает <see cref="InvalidOperationException"/> при
    /// config-ошибке (например, фигура рождается обездвиженной).
    /// Для graceful-обработки используйте <see cref="TryCreate"/>.
    /// </summary>
    public IPiece Create(BigInteger index, PieceDefinition definition)
    {
        var result = TryCreate(index, definition);
        if (!result.IsSuccess)
            throw new System.InvalidOperationException(result.Error);
        return result.Value!;
    }

    /// <summary>
    /// Создаёт фигуру с валидацией config-инвариантов.
    /// При провале освобождает ресурсы (detach модификаторов, dispose фигуры)
    /// и возвращает <see cref="Result{IPiece}.Failure"/>. Индекс в <see cref="House"/>
    /// освобождается вызывающей стороной.
    /// </summary>
    public IResult<IPiece> TryCreate(BigInteger index, PieceDefinition definition)
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

        bool modifiersAttached = false;
        if (definition.Modifiers != null && definition.Modifiers.Count > 0)
        {
            _modifierHost.AttachModifiers(piece, definition.Modifiers);
            modifiersAttached = true;
        }

        if (!HasAtLeastOneMovableCell(piece, out var error))
        {
            if (modifiersAttached) _modifierHost.RemoveAll(piece.IndexInHouse);
            piece.Dispose();
            return Result<IPiece>.Failure(error);
        }

        return Result<IPiece>.Success(piece);
    }

    private static bool HasAtLeastOneMovableCell(IPiece piece, out string error)
    {
        var movements = piece.Movements;
        if (movements == null || movements.Count == 0)
        {
            error = $"Piece '{piece.Name}' has no movements — config error";
            return false;
        }

        for (int i = 0; i < movements.Count; i++)
        {
            if (movements[i].HasAtLeastOneStayableCell(piece, Square.Zero))
            {
                error = "";
                return true;
            }
        }

        error = $"Piece '{piece.Name}' has no move cells — config error";
        return false;
    }
}