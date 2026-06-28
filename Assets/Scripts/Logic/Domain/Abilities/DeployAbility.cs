#nullable enable


public sealed class DeployAbility : Ability
{
    public DeployAbility(IEffect effect, IEventBus eventBus, ILogger logger)
        : base(TriggerType.Deploy, effect, eventBus, logger) { }
}