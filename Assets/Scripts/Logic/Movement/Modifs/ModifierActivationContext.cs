#nullable enable

using System;

public sealed class ModifierActivationContext
{
    public IPiece Owner { get; }
    public IEventBus EventBus { get; }
    public ModifierHost Host { get; }
    public IRegistry Registry { get; }

    public ModifierActivationContext(IPiece owner, IEventBus eventBus, ModifierHost host, IRegistry registry)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Host = host ?? throw new ArgumentNullException(nameof(host));
        Registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }
}