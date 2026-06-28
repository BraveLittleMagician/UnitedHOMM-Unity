#nullable enable


public enum MultipleAxes
{
    None = 0,
    One = Axis.X,
    Two = Axis.X | Axis.Y,
    Three = Axis.X | Axis.Y | Axis.Z,
    Four = Axis.X | Axis.Y | Axis.Z | Axis.W,
}