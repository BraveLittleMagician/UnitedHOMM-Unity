#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Сравнивает объекты по ссылке (ReferenceEquals), а не по значению.
/// Аналог System.Collections.Generic.ReferenceEqualityComparer из .NET 5+,
/// но доступный в Unity.
/// </summary>
public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
    public static readonly ReferenceEqualityComparer<T> Instance = new();

    private ReferenceEqualityComparer() { }

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}