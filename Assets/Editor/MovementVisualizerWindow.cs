#if UNITY_EDITOR

using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class MovementVisualizerWindow : EditorWindow
{
    private Func<Square, ISequenceEnumerator<Square>> _factory;
    private string _title = "—";
    private Square _start = Square.Zero;
    private int _radius = 7;
    private int _sliceZ = 0;
    private int _sliceW = 0;
    private float _cellSize = 25f;

    [MenuItem("Window/Movement Visualizer")]
    public static void Open() => GetWindow<MovementVisualizerWindow>("Movement Vis");

    public static void Show(
        string title,
        Func<Square, ISequenceEnumerator<Square>> factory,
        Square start = default,
        int radius = 7)
    {
        var w = GetWindow<MovementVisualizerWindow>("Movement Vis");
        w._title = title;
        w._factory = factory;
        w._start = start;
        w._radius = radius;
        w.Repaint();
    }

    private void OnGUI()
    {
        DrawToolbar();
        if (_factory == null)
        {
            EditorGUILayout.HelpBox(
                "No sequence selected. Call MovementVisualizerWindow.Show(...) from a Debug menu.",
                MessageType.Info);
            return;
        }

        ISequenceEnumerator<Square> e;
        try { e = _factory(_start); }
        catch (Exception ex) { EditorGUILayout.HelpBox(ex.ToString(), MessageType.Error); return; }

        EditorGUILayout.LabelField(
            $"All: {e.PositionsAll.Count}   Stay: {e.PositionsStayables.Count}   " +
            $"Attack: {e.PositionsAttackable.Count}   MaxDepth: {e.MaxDepth}");

        DrawGrid(e);
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label(_title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (_factory != null && GUILayout.Button("Copy as text", EditorStyles.toolbarButton))
                EditorGUIUtility.systemCopyBuffer = BuildTextGrid(_factory(_start));
        }
        _radius = EditorGUILayout.IntSlider("Radius", _radius, 1, 25);
        _sliceZ = EditorGUILayout.IntSlider("Slice Z", _sliceZ, 0, 15);
        _sliceW = EditorGUILayout.IntSlider("Slice W", _sliceW, 0, 7);
        _cellSize = EditorGUILayout.Slider("Cell size", _cellSize, 14, 48);
    }

    private void DrawGrid(ISequenceEnumerator<Square> e)
    {
        var all = e.PositionsAll;
        var attack = e.PositionsAttackable;
        int n = _radius * 2 + 1;
        float size = n * _cellSize;
        var rect = GUILayoutUtility.GetRect(size, size);
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

        for (int dy = -_radius; dy <= _radius; dy++)
            for (int dx = -_radius; dx <= _radius; dx++)
            {
                int x = _start.X + dx, y = _start.Y + dy;
                var cell = new Square { X = x, Y = y, Z = _sliceZ, W = _sliceW };
                var r = new Rect(
                    rect.x + (dx + _radius) * _cellSize,
                    rect.y + (_radius - dy) * _cellSize,
                    _cellSize - 1, _cellSize - 1);

                Color c = new Color(0.2f, 0.2f, 0.2f);
                if (all.TryGetValue(cell, out var stay))
                    c = stay == Stayable.Stay ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.5f, 0.5f, 0.5f);
                if (attack.TryGetValue(cell, out var atk) && atk == Attackable.Attack)
                    c = new Color(0.9f, 0.3f, 0.3f);
                if (cell.Equals(_start)) c = Color.Lerp(c, Color.yellow, 0.5f);

                EditorGUI.DrawRect(r, c);
                GUI.Label(r, $"{x},{y}", EditorStyles.miniLabel);
            }

        int outOfPlane = 0;
        foreach (var kv in all) if (kv.Key.Z != _sliceZ || kv.Key.W != _sliceW) outOfPlane++;
        if (outOfPlane > 0)
            EditorGUILayout.LabelField(
                $"Клеток вне среза Z={_sliceZ}, W={_sliceW}: {outOfPlane}",
                EditorStyles.miniLabel);
    }

    private string BuildTextGrid(ISequenceEnumerator<Square> e)
    {
        var all = e.PositionsAll;
        var attack = e.PositionsAttackable;
        var sb = new StringBuilder();
        sb.AppendLine($"# {_title}  start=({_start.X},{_start.Y})  Z={_sliceZ} W={_sliceW}");
        for (int dy = _radius; dy >= -_radius; dy--)
        {
            for (int dx = -_radius; dx <= _radius; dx++)
            {
                var cell = new Square { X = _start.X + dx, Y = _start.Y + dy, Z = _sliceZ, W = _sliceW };
                char ch = '.';
                if (all.TryGetValue(cell, out var stay)) ch = stay == Stayable.Stay ? 'o' : ',';
                if (attack.TryGetValue(cell, out var atk) && atk == Attackable.Attack) ch = 'x';
                if (cell.Equals(_start)) ch = 'S';
                sb.Append(ch).Append(' ');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
#endif