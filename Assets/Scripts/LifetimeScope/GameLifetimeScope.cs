using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{

    [SerializeField] private GameView _gameView;

    protected override void Configure(IContainerBuilder builder)
    {
        var configLoader = new GameConfigLoader();
        var boardConfig = configLoader.LoadBoardConfig();
        var seats = new Seats(boardConfig.NumberOfSides, boardConfig.NumberOfPlayersOnSide);
        builder.RegisterInstance(boardConfig);
        builder.RegisterInstance(seats);
        builder.RegisterInstance(configLoader).As<IGameConfigLoader>();
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<LoggerForUnity>(Lifetime.Singleton).As<ILogger>();
        builder.Register<House>(Lifetime.Singleton).As<IHouse>();
        builder.Register<Registry>(Lifetime.Singleton).As<IRegistry>();
        builder.Register<CombatService>(Lifetime.Singleton).As<ICombatService>();
        builder.Register<MovementValidator>(Lifetime.Singleton).As<IMovementValidator>();
        builder.Register<AbilityService>(Lifetime.Singleton).As<IAbilityService>();
        builder.Register<Flow>(Lifetime.Singleton).As<IGameController>();
        builder.RegisterComponent(_gameView).As<IGameView>();
        builder.Register<GamePresenter>(Lifetime.Singleton);
        builder.RegisterEntryPoint<GameInitializer>();
    }
}