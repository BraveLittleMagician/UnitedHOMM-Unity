#nullable enable

using System;

public interface IAbility : IDisposable
{
    TriggerType TriggerType { get; }
    IEffect Effect { get; }
    void Activate(IPiece owner);
    void Deactivate();
}