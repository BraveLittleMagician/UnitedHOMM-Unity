#nullable enable

using UnityEngine;

public interface IButtonColors
{
    TransitionType Transition { get; }
    Color NormalColor { get; }
    Color HighlightedColor { get; }
    Color PressedColor { get; }
    Color SelectedColor { get; }
    Color DisabledColor { get; }
    float FadeDuration { get; }
}