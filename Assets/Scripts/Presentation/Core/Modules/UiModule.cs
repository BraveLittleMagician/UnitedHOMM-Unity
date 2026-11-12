#nullable enable
using System;

public static class UiModule
{
    public static UiContext Initialize(
        DomainContext domain,
        PresentationContext presentation,
        BoardUI boardUI,
        LayerPanelUI layerPanelUI,
        PlanUI planUI,
        PromotionUI promotionUI,
        ActionChoiceUI actionChoiceUI,
        UIPointerProbe uiPointerProbe,
        ILogger logger)
    {
        if (domain == null) throw new ArgumentNullException(nameof(domain));
        if (presentation == null) throw new ArgumentNullException(nameof(presentation));

        var boardCommands = new BoardCommands(presentation.LoaderOfBoard, presentation.LayerPresenter, presentation.ViewState);

        boardUI.Initialize(
            presentation.LoaderOfBoard,
            presentation.ViewState,
            presentation.SelectionState,
            boardCommands,
            uiPointerProbe);

        layerPanelUI.Initialize(presentation.ViewState, boardCommands, uiPointerProbe);

        planUI.Initialize(
            domain.Flow,
            domain.TurnManager,
            presentation.SelectionState,
            presentation.SelectionController,
            presentation.TargetSelectionController,
            domain.ModifierActivationHandler,
            domain.PendingModalManager,
            uiPointerProbe,
            domain.MatchState,
            domain.ModifierHost,
            logger);

        promotionUI.Initialize(domain.PromotionChoiceHandler, domain.PendingModalManager, uiPointerProbe);
        actionChoiceUI.Initialize(domain.CellActionHandler, domain.PendingModalManager, uiPointerProbe);

        return new UiContext(boardUI, layerPanelUI, planUI);
    }
}