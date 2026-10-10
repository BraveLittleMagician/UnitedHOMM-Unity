#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviorEvents : MonoBehaviour, ISelectable, IHoverable, IPressable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private ISelectionHub _selectionHub = null!;
    private UIPointerProbe _uiPointerProbe = null!;
    private CameraWork _cameraWork = null!;
    private InfoOfPiece _info = null!;

    [field: SerializeField] public bool Interactable { get; private set; } = true;

    public bool IsSelected { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    private void OnDestroy()
    {
        if (_cameraWork != null) _cameraWork.ManipulationChanged -= OnManipulationChanged;
    }

    private void UpdateVisualState()
    {
        if (_info == null) return;
        if (_info.Block == null) return;

        PieceColorizer.SetVisualState(_info, IsSelected, highlighted: false, hovered: IsHovered);
    }
    private void OnManipulationChanged(bool isManipulating)
    {
        if (isManipulating && IsHovered) ForceHoverExit();
    }
    private void ForceHoverExit()
    {
        IsHovered = false;
        UpdateVisualState();
        Dehovered?.Invoke();
    }

    public void OnSelected()
    {
        if (IsSelected) return;
        IsSelected = true;
        UpdateVisualState();
        Selected?.Invoke();
    }
    public void OnDeselected()
    {
        if (!IsSelected) return;
        IsSelected = false;
        UpdateVisualState();
        Deselected?.Invoke();
    }
    public void OnHoverEnter()
    {
        if (!Interactable || IsHovered) return;
        IsHovered = true;
        UpdateVisualState();
        Hovered?.Invoke();
    }
    public void OnHoverExit()
    {
        if (!IsHovered) return;
        IsHovered = false;
        UpdateVisualState();
        Dehovered?.Invoke();
    }
    public void OnPress()
    {
        if (!Interactable || IsPressed) return;
        IsPressed = true;
        Pressed?.Invoke();
    }

    public void OnRelease()
    {
        if (!IsPressed) return;
        IsPressed = false;
        Depressed?.Invoke();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;
        if (_cameraWork.IsManipulating) return;
        if (_uiPointerProbe.IsPointerOverUI(eventData.position)) return;
        _selectionHub.Select(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cameraWork.IsManipulating) return;
        if (_uiPointerProbe.IsPointerOverUI(eventData.position)) return;
        OnHoverEnter();
    }
    public void OnPointerExit(PointerEventData _) => OnHoverExit();
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_cameraWork.IsManipulating) return;
        if (_uiPointerProbe.IsPointerOverUI(eventData.position)) return;
        OnPress();
    }
    public void OnPointerUp(PointerEventData _) => OnRelease();
    public void Initialize(ISelectionHub selectionHub, UIPointerProbe uiPointerProbe, CameraWork cameraWork)
    {
        _selectionHub = selectionHub ?? throw new ArgumentNullException(nameof(selectionHub));
        _uiPointerProbe = uiPointerProbe != null ? uiPointerProbe : throw new ArgumentNullException(nameof(uiPointerProbe));
        _cameraWork = cameraWork != null ? cameraWork : throw new ArgumentNullException(nameof(cameraWork));
        if (!TryGetComponent(out _info!))
            throw new InvalidOperationException($"На объекте '{name}' отсутствует InfoOfPiece. ButtonBehaviorEvents требует его для управления визуалом.");

        _cameraWork.ManipulationChanged += OnManipulationChanged;
    }
    public void Interact(bool val) => Interactable = val;


    public event Action? Selected;
    public event Action? Deselected;
    public event Action? Hovered;
    public event Action? Dehovered;
    public event Action? Pressed;
    public event Action? Depressed;
}