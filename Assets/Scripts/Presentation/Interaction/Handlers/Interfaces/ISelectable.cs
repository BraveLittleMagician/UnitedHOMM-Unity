#nullable enable

public interface ISelectable
{
    bool IsSelected { get; }
    void OnSelected();
    void OnDeselected();
}