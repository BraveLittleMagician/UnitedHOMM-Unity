#nullable enable

using System;

public interface IAbilityService : IDisposable
{
    void ActivateAbilities(IPiece piece);
    void DeactivateAbilities(IPiece piece);
    void ActivateAbilitiesByTrigger(IPiece piece, TriggerType trigger);
}