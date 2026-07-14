#nullable enable

using UnityEngine;

public class ButtonColors : MonoBehaviour
{
    [field: SerializeField] public TransitionType Transition { get; set; } = TransitionType.ColorTint;
    [field: SerializeField] public Color NormalColor { get; set; } = new Color(1f, 1f, 1f, 1f);
    [field: SerializeField] public Color HighlightedColor { get; set; } = new Color(1f, 0.5254902f, 0.5254902f, 1f);
    [field: SerializeField] public Color PressedColor { get; set; } = new Color(0.7843137f, 0.7843137f, 0.7843137f, 1f);
    [field: SerializeField] public Color SelectedColor { get; set; } = new Color(0.2705882f, 0.8117647f, 1f, 1f);
    [field: SerializeField] public Color DisabledColor { get; set; } = new Color(0.282353f, 0.282353f, 0.282353f, 0.4980392f);
    [field: SerializeField] public float FadeDuration { get; set; } = 0.1f;

    public enum TransitionType
    {
        None,
        ColorTint,
        SpriteSwap,
        Animation
    }
}