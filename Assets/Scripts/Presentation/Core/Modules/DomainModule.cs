#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public static class DomainModule
{
    public static DomainContext Initialize(
        IEventBus eventBus,
        ILogger logger,
        TextAsset boardLayoutJson,
        TextAsset pieceLibraryJson)
    {
        if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (boardLayoutJson == null) throw new ArgumentNullException(nameof(boardLayoutJson));
        if (pieceLibraryJson == null) throw new ArgumentNullException(nameof(pieceLibraryJson));

        // --- Data ---
        var piecePrefabs = PieceLibraryLoader.Load(pieceLibraryJson.text);
        var deckSequences = new DeckSequenceProvider(eventBus);
        var templates = StandardPieceTemplates.CreateAll(deckSequences, eventBus);
        var pieceTemplateRegistry = new PieceTemplateRegistry(templates);

        var layoutResult = BoardLayoutLoader.Load(boardLayoutJson.text, pieceTemplateRegistry);
        var configOfBoard = layoutResult.Config;
        var placements = layoutResult.Placements;
        logger.Log($"Data loaded: {placements.Count} pieces, field {configOfBoard.FieldSize}, axes {configOfBoard.Axes}");

        // --- Core services ---
        var seats = new Seats(configOfBoard.NumberOfSides, configOfBoard.NumberOfPlayersOnSide);
        var modifierHost = new ModifierHost(eventBus, logger);
        var pieceFactory = new PieceFactory(eventBus, modifierHost, logger);
        var house = new House(seats, eventBus, logger, pieceFactory);
        var registry = new Registry(house);

        var boardSize = BoardSizeFactory.FromConfig(configOfBoard);
        var board = new Board(seats, boardSize, eventBus, logger);
        house.AddRoom(board);
        house.AddRoom(new Decks(seats, eventBus, logger));

        var players = new HashSet<IdOfPlayer>();
        foreach (var p in placements) players.Add(p.Definition.Owner);
        foreach (var player in players)
        {
            var gy = new Graveyard(player, seats, eventBus, logger);
            house.AddRoom($"Graveyard_{player.IndexOfSide}_{player.IndexOfPlayerOnSide}", gy);
        }

        // --- Validators & combat ---
        var movementValidator = new MovementValidator(logger);
        var abilityService = new AbilityService(eventBus, logger);
        var attackTargetProvider = new AttackTargetProvider(modifierHost);
        var combatService = new CombatService(logger, eventBus, attackTargetProvider, modifierHost);

        modifierHost.Registry = registry;

        // --- Turn & match ---
        var turnManager = new TurnManager(placements, eventBus);
        var matchState = new MatchState();
        var matchEndChecker = new MatchEndChecker(registry, turnManager, matchState);

        // --- Reactive actions ---
        var enPassantService = new EnPassantService(registry, turnManager, eventBus, modifierHost);
        var ambushService = new AmbushService(registry, turnManager, eventBus, modifierHost);
        var reactiveRegistry = new ReactiveActionRegistry();
        reactiveRegistry.Register(enPassantService);
        reactiveRegistry.Register(ambushService);

        // --- Promotion ---
        var promotionService = new PromotionService(
            registry, house, pieceTemplateRegistry, pieceFactory, modifierHost, eventBus, logger);

        // --- Flow ---
        var flow = new Flow(
            house,
            registry,
            eventBus,
            logger,
            movementValidator,
            abilityService,
            combatService,
            modifierHost,
            enPassantService,
            ambushService,
            onCommitted: () =>
            {
                bool gameOver = matchEndChecker.CheckAfterCommit();
                if (!gameOver) turnManager.EndTurn();
            });

        // --- Handlers ---
        var pendingModalManager = new PendingModalManager();

        var attackProvider = new AttackCellActionProvider(flow, attackTargetProvider, registry, modifierHost);
        var enPassantProvider = new EnPassantActionProvider(flow);
        var ambushProvider = new AmbushActionProvider(flow);
        var cellActionProviders = new ICellActionProvider[] { attackProvider, enPassantProvider, ambushProvider };

        var cellActionHandler = new CellActionHandler(new CellActionHandlerContext
        {
            Flow = flow,
            PendingModalManager = pendingModalManager,
            MatchState = matchState,
            CellActionProviders = cellActionProviders,
            Logger = logger,
        });

        var modifierActivationHandler = new ModifierActivationHandler(new ModifierActivationHandlerContext
        {
            Flow = flow,
            PendingModalManager = pendingModalManager,
            MatchState = matchState,
            ModifierHost = modifierHost,
            Registry = registry,
            EventBus = eventBus,
            Logger = logger,
        });

        var promotionChoiceHandler = new PromotionChoiceHandler(new PromotionChoiceHandlerContext
        {
            PendingModalManager = pendingModalManager,
            MatchState = matchState,
            PromotionService = promotionService,
            Logger = logger,
        });

        return new DomainContext(
            Seats: seats,
            Board: board,
            House: house,
            Registry: registry,
            Flow: flow,
            TurnManager: turnManager,
            MatchState: matchState,
            ModifierHost: modifierHost,
            ReactiveRegistry: reactiveRegistry,
            AbilityService: abilityService,
            DeckSequences: deckSequences,
            PendingModalManager: pendingModalManager,
            CellActionHandler: cellActionHandler,
            ModifierActivationHandler: modifierActivationHandler,
            PromotionChoiceHandler: promotionChoiceHandler,
            CellActionProviders: cellActionProviders,
            Placements: placements,
            PiecePrefabs: piecePrefabs);
    }
}