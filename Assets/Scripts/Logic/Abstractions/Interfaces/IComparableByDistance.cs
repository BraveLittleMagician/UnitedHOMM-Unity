#nullable enable

public interface IComparableByDistance
{
    public int CompareByDistanceTo(object? obj);
}
public interface IComparableByDistance<in T>
{
    public int CompareByDistanceTo(T other);
}