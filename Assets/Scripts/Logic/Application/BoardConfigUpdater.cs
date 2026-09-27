#nullable enable

using System;

public sealed class BoardConfigUpdater
{
    private readonly Board _board;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private ConfigOfBoard _currentConfig;

    public BoardConfigUpdater(Board board, IEventBus eventBus, ILogger logger, ConfigOfBoard initialConfig)
    {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentConfig = initialConfig ?? throw new ArgumentNullException(nameof(initialConfig));
    }

    public void UpdateConfig(ConfigOfBoard newConfig)
    {
        if (newConfig == null) throw new ArgumentNullException(nameof(newConfig));

        int minimalSize = _board.GetMinimalFieldSize();
        var finalConfig = BoardSizeCorrector.CorrectConfig(newConfig, minimalSize);

        if (finalConfig == _currentConfig)
        {
            _logger.LogDebug("Новая конфигурация совпадает с текущей, событие не публикуется.");
            return;
        }

        var oldConfig = _currentConfig;
        _currentConfig = finalConfig;
        _eventBus.Publish(new BoardConfigChangedEvent(oldConfig, finalConfig));
        _logger.Log($"Конфигурация доски обновлена: FieldSize = {finalConfig.FieldSize}, Axes = {finalConfig.Axes}");
    }
}