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
            WUp = ParseTrueFalse(wrapper.Board.WUp),
            WDown = ParseTrueFalse(wrapper.Board.WDown),
            NumberOfSides = wrapper.Board.NumberOfSides,
            NumberOfPlayersOnSide = wrapper.Board.NumberOfPlayersOnSide
        };
    }

    private static MultipleAxesFromTwo ParseAxes(int value) => value switch
    {
        2 => MultipleAxesFromTwo.Two,
        3 => MultipleAxesFromTwo.Three,
        4 => MultipleAxesFromTwo.Four,
        _ => MultipleAxesFromTwo.Two
    };
    private static bool ParseTrueFalse(string value) => value?.ToLower() switch
    {
        "f" => false,
        "t" => true,
        _ => false,
    };

    [Serializable]
    private class ConfigWrapper
    {
        public BoardData Board = null!;
    }

    [Serializable]
    private class BoardData
    {
        public int Axes = 2;
        public int FieldSize = 8;
        public string WUp = "t";
        public string WDown = "t";
        public int NumberOfSides = 2;
        public int NumberOfPlayersOnSide = 1;
    }
}