#nullable enable

using System;
using System.Collections.Generic;

public static class PieceLibraryLoader
{
    public static PiecePrefabRegistry Load(string json)
    {
        var dto = LoaderOfDataFromJSON.LoadFromJson<PieceLibraryJson>(json);

        var map = BuildMap(dto);
        ValidateMap(map);

        return new PiecePrefabRegistry(map);
    }

    private static Dictionary<string, string> BuildMap(PieceLibraryJson dto)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        if (dto.Pieces == null)
            throw new InvalidOperationException(
                "PieceLibraryData.json не содержит раздел 'Pieces'.");

        foreach (var entry in dto.Pieces)
        {
            if (entry == null)
                throw new InvalidOperationException(
                    "В разделе 'Pieces' найден null-элемент.");

            if (string.IsNullOrWhiteSpace(entry.Name))
                throw new InvalidOperationException(
                    "В разделе 'Pieces' найдена запись с пустым 'Name'.");

            if (string.IsNullOrWhiteSpace(entry.PathToPrefab))
                throw new InvalidOperationException(
                    $"У фигуры '{entry.Name}' пустой 'PathToPrefab'.");

            if (map.ContainsKey(entry.Name))
                throw new InvalidOperationException(
                    $"В PieceLibraryData.json найдено дублирующееся имя: '{entry.Name}'.");

            map[entry.Name] = entry.PathToPrefab;
        }

        return map;
    }

    private static void ValidateMap(Dictionary<string, string> map)
    {
        if (map.Count == 0)
            throw new InvalidOperationException(
                "PieceLibraryData.json не содержит ни одной фигуры.");
    }
}