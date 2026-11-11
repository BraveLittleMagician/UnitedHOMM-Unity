#nullable enable
using System;
using System.Collections.Generic;

public sealed class ReactiveActionRegistry : IDisposable
{
    private readonly List<IDisposable> _services = new();
    private bool _disposed;

    public T Register<T>(T service) where T : IDisposable
    {
        if (service == null) throw new ArgumentNullException(nameof(service));
        _services.Add(service);
        return service;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        for (int i = _services.Count - 1; i >= 0; i--)
        {
            try { _services[i].Dispose(); }
            catch (Exception) { /* ignore on teardown */ }
        }
        _services.Clear();
    }
}