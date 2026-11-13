#nullable enable

using System;
using System.Collections.Generic;

public static class BoardLayoutLoader
{
    public static BoardLayoutResult Load(string json, PieceTemplateRegistry templates)
    {
        if (templates == null) throw new ArgumentNullException(nameof(templates));

        var dto = LoaderOfDataFromJSON.LoadFromJson<BoardLayoutJson>(json);

        ValidateLayout(dto, templates);

        var config = BuildConfig(dto.GridSize ?? throw new NullReferenceException(nameof(dto.GridSize)));
        var placements = BuildPlacements(dto.Placements ?? throw new NullReferenceException(nameof(dto.Placements)), templates);

        return new BoardLayoutResult(config, placements);
    }

    private static void ValidateLayout(BoardLayoutJson dto, PieceTemplateRegistry templates)
    {
        var errors = new List<string>();

        if (dto.GridSize == null) errors.Add("Отсутствует раздел 'GridSize'");
        else ValidateGridSize(dto.GridSize, errors);

        if (dto.Placements == null || dto.Placements.Length == 0) errors.Add("Раздел 'Placements' пуст или отсутствует");
        else ValidatePlacements(dto.Placements, templates, errors);

        if (errors.Count > 0) throw new InvalidOperationException($"BoardLayout.json содержит ошибки:\n  - {string.Join("\n  - ", errors)}");
    }

    private static void ValidateGridSize(BoardSizeJson grid, List<string> errors)
    {
        if (grid.X <= 0) errors.Add($"GridSize.x должен быть > 0, получено {grid.X}");
        if (grid.Y <= 0) errors.Add($"GridSize.y должен быть > 0, получено {grid.Y}");
        if (grid.Z <= 0) errors.Add($"GridSize.z должен быть > 0, получено {grid.Z}");

        if (grid.W.HasValue)
        {
            if (grid.W.Value < 1 || grid.W.Value > 3)
                errors.Add($"GridSize.w должен быть в [1..3], получено {grid.W.Value}");
            if (grid.W.Value == 2 && !IsValidBoolOrNull(grid.SignWNegative))
                errors.Add("GridSize.signWNegative должен быть \"t\"/\"f\"/\"true\"/\"false\" при w=2");
            if (grid.W.Value != 2 && !string.IsNullOrEmpty(grid.SignWNegative))
                errors.Add("GridSize.signWNegative допустим только при w=2");
        }
    }

    private static ConfigOfBoard BuildConfig(BoardSizeJson grid)
    {
        int fieldSize = Math.Max(grid.X, Math.Max(grid.Y, grid.Z));

        int w = 1;
        bool? signWNegative = null;

        if (grid.W.HasValue)
        {
            w = grid.W.Value;
            signWNegative = w == 2 ? ParseBool($"{grid.SignWNegative}") : null;
        }

        var axes = w > 1
            ? MultipleAxesFromTwo.Four
            : (grid.Z > 1 ? MultipleAxesFromTwo.Three : MultipleAxesFromTwo.Two);

        return new ConfigOfBoard
        {
            Axes = axes,
            FieldSize = fieldSize,
            W = w,
            SignWNegative = signWNegative,
            NumberOfSides = 2,
            NumberOfPlayersOnSide = 1,
        };
    }

    private static bool IsValidBoolOrNull(string? value) => value == null || IsValidBool(value);

    private static void ValidatePlacements(PlacementJson[] placements, PieceTemplateRegistry templates, List<string> errors)
    {
        var occupied = new HashSet<(int X, int Y, int Z, int W)>();

        for (int i = 0; i < placements.Length; i++)
        {
            var p = placements[i];
            var prefix = $"Placements[{i}]";

            if (p == null)
            {
                errors.Add($"{prefix}: null-элемент");
                continue;
            }

            if (p.Position == null)
            {
                errors.Add($"{prefix}: отсутствует 'Position'");
                continue;
            }

            if (p.Player == null)
            {
                errors.Add($"{prefix}: отсутствует 'Player'");
                continue;
            }

            if (string.IsNullOrWhiteSpace(p.Name))
            {
                errors.Add($"{prefix}: пустое 'Name'");
                continue;
            }

            if (!templates.Contains(p.Name))
            {
                errors.Add(
                    $"{prefix}: шаблон фигуры '{p.Name}' не зарегистрирован " +
                    $"(проверьте StandardPieceTemplates)");
                continue;
            }

            var key = (p.Position.X, p.Position.Y, p.Position.Z, p.Position.W);
            if (!occupied.Add(key))
            {
                errors.Add(
                    $"{prefix}: позиция ({p.Position.X}, {p.Position.Y}, " +
                    $"{p.Position.Z}, {p.Position.W}) уже занята другой фигурой");
            }

            if (p.Player.IndexOfSide < 0)
                errors.Add($"{prefix}: IndexOfSide не может быть отрицательным");

            if (p.Player.IndexOfPlayerOnSide < 0)
                errors.Add($"{prefix}: IndexOfPlayerOnSide не может быть отрицательным");
        }
    }

    private static IReadOnlyList<StartingPlacement> BuildPlacements(PlacementJson[] placements, PieceTemplateRegistry templates)
    {
        var result = new List<StartingPlacement>(placements.Length);

        foreach (var p in placements)
        {
            var template = templates.Get(p.Name ?? throw new NullReferenceException(nameof(p.Name)));
            if (p.Player == null || p.Position == null) throw new NullReferenceException(nameof(p));
            var owner = new IdOfPlayer(
                p.Player.IndexOfSide,
                p.Player.IndexOfPlayerOnSide);
            var position = new Square
            {
                X = p.Position.X,
                Y = p.Position.Y,
                Z = p.Position.Z,
                W = p.Position.W
            };

            var definition = new PieceDefinition(
                template.Name,
                owner,
                template.Health,
                template.AttackDamage,
                template.AttackCharges,
                template.InitialFacing,
                template.MovementFactories,
                template.Abilities,
                AttackEffect: null,
                Modifiers: template.Modifiers);

            result.Add(new StartingPlacement(definition, position));
        }

        return result;
    }

    private static bool ParseBool(string value) =>
        value.Equals("t", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("true", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidBool(string value) =>
        value.Equals("t", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("f", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("false", StringComparison.OrdinalIgnoreCase);
}