#nullable enable

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldColorsOverride : MonoBehaviour, IButtonColorsSubscriber
{
    [SerializeField] private UIService _uiService = null!;
    private TMP_InputField _inputField = null!;
    private IButtonColorsProvider _provider = null!;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        if (_inputField == null) throw new ArgumentNullException(nameof(_inputField));
        if (_uiService == null) throw new ArgumentNullException(nameof(_uiService));
        _uiService.RegisterSubscriber(this);
    }
    private void OnDestroy()
    {
        _provider?.Unsubscribe(this); 
        _uiService.UnregisterSubscriber(this);
    }
    public void OnButtonColorsChanged(IButtonColors colors)
    {
        _inputField.transition = ToSelectable(colors.Transition);
        ColorBlock b = _inputField.colors;
        b.normalColor = colors.NormalColor;
        b.highlightedColor = colors.HighlightedColor;
        b.pressedColor = colors.PressedColor;
        b.selectedColor = colors.SelectedColor;
        b.disabledColor = colors.DisabledColor;
        b.fadeDuration = colors.FadeDuration;
        _inputField.colors = b;
    }
    public void SetProvider(IButtonColorsProvider provider)
    {
        _provider = provider;
        _provider.Subscribe(this);
    }

    private static Selectable.Transition ToSelectable(TransitionType transition) => transition switch
    {
        TransitionType.None => Selectable.Transition.None,
        TransitionType.Animation => Selectable.Transition.Animation,
        TransitionType.ColorTint => Selectable.Transition.ColorTint,
        TransitionType.SpriteSwap => Selectable.Transition.SpriteSwap,
        _ => Selectable.Transition.None
    };
}