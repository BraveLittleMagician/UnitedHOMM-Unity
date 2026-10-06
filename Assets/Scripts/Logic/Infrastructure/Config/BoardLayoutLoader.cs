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

        if (!IsValidBool($"{grid.WUp}"))
            errors.Add($"GridSize.wUp должен быть \"t\"/\"f\" или \"true\"/\"false\", получено '{grid.WUp}'");

        if (!IsValidBool($"{grid.WDown}"))
            errors.Add($"GridSize.wDown должен быть \"t\"/\"f\" или \"true\"/\"false\", получено '{grid.WDown}'");
    }

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

    private static ConfigOfBoard BuildConfig(BoardSizeJson grid)
    {
        bool wUp = ParseBool  ($"{grid.WUp}");
        bool wDown = ParseBool($"{grid.WDown}");

        int fieldSize = Math.Max(grid.X, Math.Max(grid.Y, grid.Z));

        var axes = (wUp || wDown) ? MultipleAxesFromTwo.Four
                 : (grid.Z > 1) ? MultipleAxesFromTwo.Three
                 : MultipleAxesFromTwo.Two;

        return new ConfigOfBoard
        {
            Axes = axes,
            FieldSize = fieldSize,
            WUp = wUp,
            WDown = wDown,
            NumberOfSides = 2,
            NumberOfPlayersOnSide = 1
        };
    }

    private static IReadOnlyList<StartingPlacement> BuildPlacements(PlacementJson[] placements, PieceTemplateRegistry templates)
    {
        var result = new List<StartingPlacement>(placements.Length);

        foreach (var p in placements)
        {
            var template = templates.Get(p.Name ?? throw new NullReferenceException(nameof(p.Name)));
            if (p.Player == null || p.Position == null) throw new NullReferenceException(nameof(p));
            var owner = new IndexOfPlayer(
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
                template.MovementFactories,
                template.MeleeAttacks,
                template.RangedAttacks,
                template.Abilities);

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