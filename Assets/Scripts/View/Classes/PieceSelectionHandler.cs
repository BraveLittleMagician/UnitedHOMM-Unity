#nullable enable

using System;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(ButtonBehavior))]
public class PieceSelectionHandler : MonoBehaviour
{
    private ButtonBehavior _buttonBehavior = null!;
    private IPiece _piece = null!;
    private IEventBus _eventBus = null!;

    private void Awake()
    {
        _buttonBehavior = GetComponent<ButtonBehavior>();
        if (_buttonBehavior == null) throw new ArgumentNullException(nameof(_buttonBehavior));
        _buttonBehavior.OnSelected += OnButtonSelected;
        _buttonBehavior.OnDeselected += OnButtonDeselected;
    }
    private void OnDestroy()
    {
        _buttonBehavior.OnSelected -= OnButtonSelected;
        _buttonBehavior.OnDeselected -= OnButtonDeselected;
    }
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }
    private void OnButtonSelected()
    {
        if (_piece != null) _eventBus.Publish(new PieceSelectedEvent(_piece));
    }
    private void OnButtonDeselected()
    {

    }

    public void SetPiece(IPiece piece)
    {
        _piece = piece;
    }
}