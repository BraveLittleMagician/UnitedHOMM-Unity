#nullable enable

using System;
using System.Collections.Generic;

public class ModifiersOfMovement<TSequence, TPosition> : Modifiers
    where TSequence : notnull, ISequence<TPosition, TSequence>
    where TPosition : struct
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;
    private readonly Dictionary<Type, AttackMovement> _modifiers = new();

    public ModifiersOfMovement(IEventBus eventBus, IPiece owner)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public IEnumerable<ModifierOfMovement<TSequence, TPosition>> Attacks
    {
        get
        {
            foreach (var modifs in _modifiers.Values)
                if (modifs.Attack != null)
                    yield return modifs.Attack;
        }
    }

    public IEnumerable<ModifierOfMovement<TSequence, TPosition>> Movements
    {
        get
        {
            foreach (var modifs in _modifiers.Values)
                if (modifs.Movement != null)
                    yield return modifs.Movement;
        }
    }

    private bool AddPrivate<TModif>(TModif m, bool isAttack) where TModif : ModifierOfMovement
    {
        if (m is not ModifierOfMovement<TSequence, TPosition> modifier)
            return false;

        var type = m.GetType();

        if (!_modifiers.TryGetValue(type, out var modifs))
        {
            modifs = new AttackMovement();
            if (!modifs.Add(modifier, isAttack))
                return false;

            _modifiers[type] = modifs;
            return true;
        }

        return modifs.Add(modifier, isAttack);
    }

    private bool RemovePrivate<TModif>(bool isAttack) where TModif : ModifierOfMovement
    {
        if (!_modifiers.TryGetValue(typeof(TModif), out var gotten)) return false;

        if (!gotten.Remove(isAttack)) return false;

        if (gotten.Attack == null && gotten.Movement == null)
            _modifiers.Remove(typeof(TModif));

        return true;
    }

    public override bool Add<TModif>(TModif m, bool isAttack)
    {
        if (AddPrivate(m, isAttack))
        {
            _eventBus.Publish(new ModifiersUpdatedEvent(_owner, isAttack, typeof(TSequence)));
            return true;
        }
        return false;
    }

    public override bool Remove<TModif>(bool isAttack)
    {
        if (RemovePrivate<TModif>(isAttack))
        {
            _eventBus.Publish(new ModifiersUpdatedEvent(_owner, isAttack, typeof(TSequence)));
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        _modifiers.Clear();
    }

    private class AttackMovement
    {
        public ModifierOfMovement<TSequence, TPosition>? Attack { get; private set; }
        public ModifierOfMovement<TSequence, TPosition>? Movement { get; private set; }

        public bool Add(ModifierOfMovement<TSequence, TPosition> modifier, bool isAttack)
        {
            if (isAttack)
            {
                if (Attack != null) return false;
                Attack = modifier;
                return true;
            }
            else
            {
                if (Movement != null) return false;
                Movement = modifier;
                return true;
            }
        }

        public bool Remove(bool isAttack)
        {
            if (isAttack)
            {
                if (Attack == null) return false;
                Attack = null;
                return true;
            }
            else
            {
                if (Movement == null) return false;
                Movement = null;
                return true;
            }
        }
    }
}