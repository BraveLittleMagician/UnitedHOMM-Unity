#nullable enable
using System;
using UnityEngine;

public sealed class BoardUI : GuiPanelBehaviour, IRunnable
{
    [SerializeField] private float _panelWidth = 340f;
    [SerializeField] private float _panelHeight = 460f;
    [SerializeField] private float _panelMargin = 10f;

    private static readonly string[] _axisNames = { "X", "Y", "Z", "W" };

    private LoaderOfBoard _boardLoader = null!;
    private ViewState _viewState = null!;
    private SelectionState _selectionState = null!;
    private BoardCommands _boardCommands = null!;

    private string _inputX = "";
    private string _inputY = "";
    private string _inputZ = "";
    private string _inputW = "";
    private bool _inputsInitialized;

    private InfoOfPiece? _selectedInfo;
    private string _feedback = "";
    private float _feedbackClearTime = -1f;
    private const float _feedbackDuration = 3f;

    protected override Rect ComputePanelRect(int screenWidth, int screenHeight)
        => new(_panelMargin, _panelMargin, _panelWidth, _panelHeight);

    public void Initialize(
        LoaderOfBoard boardLoader,
        ViewState viewState,
        SelectionState selectionState,
        BoardCommands boardCommands,
        UIPointerProbe uiPointerProbe)
    {
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _selectionState = selectionState ?? throw new ArgumentNullException(nameof(selectionState));
        _boardCommands = boardCommands ?? throw new ArgumentNullException(nameof(boardCommands));
        InitializePanel(uiPointerProbe);
    }

    public void Run()
    {
        RegisterPanel();
        _selectionState.Changed += OnSelectionChanged;
        AddSubscription(() => _selectionState.Changed -= OnSelectionChanged);
    }

    private void OnSelectionChanged() => _selectedInfo = _selectionState.SelectedInfo;

    private void OnGUI()
    {
        if (!IsRegistered || _boardLoader == null) return;
        EnsureInputsInitialized();
        EnsurePanelRectCurrent();

        GUILayout.BeginArea(PanelRect);
        GUILayout.Box("Board Controller");
        DrawGridSizeControls();
        GUILayout.Space(8);
        DrawSpacingControl();
        DrawLayerGapControl();
        GUILayout.Space(8);
        DrawAxisControls();
        GUILayout.Space(8);
        DrawSelectionInfo();
        GUILayout.EndArea();
    }

    private void EnsureInputsInitialized()
    {
        if (_inputsInitialized) return;
        var s = _boardLoader.GridSize;
        _inputX = s.X.ToString();
        _inputY = s.Y.ToString();
        _inputZ = s.Z.ToString();
        _inputW = s.W.ToString();
        _inputsInitialized = true;
    }

    private void DrawGridSizeControls()
    {
        Square size = _boardLoader.GridSize;
        GUILayout.Label($"Размер поля: {size.X}×{size.Y}×{size.Z}×{size.W}");
        GUILayout.BeginHorizontal();
        GUILayout.Label("X:", GUILayout.Width(20));
        _inputX = GUILayout.TextField(_inputX, 3, GUILayout.Width(40));
        GUILayout.Label("Y:", GUILayout.Width(20));
        _inputY = GUILayout.TextField(_inputY, 3, GUILayout.Width(40));
        GUILayout.Label("Z:", GUILayout.Width(20));
        _inputZ = GUILayout.TextField(_inputZ, 3, GUILayout.Width(40));
        GUILayout.Label("W:", GUILayout.Width(20));
        _inputW = GUILayout.TextField(_inputW, 3, GUILayout.Width(40));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Применить размер")) TryApplySize();

        if (_feedbackClearTime > 0f && Time.unscaledTime >= _feedbackClearTime)
        {
            _feedback = "";
            _feedbackClearTime = -1f;
        }
        if (!string.IsNullOrEmpty(_feedback)) GUILayout.Label(_feedback);
    }

    private void TryApplySize()
    {
        if (!int.TryParse(_inputX, out int x) ||
            !int.TryParse(_inputY, out int y) ||
            !int.TryParse(_inputZ, out int z) ||
            !int.TryParse(_inputW, out int w))
        {
            SetFeedback("Введите целые числа во все поля.");
            return;
        }
        if (x < 1 || y < 1 || z < 1 || w < 1)
        {
            SetFeedback("Все размеры должны быть >= 1.");
            return;
        }

        _boardCommands.SetGridSize(new Square { X = x, Y = y, Z = z, W = w });
        var applied = _boardLoader.GridSize;
        _inputX = applied.X.ToString();
        _inputY = applied.Y.ToString();
        _inputZ = applied.Z.ToString();
        _inputW = applied.W.ToString();
        SetFeedback("Размер применён.");
    }

    private void DrawSpacingControl()
    {
        GUILayout.Label($"Расстояние между фигурами: {_boardLoader.Spacing:F3}");
        float newSpacing = GUILayout.HorizontalSlider(_boardLoader.Spacing, 0f, 1f);
        if (!Mathf.Approximately(newSpacing, _boardLoader.Spacing))
            _boardCommands.SetSpacing(newSpacing);
    }

    private void DrawLayerGapControl()
    {
        GUILayout.Label($"Зазор между слоями: {_viewState.LayerGap:F3}");
        float newGap = GUILayout.HorizontalSlider(_viewState.LayerGap, 0f, 1f);
        if (!Mathf.Approximately(newGap, _viewState.LayerGap))
            _boardCommands.SetLayerGap(newGap);
    }

    private void DrawAxisControls()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Ось:", GUILayout.Width(40));
        int newAxis = GUILayout.Toolbar(_viewState.UIAxisIndex, _axisNames);
        if (newAxis != _viewState.UIAxisIndex) _boardCommands.SetAxis(newAxis);
        GUILayout.EndHorizontal();
    }

    private void DrawSelectionInfo()
    {
        var selected = _selectedInfo;
        if (selected != null && !selected.IsDead)
        {
            var piece = selected.Piece;
            var pos = selected.PositionInGrid;
            GUILayout.Label($"Выбрана: {piece.Name} [{piece.Owner}]");
            GUILayout.Label($"Позиция: ({pos.X}, {pos.Y}, {pos.Z}, {pos.W})");
            GUILayout.Label($"HP: {piece.Health}/{piece.HealthNormal}  Charges: {piece.AttackCharges}/{piece.MaxAttackCharges}");
            GUILayout.Label($"Dmg: {piece.AttackDamage}  Facing: {piece.Facing}");
        }
        else
        {
            GUILayout.Label("Фигура не выбрана.");
        }
    }

    private void SetFeedback(string message)
    {
        _feedback = message;
        _feedbackClearTime = Time.unscaledTime + _feedbackDuration;
    }
}