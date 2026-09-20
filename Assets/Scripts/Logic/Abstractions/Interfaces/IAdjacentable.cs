#nullable enable

public interface IAdjacentable<in T>
{
    bool IsAdjacent(T other);
}