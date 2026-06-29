#nullable enable

using System;

public abstract class Ability : IAbility
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private IDisposable? _subscription;
    private IPiece? _owner;

    protected Ability(TriggerType triggerType, IEffect effect, IEventBus eventBus, ILogger logger)
    {
        TriggerType = triggerType;
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _eventBus = eventBus;
        _logger = logger;
    }

    public TriggerType TriggerType { get; }
    public IEffect Effect { get; }

    public void Activate(IPiece? owner)
    {
        if (_owner != null) return;
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        _subscription = TriggerType switch
        {
            TriggerType.Deploy => SubscribeToDeploy(),
            TriggerType.Initiative => SubscribeToInitiative(),
            TriggerType.Death => SubscribeToDeath(),
            _ => null
        };

        if (_subscription == null)
            _logger.LogWarning($"Неизвестный триггер {TriggerType} для способности {GetType().Name}");
    }

    public void Deactivate()
    {
        _subscription?.Dispose();
        _subscription = null;
        _owner = null;
    }

    public void Dispose() => Deactivate();

    private IDisposable SubscribeToDeploy()
    {
        void Handler(PieceDeployedEvent e)
        {
            if (e.Piece == _owner)
                Effect.Execute(_owner, e.Room);
        }
        _eventBus.Subscribe<PieceDeployedEvent>(Handler);
        return new SubscriptionToken<PieceDeployedEvent>(_eventBus, Handler);
    }

    private IDisposable SubscribeToInitiative()
    {
        void Handler(PieceInitiativeEvent e)
        {
            if (e.Piece == _owner)
                Effect.Execute(_owner, e.Room);
        }
        _eventBus.Subscribe<PieceInitiativeEvent>(Handler);
        return new SubscriptionToken<PieceInitiativeEvent>(_eventBus, Handler);
    }

    private IDisposable SubscribeToDeath()
    {
        void Handler(PieceDiedEvent e)
        {
            if (e.Piece == _owner)
                Effect.Execute(_owner, null);
        }
        _eventBus.Subscribe<PieceDiedEvent>(Handler);
        return new SubscriptionToken<PieceDiedEvent>(_eventBus, Handler);
    }

    private class SubscriptionToken<TEvent> : IDisposable where TEvent : class
    {
        private readonly IEventBus _bus;
        private readonly Action<TEvent> _handler;
        private bool _disposed;

        public SubscriptionToken(IEventBus bus, Action<TEvent> handler)
        {
            _bus = bus;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _bus.Unsubscribe(_handler);
        }
    }
}