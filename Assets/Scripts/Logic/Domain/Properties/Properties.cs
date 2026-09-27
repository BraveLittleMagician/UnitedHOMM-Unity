#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public sealed class Properties : IDisposable
{
    private readonly Dictionary<Type, Property> _properties = new();
    private bool _disposed;

    public void Add(Property property)
    {
        if (property == null) throw new ArgumentNullException(nameof(property));

        var type = property.GetType();
        if (_properties.ContainsKey(type)) throw new InvalidOperationException($"Свойство {type.Name} уже добавлено. Используйте TryToGet для получения.");

        _properties[type] = property;
    }
    public bool TryToGet<T>([NotNullWhen(true)] out T? property) where T : Property 
    {
        if (_properties.TryGetValue(typeof(T), out var prop))
        {
            property = (T)prop;
            return true;
        }
        property = null;
        return false;
    }
    public void Apply(IOperation operation)
    {
        foreach (var prop in _properties.Values) prop.Apply(operation);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var prop in _properties.Values)
            prop.Dispose();
        _properties.Clear();
    }
}