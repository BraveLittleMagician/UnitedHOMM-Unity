#nullable enable

using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private IEventBus _eventBus = null!;
    private IPiece _piece = null!;
    private Square _position;

    private bool _isHovered;
    private bool _isSelected;

    public BigInteger IndexInHouse => _piece.IndexInHouse;

    public void Initialize(IEventBus eventBus, IPiece piece, Square position)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _position = position;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_eventBus == null) return;

        _eventBus.Publish(new PieceSelectedEvent<Square>(_piece, _position));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateVisual();
    }

    public void UpdatePosition(Square newPosition)
    {
        _position = newPosition;
    }

    private void UpdateVisual()
    {
        if (!TryGetComponent<InfoOfPiece>(out var info)) return;
        if (info.Block == null) return;

        PieceColorizer.SetVisualState(info, _isSelected, highlighted: false, hovered: _isHovered);
    }
}