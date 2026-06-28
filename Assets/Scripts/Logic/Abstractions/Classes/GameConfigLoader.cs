#nullable enable

using System;
using UnityEngine;

public sealed class GameConfigLoader : IGameConfigLoader
{
    public BoardConfig LoadBoardConfig()
    {
        var textAsset = Resources.Load<TextAsset>("appsettings");
        if (textAsset == null) throw new Exception("appsettings.json not found in Resources folder!");

        var wrapper = JsonUtility.FromJson<ConfigWrapper>(textAsset.text);
        if (wrapper?.Board == null)
            throw new Exception("Invalid config format: missing 'Board' section.");

        return new BoardConfig
        {
            Is3D = wrapper.Board.Is3D,
            FieldSize = wrapper.Board.FieldSize,
            FrameThickness = wrapper.Board.FrameThickness,
            NumberOfSides = wrapper.Board.NumberOfSides,
            NumberOfPlayersOnSide = wrapper.Board.NumberOfPlayersOnSide
        };
    }

    [Serializable]
    private class ConfigWrapper
    {
        public BoardData? Board;
    }

    [Serializable]
    private class BoardData
    {
        public bool Is3D = false;
        public int FieldSize = 8;
        public int FrameThickness = 0;
        public int NumberOfSides = 2;
        public int NumberOfPlayersOnSide = 1;
    }
}