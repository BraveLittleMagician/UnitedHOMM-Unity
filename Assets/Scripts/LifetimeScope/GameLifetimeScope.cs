#nullable enable

using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private GlobalDeselector _globalDeselector = null!;
    [SerializeField] private ButtonColors _buttonColors = null!;
    [SerializeField] private GlobalGeneratedSurroundingsHolder _globalSurroundingsHolder = null!;
    [SerializeField] private UIService _uiService = null!;
    [SerializeField] private ClickEventer _clickEventer = null!;
    [SerializeField] private ControllerOfCamera _cameraController = null!;
    [SerializeField] private GameView _gameView = null!;
    [SerializeField] private GridRenderer _gridRenderer = null!;
    [SerializeField] private GameObject _layerPrefab = null!;
    [SerializeField] private bool _autoStart = true;

    private void Start()
    {
        if (_globalDeselector == null) throw new ArgumentNullException(nameof(_globalDeselector));
        if (_buttonColors == null) throw new ArgumentNullException(nameof(_buttonColors));
        if (_globalSurroundingsHolder == null) throw new ArgumentNullException(nameof(_globalSurroundingsHolder));
        if (_uiService == null) throw new ArgumentNullException(nameof(_uiService));
        if (_clickEventer == null) throw new ArgumentNullException(nameof(_clickEventer));
        if (_cameraController == null) throw new ArgumentNullException(nameof(_cameraController));
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
        if (_gameView == null) throw new ArgumentNullException(nameof(_gameView));
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
    }

    protected override void Configure(IContainerBuilder builder)
    {
        var configLoader = new GameConfigLoader();
        var boardConfig = configLoader.LoadBoardConfig();
        var seats = new Seats(boardConfig.NumberOfSides, boardConfig.NumberOfPlayersOnSide);
        var box = new AxisAlignedBox((MultipleAxes)boardConfig.Axes, boardConfig.FieldSize, boardConfig.WUp, boardConfig.WDown);
        builder.RegisterInstance(boardConfig);
        builder.RegisterInstance(seats);
        builder.RegisterInstance(configLoader).As<IGameConfigLoader>();
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<LoggerForUnity>(Lifetime.Singleton).As<ILogger>();
        builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>();
        builder.Register<House>(Lifetime.Singleton).As<IHouse>();
        builder.Register<Registry>(Lifetime.Singleton).As<IRegistry>();
        builder.Register(resolver =>
        {
            var eventBus = resolver.Resolve<IEventBus>();
            var logger = resolver.Resolve<ILogger>();
            return new Board(seats, box, eventBus, logger);
        }, Lifetime.Singleton).AsSelf();
        builder.Register(resolver =>
        {
            var board = resolver.Resolve<Board>();
            var eventBus = resolver.Resolve<IEventBus>();
            var logger = resolver.Resolve<ILogger>();
            return new BoardConfigUpdater(board, eventBus, logger, boardConfig);
        }, Lifetime.Singleton).AsSelf();
        builder.Register<CombatService>(Lifetime.Singleton).As<ICombatService>();
        builder.Register<MovementValidator>(Lifetime.Singleton).As<IMovementValidator>();
        builder.Register<AbilityService>(Lifetime.Singleton).As<IAbilityService>();
        builder.Register<Flow>(Lifetime.Singleton).As<IFlow>();
        builder.Register<GamePresenter>(Lifetime.Singleton).As<IStartable>().AsSelf();
        builder.RegisterComponent(_gameView).As<IGameView>();
        builder.RegisterComponent(_gridRenderer).AsSelf();
        builder.RegisterComponent(_cameraController).As<IControllerOfCamera>();
        builder.RegisterComponent(_buttonColors).As<IButtonColors>();
        builder.RegisterComponent(_uiService).AsSelf();
        builder.Register<ButtonColorsProvider>(Lifetime.Singleton).As<IButtonColorsProvider>().WithParameter("initialColors", _buttonColors);
        builder.RegisterComponent(_globalDeselector).As<IGlobalDeselector>();
        builder.RegisterComponent(_globalSurroundingsHolder).As<IGlobalGeneratedSurroundingsHolder>();
        builder.RegisterComponent(_clickEventer).AsSelf();
        builder.Register<GridBuilder>(Lifetime.Singleton).As<IGridBuilder>().WithParameter("layerPrefab", _layerPrefab);
        builder.Register<RoomInitializer>(Lifetime.Singleton).As<IRoomInitializer>();
        builder.Register<ViewInitializer>(Lifetime.Singleton).As<IViewInitializer>();
        builder.Register<PiecesSpawner>(Lifetime.Singleton).As<IPiecesSpawner>();
        builder.Register<GameStarter>(Lifetime.Singleton).As<IGameStarter>();
        if (_autoStart) builder.RegisterEntryPoint<AutoGameStarter>();
    }

    private class AutoGameStarter : IStartable
    {
        private readonly IGameStarter _gameStarter;

        public AutoGameStarter(IGameStarter gameStarter) => _gameStarter = gameStarter;
        public void Start() => _gameStarter.StartGame();
    }
}