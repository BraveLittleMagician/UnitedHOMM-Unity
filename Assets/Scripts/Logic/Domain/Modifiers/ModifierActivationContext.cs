#nullable enable
using System;

public sealed class ModifierActivationContext
{
    public PieceModel Owner { get; }
    public PieceEventBus EventBus { get; }
    public ModifierHost Host { get; }
    public PieceRegistry Registry { get; }

    public ModifierActivationContext(PieceModel owner, PieceEventBus eventBus, ModifierHost host, PieceRegistry registry)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Host = host ?? throw new ArgumentNullException(nameof(host));
        Registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }
}