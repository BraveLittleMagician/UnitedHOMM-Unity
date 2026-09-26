#nullable enable

using System;

public interface ISelectionHub
{
    void Select(ISelectable selectable);
    void Deselect(ISelectable selectable);
    void DropSelection();
    event Action<ISelectable?, ISelectable?>? OnSelectionChanged;
}