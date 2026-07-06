#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickEventer : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _action = null!;
    private InputAction _click = null!;

    private void Awake()
    {
        if (_action == null) throw new NullReferenceException(nameof(_action));
        _click = _action.FindAction("UI/Click") ?? throw new NullReferenceException(nameof(_click));
        _click.Enable();
        _click.performed += OnClickEvent;
    }

    private void OnClickEvent(InputAction.CallbackContext ctx) => GlobalDeselector.Instance.TriggerGlobalClick();

    private void OnDestroy() => _click.Disable();
}