#nullable enable

using System;
using UnityEngine;

public sealed class GameConfigLoader : IGameConfigLoader
{
    public BoardConfig LoadBoardConfig()
    {
        var textAsset = Resources.Load<TextAsset>("appsettings");
        if (textAsset == null) throw new Exception("Файл appsettings.json не найден в папке Resources");

        var wrapper = JsonUtility.FromJson<ConfigWrapper>(textAsset.text);
        if (wrapper?.Board == null) throw new Exception("Неверный формат конфигурации: отсутствует раздел 'Board'");

        return new BoardConfig
        {
            Axes = wrapper.Board.Axes,
            FieldSize = wrapper.Board.FieldSize,
            NumberOfSides = wrapper.Board.NumberOfSides,
            NumberOfPlayersOnSide = wrapper.Board.NumberOfPlayersOnSide
        };
    }

    [Serializable]
    private class ConfigWrapper
    {
        public BoardData? Board { get; set; } = null;
    }

    [Serializable]
    private class BoardData
    {
        public MultipleAxesFromTwo Axes { get; set; } = MultipleAxesFromTwo.Two;
        public int FieldSize { get; set; } = 8;
        public int NumberOfSides { get; set; } = 2;
        public int NumberOfPlayersOnSide { get; set; } = 1;
    }
}