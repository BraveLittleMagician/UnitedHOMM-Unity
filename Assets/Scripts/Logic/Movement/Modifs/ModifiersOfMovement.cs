#nullable enable

using System;
using System.Collections.Generic;

public class ModifiersOfMovement<TSequence, TPosition> : Modifiers where TSequence : notnull, ISequence<TPosition, TSequence>, new() where TPosition : struct
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;
    private readonly Dictionary<Type, AttackMovement> _modifiers = new();

    public ModifiersOfMovement(IEventBus eventBus, IPiece owner)
    {
        _eventBus = eventBus;
        _owner = owner;
    }

    public IEnumerable<ModifierOfMovement<TSequence, TPosition>> Attacks
    {
        get
        {
            List<ModifierOfMovement<TSequence, TPosition>> list = new();
            foreach (var modifs in _modifiers.Values)
            {
                if (modifs.Attack != null)
                    list.Add(modifs.Attack);
            }
            return list;
        }
    }
    public IEnumerable<ModifierOfMovement<TSequence, TPosition>> Movements
    {
        get
        {
            List<ModifierOfMovement<TSequence, TPosition>> list = new();
            foreach (var modifs in _modifiers.Values)
            {
                if (modifs.Movement != null)
                    list.Add(modifs.Movement);
            }
            return list;
        }
    }

    private bool AddPrivate<TModif>(TModif m, bool isAttack) where TModif : ModifierOfMovement
    {
        if (m is ModifierOfMovement<TSequence, TPosition> modifier)
        {
            Type type = m.GetType();
            if (_modifiers.TryGetValue(type, out var gotten))
                return gotten.Add(modifier, isAttack);
            else
            {
                AttackMovement modifs = new();
                if (modifs.Add(modifier, isAttack))
                {
                    _modifiers[type] = modifs;
                    return true;
                }
            }
        }
        return false;
    }
    private bool RemovePrivate<TModif>(bool isAttack) where TModif : ModifierOfMovement
    {
        if (_modifiers.TryGetValue(typeof(TModif), out var gotten))
            return gotten.Remove<TModif>(isAttack);
        return false;
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
        GC.SuppressFinalize(this);
    }

    private class AttackMovement
    {
        public ModifierOfMovement<TSequence, TPosition>? Attack { get; private set; }
        public ModifierOfMovement<TSequence, TPosition>? Movement { get; private set; }

        public bool Add(ModifierOfMovement<TSequence, TPosition> modifier, bool isAttack)
        {
            if (isAttack)
            {
                if (Attack == null)
                {
                    Attack = modifier;
                    return true;
                }
            }
            else
            {
                if (Movement == null)
                {
                    Movement = modifier;
                    return true;
                }
            }
            return false;
        }

        public bool Remove<TModif>(bool isAttack) where TModif : ModifierOfMovement
        {
            if (typeof(TModif) == typeof(ModifierOfMovement<TSequence, TPosition>))
            {
                if (isAttack)
                    Attack = null;
                else
                    Movement = null;
                return true;
            }
            return false;
        }
    }
}