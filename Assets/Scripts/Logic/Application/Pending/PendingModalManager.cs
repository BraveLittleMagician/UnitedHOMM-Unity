
#nullable enable

using System;

public sealed class PendingModalManager
{
    private PendingModal? _current;

    public PendingModal? Current => _current;
    public PendingPromotion? CurrentPromotion => _current as PendingPromotion;
    public PendingChoice? CurrentChoice => _current as PendingChoice;
    public bool HasPending => _current != null;

    public event Action? Changed;

    public void Set(PendingModal modal)
    {
        if (ReferenceEquals(_current, modal)) return;
        _current = modal;
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (_current == null) return;
        _current = null;
        Changed?.Invoke();
    }
}
