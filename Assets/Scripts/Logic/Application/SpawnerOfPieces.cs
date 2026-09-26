#nullable enable

using System;
using System.Collections.Generic;

public sealed class SpawnerOfPieces
{
    private readonly IFlow _flow;
    private readonly ILogger _logger;
    private readonly IReadOnlyList<StartingPlacement> _placements;

    public SpawnerOfPieces(IFlow flow, ILogger logger, IReadOnlyList<StartingPlacement> placements)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _placements = placements ?? throw new ArgumentNullException(nameof(placements));
    }

    public void SpawnPieces()
    {
        foreach (var placement in _placements)
        {
            var result = _flow.AddPiece<Board, Square>(placement.Definition, placement.Position);

            if (result.IsSuccess)
                _logger.LogDebug($"Развёрнута фигура {result.Value} в {placement.Position}");
            else
                _logger.LogError($"Не удалось развернуть {placement.Definition.Name} " +
                                 $"в {placement.Position}: {result.Error}");
        }

        _logger.Log($"Расстановка завершена: {_placements.Count} фигур");
    }
}
