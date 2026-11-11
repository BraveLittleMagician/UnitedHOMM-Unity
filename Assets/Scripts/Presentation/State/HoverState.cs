#nullable enable

using System;

public sealed class HoverState
{
    public IHoverable? Hovered { get; private set; }

    public void Set(IHoverable? target)
    {
        if (IsDestroyed(target)) target = null;
        if (IsDestroyed(Hovered)) Hovered = null;
        if (ReferenceEquals(Hovered, target)) return;

        Hovered?.SetHovered(false);
        Hovered = target;
        Hovered?.SetHovered(true);
    }

    public void Clear() => Set(null);

    private static bool IsDestroyed(IHoverable? hoverable)
    {
        if (hoverable == null) return true;
        if (hoverable is Object uo) return uo == null;
        return false;
    }
}