#nullable enable
public sealed record UiContext(
    BoardUI BoardUI,
    LayerPanelUI LayerPanelUI,
    PlanUI PlanUI)
{
    public void Run()
    {
        BoardUI.Run();
        LayerPanelUI.Run();
        PlanUI.Run();
    }
}