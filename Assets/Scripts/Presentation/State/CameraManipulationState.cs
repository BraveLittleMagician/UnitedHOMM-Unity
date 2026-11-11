#nullable enable

using System;

public sealed class CameraManipulationState
{
    public bool IsActive { get; private set; }

    public void SetActive(bool value)
    {
        if (IsActive == value) return;
        IsActive = value;
        Changed?.Invoke();
    }

    public event Action? Changed;
}