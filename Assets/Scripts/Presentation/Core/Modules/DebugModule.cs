#nullable enable
using System;
using UnityEngine;

public static class DebugModule
{
    public static DebugContext Initialize(
        DomainContext domain,
        PresentationContext presentation,
        SelectionHub selectionHub,
        PieceDebugLogger? pieceDebugLogger,
        BoardDebugDumper? boardDebugDumper,
        Camera camera,
        Camera layerCamera,
        IEventBus eventBus,
        ILogger logger)
    {
        if (domain == null) throw new ArgumentNullException(nameof(domain));
        if (presentation == null) throw new ArgumentNullException(nameof(presentation));

        if (pieceDebugLogger != null)
        {
            pieceDebugLogger.Initialize(
                presentation.SelectionController,
                selectionHub,
                domain.ModifierActivationHandler,
                domain.PromotionChoiceHandler,
                eventBus,
                logger);
        }

        if (boardDebugDumper != null)
        {
            boardDebugDumper.Initialize(
                presentation.LoaderOfBoard,
                presentation.LayerPresenter,
                camera,
                layerCamera,
                presentation.ViewState,
                presentation.SelectionState);
        }

        return new DebugContext(pieceDebugLogger);
    }
}