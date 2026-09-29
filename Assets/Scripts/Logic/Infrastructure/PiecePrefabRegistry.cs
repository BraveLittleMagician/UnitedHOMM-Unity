#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PiecePrefabRegistry
{
    private readonly Dictionary<string, string> _nameToPath;
    private readonly Dictionary<string, GameObject> _cache = new(StringComparer.Ordinal);

    public PiecePrefabRegistry(Dictionary<string, string> nameToPath)
    {
        _nameToPath = nameToPath ?? throw new ArgumentNullException(nameof(nameToPath));
    }

    public bool Contains(string pieceName)
    {
        if (string.IsNullOrEmpty(pieceName)) return false;
        return _nameToPath.ContainsKey(pieceName);
    }
    public GameObject GetPrefab(string pieceName)
    {
        if (string.IsNullOrEmpty(pieceName))
            throw new ArgumentException("Имя фигуры не может быть пустым", nameof(pieceName));

        if (_cache.TryGetValue(pieceName, out var cached))
            return cached;

        if (!_nameToPath.TryGetValue(pieceName, out var path))
            throw new KeyNotFoundException(
                $"Префаб для фигуры '{pieceName}' не зарегистрирован. " +
                $"Проверьте, что имя есть в PieceLibraryData.json.");

        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException(
                $"У фигуры '{pieceName}' указан пустой путь к префабу.");

        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
            throw new InvalidOperationException(
                $"Префаб '{pieceName}' не найден по пути 'Resources/{path}.prefab'. " +
                $"Проверьте путь в PieceLibraryData.json и наличие файла в Assets/Resources/.");

        ValidatePrefab(prefab, pieceName);
        _cache[pieceName] = prefab;
        return prefab;
    }

    private static void ValidatePrefab(GameObject prefab, string pieceName)
    {
        if (prefab.GetComponentsInChildren<Collider>(includeInactive: false).Length == 0)
            throw new InvalidOperationException(
                $"У префаба '{pieceName}' нет ни одного Collider. " +
                $"Добавьте MeshCollider (Convex) или составной коллайдер.");

        if (prefab.GetComponentInChildren<Renderer>() == null)
            throw new InvalidOperationException(
                $"У префаба '{pieceName}' нет ни одного Renderer.");
    }
}