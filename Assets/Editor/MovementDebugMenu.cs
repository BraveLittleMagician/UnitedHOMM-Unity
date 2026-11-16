#if UNITY_EDITOR
using System.Collections.Immutable;
using UnityEditor;
#endif

public static class MovementDebugMenu
{
    [MenuItem("Debug/Movement/Circle r=3")]
    public static void CircleArea()
    {
        var a = new RelativeArea<Square>(
            MultipleAxes.Three,
            ImmutableHashSet.Create(1, 3),
            length: 3,
            isCircle: true,
            guarantee: false);
        MovementVisualizerWindow.Show("Circle r=3", s => a.GetEnumerator(s));
    }
}