#nullable enable


public interface IComparableByDistance
{
    public int CompareByDistanseTo(object? obj);
}
public interface IComparableByDistance<in T>
{
    public int CompareByDistanseTo(T? other);
}