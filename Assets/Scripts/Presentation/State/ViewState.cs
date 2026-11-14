#nullable enable
using System;
using UnityEngine;

public sealed class ViewState
{
    private const int _lastLayer = int.MaxValue;
    private readonly bool[] _flippedByAxis = new bool[GridAxes.Count];

    private static readonly int[] _normalDir = { -1, +1, -1, +1 };
    private static readonly int[] _reverseDir = { +1, -1, -1, -1 };
    private static readonly int[] _normalLayer = { _lastLayer, 0, _lastLayer, 0 };
    private static readonly int[] _reverseLayer = { 0, _lastLayer, _lastLayer, _lastLayer };
    private static readonly int[] _normalRoll = { 0, 0, 0, 0 };
    private static readonly int[] _reverseRoll = { 0, 0, 180, 180 };

    public float LayerGap { get; private set; }
    public int AxisIndex { get; private set; }
    public int ActiveLayerIndex { get; private set; }
    public int ActiveWIndex { get; private set; }
    public int LayerRollDegrees { get; private set; }
    public bool IsReverseView { get; private set; }
    public int WWOffset { get; private set; }
    public int UIAxisIndex => GridAxes.ToUIAxis(AxisIndex);

    private int[] CurrentRoll => IsReverseView ? _reverseRoll : _normalRoll;

    public int ViewDirection
    {
        get
        {
            int baseDir = (IsReverseView ? _reverseDir : _normalDir)[AxisIndex];
            return _flippedByAxis[AxisIndex] ? -baseDir : baseDir;
        }
    }

    public void Initialize(int axisIndex, int activeLayerIndex, int activeWIndex, int wwOffset)
    {
        if (axisIndex == GridAxes.W)
            throw new ArgumentOutOfRangeException(nameof(axisIndex),
                "AxisIndex must be X, Y, or Z. W is controlled via ActiveWIndex.");

        AxisIndex = axisIndex;
        ActiveLayerIndex = activeLayerIndex;
        ActiveWIndex = activeWIndex;
        WWOffset = wwOffset;
        LayerRollDegrees = CurrentRoll[AxisIndex];
    }

    public void SetAxis(int axisIndex)
    {
        if (axisIndex == GridAxes.W)
        {
            Debug.LogWarning("[ViewState] SetAxis(W) ignored: W is not a slice axis. Use SetActiveWIndex.");
            return;
        }
        if (axisIndex == AxisIndex) return;
        AxisIndex = axisIndex;
        int newRoll = CurrentRoll[AxisIndex];
        bool rollChanged = newRoll != LayerRollDegrees;
        LayerRollDegrees = newRoll;
        AxisChanged?.Invoke();
        if (rollChanged) LayerRollChanged?.Invoke();
    }

    public void SetActiveWIndex(int index)
    {
        if (ActiveWIndex == index) return;
        ActiveWIndex = index;
        ActiveWIndexChanged?.Invoke();
    }

    public void SetActiveLayer(int index)
    {
        if (ActiveLayerIndex == index) return;
        ActiveLayerIndex = index;
        ActiveLayerChanged?.Invoke();
    }

    public void StepActiveLayer(int delta, int maxLayer)
    {
        int target = Mathf.Clamp(ActiveLayerIndex + delta, 0, maxLayer);
        SetActiveLayer(target);
    }

    public void SetReverseView(bool value)
    {
        if (IsReverseView == value) return;
        IsReverseView = value;
        for (int i = 0; i < GridAxes.Count; i++) _flippedByAxis[i] = false;
        int newRoll = CurrentRoll[AxisIndex];
        bool rollChanged = newRoll != LayerRollDegrees;
        LayerRollDegrees = newRoll;
        ViewFlipped?.Invoke();
        if (rollChanged) LayerRollChanged?.Invoke();
    }

    public void Flip()
    {
        _flippedByAxis[AxisIndex] = !_flippedByAxis[AxisIndex];
        ViewFlipped?.Invoke();
    }

    public void SetLayerGap(float gap)
    {
        float clamped = Mathf.Max(0f, gap);
        if (Mathf.Approximately(clamped, LayerGap)) return;
        LayerGap = clamped;
        LayerGapChanged?.Invoke();
    }

    public void RotateLayerCamera(int deltaDegrees)
    {
        int next = (LayerRollDegrees + deltaDegrees) % 360;
        if (next < 0) next += 360;
        if (next == LayerRollDegrees) return;
        LayerRollDegrees = next;
        LayerRollChanged?.Invoke();
    }

    public int GetDefaultLayerForCurrentAxis() => (IsReverseView ? _reverseLayer : _normalLayer)[AxisIndex];
    public static int GetDefaultLayerFor(int axis, bool isReverse) => (isReverse ? _reverseLayer : _normalLayer)[axis];

    public event Action? AxisChanged;
    public event Action? ActiveLayerChanged;
    public event Action? ActiveWIndexChanged;
    public event Action? ViewFlipped;
    public event Action? LayerGapChanged;
    public event Action? LayerRollChanged;
}