#nullable enable

using System;
using UnityEngine;

public sealed class LoaderOfConfig
{
    public ConfigOfBoard LoadBoardConfig()
    {
        var textAsset = Resources.Load<TextAsset>("appsettings");
        if (textAsset == null) throw new Exception("Файл appsettings.json не найден в папке Resources");
        var wrapper = JsonUtility.FromJson<ConfigWrapper>(textAsset.text) ?? throw new Exception("Не удалось распарсить appsettings.json");
        if (wrapper.Board == null) throw new Exception("В appsettings.json отсутствует раздел 'Board'");
        int correctedSize = BoardSizeCorrector.CorrectSize(wrapper.Board.FieldSize);

        return new ConfigOfBoard
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