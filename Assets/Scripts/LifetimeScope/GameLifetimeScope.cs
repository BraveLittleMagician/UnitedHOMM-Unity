#nullable enable

using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private ControllerOfCamera _cameraController = null!;
    [SerializeField] private GameView _gameView = null!;
    [SerializeField] private bool _autoStart = true;

    private void Start()
    {
       if (_cameraController == null) throw new NullReferenceException(nameof(_cameraController));
       if (_gameView == null) throw new NullReferenceException(nameof(_gameView));
    }

    protected override void Configure(IContainerBuilder builder)
    {
        var configLoader = new GameConfigLoader();
        var boardConfig = configLoader.LoadBoardConfig();
        var seats = new Seats(boardConfig.NumberOfSides, boardConfig.NumberOfPlayersOnSide);
        var axes = (MultipleAxes)boardConfig.Axes;
        var box = new AxisAlignedBox(axes, boardConfig.FieldSize, 0);
        builder.RegisterInstance(boardConfig);
        builder.RegisterInstance(seats);
        builder.RegisterInstance(configLoader).As<IGameConfigLoader>();
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<LoggerForUnity>(Lifetime.Singleton).As<ILogger>();
        builder.Register<House>(Lifetime.Singleton).As<IHouse>();
        builder.Register<Registry>(Lifetime.Singleton).As<IRegistry>();
        builder.Register(resolver =>
        {
            var eventBus = resolver.Resolve<IEventBus>();
            var logger = resolver.Resolve<ILogger>();
            return new Board(seats, box, eventBus, logger);
        }, Lifetime.Singleton);
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
        builder.RegisterComponent(_gameView).As<IGameView>();
        if (_gameView.TryGetComponent<GridRenderer>(out var gridRenderer))
            builder.RegisterComponent(gridRenderer);
        builder.Register<GamePresenter>(Lifetime.Singleton);
        builder.RegisterComponent(_cameraController).As<IControllerOfCamera>();
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