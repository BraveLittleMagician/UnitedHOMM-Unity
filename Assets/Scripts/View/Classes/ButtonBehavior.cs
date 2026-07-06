#nullable enable

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class ButtonBehavior : MonoBehaviour, IGlobalDeselectSubscriber
{
    private bool _isHovered = false;
    private bool _isPressed = false;
    private bool _isSelected = false;
    private Renderer _rend = null!;
    private Coroutine? _colorCoroutine;
    
    [field: SerializeField] public bool Interactable { get; private set; } = true;
    [field: SerializeField] public TransitionType Transition { get; set; } = TransitionType.ColorTint;
    [field: SerializeField] public Color NormalColor { get; set; } = new Color(1f, 1f, 1f, 1f);
    [field: SerializeField] public Color HighlightedColor { get; set; } = new Color(1f, 0.5254902f, 0.5254902f, 1f);
    [field: SerializeField] public Color PressedColor { get; set; } = new Color(0.7843137f, 0.7843137f, 0.7843137f, 1f);
    [field: SerializeField] public Color SelectedColor { get; set; } = new Color(0.2705882f, 0.8117647f, 1f, 1f);
    [field: SerializeField] public Color DisabledColor { get; set; } = new Color(0.282353f, 0.282353f, 0.282353f, 0.4980392f);
    [field: SerializeField] public float FadeDuration { get; set; } = 0.1f;
    [SerializeField] private UnityEvent _onClick = new();

    public void Interact(bool val)
    {
        Interactable = val;
        if (!Interactable)
        {
            _isHovered = false;
            _isPressed = false;
        }
    }

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        if (_rend == null)
        {
            enabled = false;
            throw new NullReferenceException(nameof(_rend));
        }
        _rend.material.color = GetCurrentStateColor();
    }
    private void OnDestroy()
    {
        if (_colorCoroutine != null)
            StopCoroutine(_colorCoroutine);
    }

    private void UpdateVisualState()
    {
        Color target = GetCurrentStateColor();
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(FadeToColor(target, FadeDuration));
    }
    private Color GetCurrentStateColor()
    {
        if (!Interactable) return DisabledColor;
        if (_isPressed) return PressedColor;
        if (_isSelected) return SelectedColor;
        if (_isHovered) return HighlightedColor;
        return NormalColor;
    }
    private IEnumerator FadeToColor(Color target, float duration)
    {
        if (_rend == null) yield break;
        Color start = _rend.material.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _rend.material.color = Color.Lerp(start, target, t);
            yield return null;
        }
        _rend.material.color = target;
        _colorCoroutine = null;
    }

    public void OnPointerEnter()
    {
        if (!Interactable) return;
        _isHovered = true;
        UpdateVisualState();
    }
    public void OnPointerExit()
    {
        _isHovered = false;
        UpdateVisualState(); 
        if (GlobalDeselector.Instance != null)
            GlobalDeselector.Instance.RegisterSubscriber(this);
    }
    public void OnPointerDown()
    {
        if (!Interactable) return;
        _isPressed = true;
        UpdateVisualState();
    }
    public void OnPointerUp()
    {
        _isPressed = false;
        UpdateVisualState();
    }
    public void OnPointerClick()
    {
        if (!Interactable) return;
        Select();
        _onClick.Invoke();
    }

    public void Select()
    {
        _isSelected = true;
        UpdateVisualState();
    }
    public void Deselect()
    {
        GlobalDeselector.Instance.UnregisterSubscriber(this);
        _isSelected = false;
        UpdateVisualState();
    }

    public enum TransitionType
    {
        None,
        ColorTint,
        SpriteSwap,
        Animation
    }
}