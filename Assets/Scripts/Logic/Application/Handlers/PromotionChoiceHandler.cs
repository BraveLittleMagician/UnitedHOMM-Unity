#nullable enable
using System;
using System.Numerics;

public sealed class PromotionChoiceHandler
{
    private readonly PendingModalManager _modalManager;
    private readonly MatchState _matchState;
    private readonly PromotionService _promotionService;
    private readonly ILogger _logger;

    public event Action? StateChanged;
    public event Action<IPiece, string, string>? PromotionOccurred;

    public PromotionChoiceHandler(PromotionChoiceHandlerContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _modalManager = context.PendingModalManager;
        _matchState = context.MatchState;
        _promotionService = context.PromotionService;
        _logger = context.Logger;
    }

    public void HandlePromotionChoice(BigInteger templateIndex)
    {
        var pending = _modalManager.CurrentPromotion;
        if (pending == null)
        {
            _logger.LogWarning($"[PROMOTION] No pending promotion, ignore click {templateIndex}");
            return;
        }
        if (_matchState.IsOver)
        {
            CloseAndNotify();
            return;
        }
        if (pending.Piece == null || pending.Piece.IsDead)
        {
            _logger.LogWarning($"[PROMOTION] Piece is dead, closing modal");
            CloseAndNotify();
            return;
        }

        PromotionOption? chosen = null;
        for (int i = 0; i < pending.Result.Options.Count; i++)
        {
            if (pending.Result.Options[i].TemplateIndex == templateIndex)
            {
                chosen = pending.Result.Options[i];
                break;
            }
        }
        if (chosen == null)
        {
            _logger.LogWarning($"[PROMOTION] Option {templateIndex} not in list");
            return;
        }

        try
        {
            bool ok = _promotionService.Apply(pending.Piece, chosen);
            if (!ok)
            {
                _logger.LogError($"[PROMOTION] Apply returned false, closing");
                CloseAndNotify();
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PROMOTION] Apply threw: {ex}");
            CloseAndNotify();
            return;
        }

        _modalManager.Clear();
        PromotionOccurred?.Invoke(pending.Piece, pending.Result.FromName, chosen.Name);
        StateChanged?.Invoke();
    }

    private void CloseAndNotify()
    {
        _modalManager.Clear();
        StateChanged?.Invoke();
    }
}