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
        if (wrapper == null) throw new Exception("Неверный формат конфигурации: отсутствует раздел 'Board'");
        
        int correctedSize = BoardSizeCorrector.CorrectSize(wrapper.Board.FieldSize);

        return new BoardConfig
        {
            Axes = ParseAxes(wrapper.Board.Axes),
            FieldSize = correctedSize,
            NumberOfSides = wrapper.Board.NumberOfSides,
            NumberOfPlayersOnSide = wrapper.Board.NumberOfPlayersOnSide
        };
    }

    private static MultipleAxesFromTwo ParseAxes(string value) => value?.ToLower() switch
    {
        "two" => MultipleAxesFromTwo.Two,
        "three" => MultipleAxesFromTwo.Three,
        "four" => MultipleAxesFromTwo.Four,
        _ => MultipleAxesFromTwo.Two
    };

    [Serializable]
    private class ConfigWrapper
    {
        public BoardData Board = null!;
    }

    [Serializable]
    private class BoardData
    {
        public string Axes = "Two";
        public int FieldSize = 8;
        public int NumberOfSides = 2;
        public int NumberOfPlayersOnSide = 1;
    }
}