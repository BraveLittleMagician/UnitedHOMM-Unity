#nullable enable

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

[RequireComponent(typeof(Renderer))]
public class ButtonBehavior : MonoBehaviour, IGlobalDeselectSubscriber
{
    private bool _isHovered = false;
    private bool _isPressed = false;
    private bool _isSelected = false;
    private Renderer _rend = null!;
    private Coroutine? _colorCoroutine;
    private IButtonColors _colors = null!;
    private IGlobalDeselector _deselector = null!;
    private bool _initialized = false;

    [field: SerializeField] public bool Interactable { get; private set; } = true;
    [SerializeField] private UnityEvent _onClick = new();

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        if (_rend == null)
        {
            enabled = false;
            throw new ArgumentNullException(nameof(_rend));
        }
    }
    private void OnDestroy()
    {
        if (_colorCoroutine != null)
            StopCoroutine(_colorCoroutine);
    }

    private void UpdateVisualState()
    {
        if (!_initialized) return;
        Color target = GetCurrentStateColor();
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(FadeToColor(target, _colors.FadeDuration));
    }
    private Color GetCurrentStateColor()
    {
        if (!Interactable) return _colors.DisabledColor;
        if (_isPressed) return _colors.PressedColor;
        if (_isSelected) return _colors.SelectedColor;
        if (_isHovered) return _colors.HighlightedColor;
        return _colors.NormalColor;
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

    [Inject]
    public void Construct(IButtonColors colors, IGlobalDeselector deselector)
    {
        _colors = colors ?? throw new ArgumentNullException(nameof(colors));
        _deselector = deselector ?? throw new ArgumentNullException(nameof(deselector));
        _initialized = true;
        UpdateVisualState();
    }

    public void Interact(bool val)
    {
        Interactable = val;
        if (!Interactable)
        {
            _isHovered = false;
            _isPressed = false;
        }
    }
    public void OnPointerEnter()
    {
        if (!_initialized || !Interactable) return;
        _isHovered = true;
        UpdateVisualState();
    }
    public void OnPointerExit()
    {
        _isHovered = false;
        UpdateVisualState();
        _deselector.RegisterSubscriber(this);
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
        _deselector.UnregisterSubscriber(this);
        _isSelected = false;
        UpdateVisualState();
    }
}