#nullable enable
using System;
using UnityEngine;

public sealed class LayerPanelUI : GuiPanelBehaviour, IRunnable
{
    [SerializeField] private float _panelWidth = 400f;
    [SerializeField] private float _panelHeight = 160f;

    private ViewState _viewState = null!;
    private BoardCommands _boardCommands = null!;

    protected override Rect ComputePanelRect(int screenWidth, int screenHeight)
    {
        float x = screenWidth * ViewportLayout.LeftCameraWidthFraction;
        float y = screenHeight - _panelHeight;
        return new Rect(x, y, _panelWidth, _panelHeight);
    }

    public void Initialize(ViewState viewState, BoardCommands boardCommands, UIPointerProbe uiPointerProbe)
    {
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _boardCommands = boardCommands ?? throw new ArgumentNullException(nameof(boardCommands));
        InitializePanel(uiPointerProbe);
    }

    public void Run() => RegisterPanel();

    private void OnGUI()
    {
        if (!IsRegistered || _viewState == null) return;
        EnsurePanelRectCurrent();

        GUILayout.BeginArea(PanelRect);
        GUILayout.Box("Layer view");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("←", GUILayout.Width(60), GUILayout.Height(40))) _boardCommands.StepActiveLayer(-1);
        if (GUILayout.Button("→", GUILayout.Width(60), GUILayout.Height(40))) _boardCommands.StepActiveLayer(+1);
        if (GUILayout.Button("Развернуться", GUILayout.Height(40))) _boardCommands.FlipView();
        if (GUILayout.Button("⟲ 90°", GUILayout.Width(60), GUILayout.Height(40))) _boardCommands.RotateLayerCamera(+90);
        if (GUILayout.Button("⟳ 90°", GUILayout.Width(60), GUILayout.Height(40))) _boardCommands.RotateLayerCamera(-90);
        GUILayout.EndHorizontal();

        bool newReverse = GUILayout.Toggle(_viewState.IsReverseView, " Обратный показ");
        if (newReverse != _viewState.IsReverseView) _boardCommands.SetReverseView(newReverse);

        GUILayout.Label($"Активный слой: {_viewState.ActiveLayerIndex} (ось {GridAxes.GetAxisName(_viewState.AxisIndex)})");
        GUILayout.EndArea();
    }
}