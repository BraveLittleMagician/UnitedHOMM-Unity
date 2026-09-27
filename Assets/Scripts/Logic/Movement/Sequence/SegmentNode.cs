#nullable enable

using System.Collections.Generic;
using System.Text;

public class SegmentNode<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public SegmentNode(TSquare square) : this(square, new Stayables(0)) { }
    public SegmentNode(TSquare square, Stayables stayables)
    {
        Position = square;
        Stayables = stayables;
    }

    public int LastRelativeIndex => Stayables.LastRelativeIndex;
    public int XMax => Position.X + LastRelativeIndex;
    public TSquare Position { get; }
    public Stayables Stayables { get; }
    public HashSet<SegmentNode<TSquare>> Neighbors { get; } = new(ReferenceEqualityComparer<SegmentNode<TSquare>>.Instance);

    private static bool IntersectsX((int XMin, int XMax) current, (int XMin, int XMax) other) => !(current.XMax < other.XMin || other.XMax < current.XMin);
    private static bool TouchesX((int XMin, int XMax) current, (int XMin, int XMax) other) => !(current.XMax < (other.XMin - 1) || other.XMax < (current.XMin - 1));

    public bool TouchesX(int x, int length) => TouchesX((Position.X, XMax), (x, x + length));
    public bool IntersectsX(int x, int length) => IntersectsX((Position.X, XMax), (x, x + length));

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append("[X=").Append(Position.X).Append("..").Append(XMax);

        foreach (var pair in Position.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            sb.Append(", ").Append(pair.Key).Append('=').Append(pair.Value);
        }

        sb.Append(']');
        sb.Append(' ').Append(Neighbors.Count).Append(" N");

        if (Stayables.Count > 0)
        {
            sb.Append('(');
            bool first = true;
            foreach (int v in Stayables.All)
            {
                if (!first) sb.Append(' ');
                first = false;
                sb.Append(v);
            }
            sb.Append(')');
        }

        return sb.ToString();
    }
}