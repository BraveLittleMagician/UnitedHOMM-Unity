#nullable enable

using System;
using UnityEngine;

public class SelectionHub : MonoBehaviour, ISelectionHub
{
    private ISelectable? _current = null;

    public void Select(ISelectable selectable)
    {
        if (selectable == null || _current == selectable) return;
        var old = _current;
        _current?.OnDeselected();
        _current = selectable;
        _current.OnSelected();
        OnSelectionChanged?.Invoke(old, _current);
    }
    public void Deselect(ISelectable selectable)
    {
        if (_current == selectable)
        {
            _current.OnDeselected();
            OnSelectionChanged?.Invoke(_current, null);
            _current = null;
        }
    }
    public void DropSelection()
    {
        if (_current != null)
        {
            _current?.OnDeselected();
            OnSelectionChanged?.Invoke(_current, null);
            _current = null;
        }
    }

    public event Action<ISelectable?, ISelectable?>? OnSelectionChanged;
}
