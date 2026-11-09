#nullable enable
using System;

public sealed class ModifierContext
{
    public PieceModel Owner { get; }
    public PieceEventBus EventBus { get; }
    public ModifierHost Host { get; }
    public Action<IModifier> SelfRemove { get; }
    public int InitialDuration { get; }
    public int InitialCharges { get; }

    public ModifierContext(
        PieceModel owner,
        PieceEventBus eventBus,
        ModifierHost host,
        Action<IModifier> selfRemove,
        int initialDuration,
        int initialCharges)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Host = host ?? throw new ArgumentNullException(nameof(host));
        SelfRemove = selfRemove ?? throw new ArgumentNullException(nameof(selfRemove));
        if (initialDuration < -1)
            throw new ArgumentOutOfRangeException(nameof(initialDuration), "Duration must be >= -1.");
        if (initialCharges < -1)
            throw new ArgumentOutOfRangeException(nameof(initialCharges), "Charges must be >= -1.");
        InitialDuration = initialDuration;
        InitialCharges = initialCharges;
    }
}