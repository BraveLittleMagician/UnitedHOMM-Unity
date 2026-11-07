#nullable enable

using System;

[Flags]
public enum ShapeTarget
{
    None = 0,
    Movement = 1,
    Attack = 2,
    Both = Movement | Attack,
}