#nullable enable

public interface IHoverable
{
    bool IsHovered { get; }
    void SetHovered(bool value);
}