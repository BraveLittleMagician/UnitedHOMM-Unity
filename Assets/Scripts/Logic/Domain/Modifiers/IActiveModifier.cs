#nullable enable
public interface IActiveModifier : IModifier
{
    bool TriggersStartedAction { get; }
    bool CanActivate(ModifierActivationContext context);
    void Activate(ModifierActivationContext context);
}