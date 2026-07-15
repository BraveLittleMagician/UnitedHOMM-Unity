#nullable enable

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldColorsOverride : MonoBehaviour
{
    private TMP_InputField _inputField = null!;
    private IButtonColors _colors = null!;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        if (_inputField == null) throw new ArgumentNullException(nameof(_inputField));
    }
    private void Start()
    {
        if (_colors == null) throw new ArgumentNullException(nameof(_colors));
        ApplyColors();
    }
    private void ApplyColors()
    {
        _inputField.transition = ToSelectable(_colors.Transition);
        ColorBlock b = _inputField.colors;
        b.normalColor = _colors.NormalColor;
        b.highlightedColor = _colors.HighlightedColor;
        b.pressedColor = _colors.PressedColor;
        b.selectedColor = _colors.SelectedColor;
        b.disabledColor = _colors.DisabledColor;
        b.fadeDuration = _colors.FadeDuration;
        _inputField.colors = b;
    }

    [Inject]
    public void Construct(IButtonColors colors) => _colors = colors;

    private static Selectable.Transition ToSelectable(TransitionType transition) => transition switch
    {
        TransitionType.None => Selectable.Transition.None,
        TransitionType.Animation => Selectable.Transition.Animation,
        TransitionType.ColorTint => Selectable.Transition.ColorTint,
        TransitionType.SpriteSwap => Selectable.Transition.SpriteSwap,
        _ => Selectable.Transition.None
    };
}