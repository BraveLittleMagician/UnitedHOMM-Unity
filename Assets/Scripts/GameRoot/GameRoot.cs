#nullable enable

using System;
using UnityEngine;

public sealed class GameRoot : IDisposable
{
    private readonly bool _autoStart;
    private bool _disposed;

    public IEventBus EventBus { get; }
    public ILogger Logger { get; }
    public IHouse House { get; }
    public IFlow Flow { get; }
    public IGameStateManager GameStateManager { get; }
    public IGameStarter StartGame { get; }
    public Presenter Presenter { get; }

    public GameRoot(GlobalDeselector globalDeselector, ButtonColors buttonColors, GlobalGeneratedSurroundingsHolder globalSurroundingsHolder, UIService uiService, ClickEventer clickEventer, ControllerOfCamera cameraController, GameView gameView, GridRenderer gridRenderer, GameObject layerPrefab, bool autoStart)
    {
        _autoStart = autoStart;

        // ================= Infrastructure =================
        EventBus = new EventBus();
        Logger = new LoggerForUnity();

        // ================= Config =================
        var configLoader = new GameConfigLoader();
        var boardConfig = configLoader.LoadBoardConfig();
        var seats = new Seats(boardConfig.NumberOfSides, boardConfig.NumberOfPlayersOnSide);
        var box = new AxisAlignedBox((MultipleAxes)boardConfig.Axes, boardConfig.FieldSize, boardConfig.WUp, boardConfig.WDown);

        // ================= Game State =================
        GameStateManager = new GameStateManager();

        // ================= Rooms =================
        var board = new Board(seats, box, EventBus, Logger);
        var decks = new Decks(seats, EventBus, Logger);
        var house = new House(seats, EventBus, Logger);
        house.AddRoom(board);
        house.AddRoom(decks);
        House = house;

        var registry = new Registry(house);
        var configUpdater = new BoardConfigUpdater(board, EventBus, Logger, boardConfig);

        // ================= Application =================
        var combatService = new CombatService(Logger, EventBus);
        var movementValidator = new MovementValidator(Logger);
        var abilityService = new AbilityService(EventBus, Logger);
        Flow = new Flow(house, EventBus, Logger, combatService, movementValidator, abilityService, registry);

        // ================= UI / Provider =================
        var buttonColorsProvider = new ButtonColorsProvider(buttonColors);
        uiService.Initialize(buttonColorsProvider);
        clickEventer.Initialize(globalDeselector);

        // ================= Grid =================
        var gridBuilder = new GridBuilder(new NoopObjectInjector(), layerPrefab);
        gridRenderer.Initialize(EventBus, globalSurroundingsHolder, GameStateManager, gridBuilder);

        // ================= Presenter =================
        Presenter = new (Flow, gameView, EventBus, cameraController, registry, configUpdater, boardConfig);

        // ================= Game Start Orchestration =================
        var roomInitializer = new RoomInitializer(house, EventBus, Logger, seats, boardConfig, board);
        var viewInitializer = new ViewInitializer(gameView, cameraController, boardConfig, board);
        var piecesSpawner = new PiecesSpawner(Flow);

        //================= Run =================
        StartGame = new StartGame(GameStateManager, roomInitializer, viewInitializer, piecesSpawner);
    }

    public void Start()
    {
        Presenter.Start();
        if (_autoStart) StartGame.Start();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Presenter.Dispose();
        (Flow as IDisposable)?.Dispose();
        (House as IDisposable)?.Dispose();
        (EventBus as IDisposable)?.Dispose();
    }
}