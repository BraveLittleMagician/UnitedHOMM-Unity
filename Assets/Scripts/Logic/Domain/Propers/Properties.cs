#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public sealed class Properties
{
    private readonly Dictionary<Type, Property> _properties = new();

    public void Add(Property property) => _properties[property.GetType()] = property;

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
        foreach (var prop in _properties.Values)
            prop.Apply(operation);
    }
}