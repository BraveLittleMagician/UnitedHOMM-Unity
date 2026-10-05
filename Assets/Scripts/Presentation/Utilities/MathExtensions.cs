#nullable enable

using UnityEngine;

public static class MathExtensions
{
    public static int GetMaxDividedByTwo(int x, int y, int z) => Mathf.CeilToInt(Mathf.Max(x, y, z) / 2f);
}