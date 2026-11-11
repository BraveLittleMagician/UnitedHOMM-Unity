#nullable enable

using System.Collections.Generic;
using UnityEngine;

public static class BoxMeshCache
{
    private static readonly Dictionary<Vector3Int, Mesh> _cache = new();

    public static Mesh Get(Vector3 size)
    {
        var key = new Vector3Int(
            Mathf.RoundToInt(size.x * 1000f),
            Mathf.RoundToInt(size.y * 1000f),
            Mathf.RoundToInt(size.z * 1000f));
        if (_cache.TryGetValue(key, out var mesh)) return mesh;
        mesh = Build(size);
        _cache[key] = mesh;
        return mesh;
    }

    private static Mesh Build(Vector3 size)
    {
        Vector3 h = size * 0.5f;
        Vector3[] vertices =
        {
            new(-h.x, -h.y, -h.z), new( h.x, -h.y, -h.z),
            new( h.x,  h.y, -h.z), new(-h.x,  h.y, -h.z),
            new(-h.x, -h.y,  h.z), new( h.x, -h.y,  h.z),
            new( h.x,  h.y,  h.z), new(-h.x,  h.y,  h.z),
        };
        int[] triangles =
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            3, 7, 6, 3, 6, 2,
            0, 4, 7, 0, 7, 3,
            1, 2, 6, 1, 6, 5,
        };
        var mesh = new Mesh
        {
            name = $"Box_{size.x:F3}_{size.y:F3}_{size.z:F3}",
            vertices = vertices,
            triangles = triangles,
            uv = new Vector2[]
            {
                new(0, 0), new(1, 0),
                new(1, 1), new(0, 1),
                new(0, 0), new(1, 0),
                new(1, 1), new(0, 1),
            }
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}