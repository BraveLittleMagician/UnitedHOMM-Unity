#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BoardDebugDumper : MonoBehaviour
{
    [SerializeField] private Key _dumpKey = Key.F1;

    private LoaderOfBoard _boardLoader = null!;
    private LayerPresenter _layerPresenter = null!;
    private Camera _cubeCamera = null!;
    private Camera _layerCamera = null!;
    private ViewState _viewState = null!;
    private SelectionState _selectionState = null!;

    public void Initialize(
        LoaderOfBoard boardLoader,
        LayerPresenter layerPresenter,
        Camera cubeCamera,
        Camera layerCamera,
        ViewState viewState,
        SelectionState selectionState)
    {
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _cubeCamera = cubeCamera ?? throw new ArgumentNullException(nameof(cubeCamera));
        _layerCamera = layerCamera ?? throw new ArgumentNullException(nameof(layerCamera));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _selectionState = selectionState ?? throw new ArgumentNullException(nameof(selectionState));
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current[_dumpKey].wasPressedThisFrame) Dump();
    }

    [ContextMenu("Dump Board")]
    public void Dump()
    {
        if (_boardLoader == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("===== BOARD DEBUG =====");
        DumpBoardTransform(sb);
        DumpGrid(sb);
        DumpViewState(sb);
        DumpSelection(sb);
        DumpCameras(sb);
        DumpLayers(sb);
        DumpPieces(sb);
        Debug.Log(sb.ToString());
    }

    private void DumpBoardTransform(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("[Board Transform]");
        sb.AppendLine($"  LoaderOfBoard.position: {Fmt(_boardLoader.transform.position)}");
        sb.AppendLine($"  LayerPresenter.position: {Fmt(_layerPresenter.transform.position)}");
    }

    private void DumpGrid(StringBuilder sb)
    {
        var s = _boardLoader.GridSize;
        sb.AppendLine();
        sb.AppendLine($"[Grid] {s.X}×{s.Y}×{s.Z}×{s.W}");
        sb.AppendLine($"  Step: {_boardLoader.Step:F3} (Spacing: {_boardLoader.Spacing:F3}, WSpread: {_boardLoader.WSpread:F3})");
    }

    private void DumpViewState(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("[View State]");
        sb.AppendLine($"  Axis:              {GridAxes.GetAxisName(_viewState.AxisIndex)} (UI index {_viewState.UIAxisIndex})");
        sb.AppendLine($"  Active layer:      {_viewState.ActiveLayerIndex}");
        sb.AppendLine($"  View direction:    {(_viewState.ViewDirection > 0 ? "+1" : "-1")}");
        sb.AppendLine($"  Layer roll:        {_viewState.LayerRollDegrees}°");
        sb.AppendLine($"  Reverse view:      {_viewState.IsReverseView}");
        sb.AppendLine($"  Layer gap:         {_viewState.LayerGap:F3}");
    }

    private void DumpSelection(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("[Selection]");
        var sel = _selectionState.SelectedInfo;
        if (sel == null)
        {
            sb.AppendLine("  Selected: (none)");
            return;
        }
        var p = sel.Piece;
        sb.AppendLine($"  Selected: {p.Name}#{p.IndexInHouse} [{p.Owner}]");
        sb.AppendLine($"  Grid pos: {Fmt(sel.PositionInGrid)}");
        sb.AppendLine($"  HP: {p.Health}/{p.HealthNormal}  Charges: {p.AttackCharges}/{p.MaxAttackCharges}");
        sb.AppendLine($"  Dmg: {p.AttackDamage}  Facing: {p.Facing}");
    }

    private void DumpCameras(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("[Cameras]");
        sb.AppendLine("  Cube camera:");
        sb.AppendLine($"    rect:     {_cubeCamera.rect}");
        sb.AppendLine($"    position: {Fmt(_cubeCamera.transform.position)}");
        sb.AppendLine($"    forward:  {Fmt(_cubeCamera.transform.forward)}");
        sb.AppendLine("  Layer camera:");
        sb.AppendLine($"    rect:     {_layerCamera.rect}");
        sb.AppendLine($"    position: {Fmt(_layerCamera.transform.position)}");
        sb.AppendLine($"    forward:  {Fmt(_layerCamera.transform.forward)}");
        sb.AppendLine($"    up:       {Fmt(_layerCamera.transform.up)}");
    }

    private void DumpLayers(StringBuilder sb)
    {
        var containers = _layerPresenter.Containers;
        sb.AppendLine();
        sb.AppendLine($"[Layers] ({containers.Count})");
        if (containers.Count == 0)
        {
            sb.AppendLine("  (none)");
            return;
        }

        int activeIndex = _viewState.ActiveLayerIndex;
        var sorted = new List<LayerContainer>(containers);
        sorted.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));

        foreach (var c in sorted)
        {
            if (c == null) continue;
            string marker = c.LayerIndex == activeIndex ? " <-- active" : string.Empty;
            sb.AppendLine($"  Layer {c.LayerIndex}: pieces={c.PieceCount}{marker}");
            sb.AppendLine($"    target local: {Fmt(c.TargetLocalPosition)}");
            sb.AppendLine($"    target world: {Fmt(c.TargetWorldPosition)}");
        }
    }

    private void DumpPieces(StringBuilder sb)
    {
        var views = _boardLoader.AllPieceViews;
        sb.AppendLine();
        sb.AppendLine($"[Pieces] ({views.Count})");

        var lines = new List<string>();
        foreach (var info in views)
        {
            if (info == null) continue;
            var p = info.Piece;
            var g = info.PositionInGrid;
            string flags = "";
            if (info.IsSelected) flags += " [selected]";
            if (info.IsHovered) flags += " [hovered]";
            if (p.IsDead) flags += " [dead]";

            lines.Add(
                $"  {p.Name,-8} #{p.IndexInHouse} [{p.Owner}]  " +
                $"grid={Fmt(g)}  world={Fmt(info.transform.position)}  " +
                $"HP={p.Health}/{p.HealthNormal}  Charges={p.AttackCharges}/{p.MaxAttackCharges}  " +
                $"Dmg={p.AttackDamage}  Facing={p.Facing}" + flags);
        }
        lines.Sort();
        foreach (var line in lines) sb.AppendLine(line);
    }

    private static string Fmt(Vector3 v) => $"({v.x:F3}, {v.y:F3}, {v.z:F3})";
    private static string Fmt(Square s) => $"({s.X},{s.Y},{s.Z},{s.W})";
}