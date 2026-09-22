#nullable enable

using System;

public abstract class Modifiers : IDisposable
{
    public abstract bool Add<TModif>(TModif m, bool isAttack) where TModif : ModifierOfMovement;
    public abstract bool Remove<TModif>(bool isAttack) where TModif : ModifierOfMovement;
    public abstract void Dispose();
}