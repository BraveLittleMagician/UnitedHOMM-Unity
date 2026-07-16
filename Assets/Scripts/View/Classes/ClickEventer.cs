#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class ClickEventer : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _action = null!;
    private InputAction _click = null!;
    private IGlobalDeselector _deselector = null!;

    private void Awake()
    {
        if (_action == null) throw new ArgumentNullException(nameof(_action));
        _click = _action.FindAction("UI/Click") ?? throw new ArgumentNullException(nameof(_click));
        _click.Enable();
        _click.performed += OnClickEvent;
    }
    private void OnDestroy()
    {
        if (_click != null)
        {
            _click.performed -= OnClickEvent;
            _click.Disable();
        }
    }
    private void OnClickEvent(InputAction.CallbackContext ctx)
    {
        _deselector?.TriggerGlobalClick();
    }

    [Inject]
    public void Construct(IGlobalDeselector deselector)
    {
        _deselector = deselector ?? throw new ArgumentNullException(nameof(deselector));
    }
}