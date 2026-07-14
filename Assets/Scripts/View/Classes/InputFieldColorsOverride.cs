#nullable enable

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldColorsOverride : MonoBehaviour
{
    private TMP_InputField _inputField = null!;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>(); if (_inputField == null) throw new NullReferenceException(nameof(_inputField));
        _inputField.transition = ButtonColors.Instance.Transition.ToSelectable();
        ColorBlock b = _inputField.colors;
        b.normalColor = ButtonColors.Instance.NormalColor;
        b.highlightedColor = ButtonColors.Instance.HighlightedColor;
        b.pressedColor = ButtonColors.Instance.PressedColor;
        b.selectedColor = ButtonColors.Instance.SelectedColor;
        b.disabledColor = ButtonColors.Instance.DisabledColor;
        b.fadeDuration = ButtonColors.Instance.FadeDuration;
        _inputField.colors = b;
    }
}