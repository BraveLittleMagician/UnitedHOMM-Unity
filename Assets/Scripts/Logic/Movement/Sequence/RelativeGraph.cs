#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

public sealed class RelativeGraph<TSquare> : ISequence<TSquare, RelativeGraph<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    private readonly Dictionary<LineKey, List<SegmentNode<TSquare>>> _lineIndex = new();
    private int _totalStayablesCount;
    private static readonly int[] _fullRange = { -1, 0, 1 };
    private static readonly int[] _zeroRange = { 0 };

    public RelativeGraph() : this(true) { }
    public RelativeGraph(bool guaranteesAtLeastOneStayable)
    {
        GuaranteesAtLeastOneStayable = guaranteesAtLeastOneStayable;
        if (guaranteesAtLeastOneStayable)
        {
            var zeroNode = new SegmentNode<TSquare>(new TSquare(), new Stayables(1, new[] { 1 }));
            AddToIndex(zeroNode);
            _totalStayablesCount = 1;
            Count = 1;
            OriginNode = zeroNode;
        }
        else
        {
            var zeroNode = new SegmentNode<TSquare>(new TSquare(), new Stayables(0));
            AddToIndex(zeroNode);
            _totalStayablesCount = 0;
            Count = 1;
            OriginNode = zeroNode;
        }
    }

    public int Count { get; private set; }
    public bool GuaranteesAtLeastOneStayable { get; private set; }
    public SegmentNode<TSquare> OriginNode { get; private set; }
    public IEnumerable<SegmentNode<TSquare>> Nodes => _lineIndex.Values.SelectMany(list => list);

    private void AddToIndex(SegmentNode<TSquare> node)
    {
        var key = node.Position.GetLineKey();
        if (!_lineIndex.TryGetValue(key, out var list))
            _lineIndex[key] = list = new List<SegmentNode<TSquare>>();
        list.Add(node);
        list.Sort((a, b) => a.Position.X.CompareTo(b.Position.X));
        Count++;
    }
    private void RemoveFromIndex(SegmentNode<TSquare> node)
    {
        var key = node.Position.GetLineKey();
        if (_lineIndex.TryGetValue(key, out var list) && list.Remove(node))
        {
            Count--;
            if (list.Count == 0)
                _lineIndex.Remove(key);
        }
    }
    private void RemoveOutdatedNodes(List<SegmentNode<TSquare>> overlap)
    {
        foreach (var node in overlap)
        {
            _totalStayablesCount -= node.Stayables.Count;
            RemoveConnections(node);
            RemoveFromIndex(node);
        }
    }
    private void ReconnectNode(SegmentNode<TSquare> node)
    {
        foreach (var neighbor in node.Neighbors.ToList())
            neighbor.Neighbors.Remove(node);
        node.Neighbors.Clear();

        var candidates = new HashSet<SegmentNode<TSquare>>();

        var sameKey = node.Position.GetLineKey();
        if (_lineIndex.TryGetValue(sameKey, out var sameLineNodes))
            candidates.UnionWith(sameLineNodes);

        var neighborKeys = RelativeGraph<TSquare>.GetNeighborLineKeys(node.Position);
        foreach (var key in neighborKeys)
            if (_lineIndex.TryGetValue(key, out var nodes))
                candidates.UnionWith(nodes);

        foreach (var other in candidates)
        {
            if (other == node) continue;
            if (other.Position.IsNeighborByPosition(node.Position) && other.TouchesX(node.Position.X, node.LastRelativeIndex))
            {
                node.Neighbors.Add(other);
                other.Neighbors.Add(node);
            }
        }
    }
    private bool Contains(SegmentNode<TSquare> node)
    {
        var key = node.Position.GetLineKey();
        return _lineIndex.TryGetValue(key, out var list) && list.Contains(node);
    }
    private bool Add(TSquare square, Stayables stayables)
    {
        var newNode = new SegmentNode<TSquare>(square, stayables);
        if (newNode.Stayables.Count == 0 && !newNode.Position.IsZeroLine() && _totalStayablesCount == 0)
            return false;

        var (overlapping, adjacent) = FindOverlappingAndAdjacent(newNode);
        if (adjacent.Count == 0 && overlapping.Count == 0 && Count > 0)
            return false;

        if (overlapping.Count == 0)
        {
            AddToIndex(newNode);
            _totalStayablesCount += newNode.Stayables.Count;
            ReconnectNode(newNode);
            return true;
        }

        var (minX, maxX, mergedIndices) = MergeOverlappingNodes(newNode, overlapping);
        RemoveOutdatedNodes(overlapping);
        _totalStayablesCount += mergedIndices.Count;

        bool containsZeroInMerged = (minX <= 0 && maxX >= 0) &&
            (newNode.Position.IsZeroLine() || overlapping.Any(n => n.Position.IsZeroLine()));

        var merged = new SegmentNode<TSquare>(square.WithValue(Axis.X, minX), new Stayables(maxX - minX, mergedIndices.OrderBy(i => i).ToList()));
        if (containsZeroInMerged) OriginNode = merged;
        AddToIndex(merged);
        ReconnectNode(merged);
        return true;
    }
    private bool Remove(SegmentNode<TSquare> node)
    {
        if (GuaranteesAtLeastOneStayable && node.Position.X <= 0 && node.XMax >= 0 && node.Position.IsZeroLine()) return false;
        if (!Contains(node)) return false;
        _totalStayablesCount -= node.Stayables.Count;
        RemoveConnections(node);
        RemoveFromIndex(node);
        return true;
    }
    private bool TryToGet(TSquare square, [NotNullWhen(true)] out SegmentNode<TSquare>? node)
    {
        node = Nodes.FirstOrDefault(n => n.Position.IsSameLine(square) && n.Position.X <= square.X && square.X <= n.XMax);
        return node != null;
    }
    private RelativeGraph<TSquare> PrivateCopy()
    {
        var copy = new RelativeGraph<TSquare>();
        copy._lineIndex.Clear();
        copy.Count = 0;
        copy._totalStayablesCount = 0;

        var nodeMap = new Dictionary<SegmentNode<TSquare>, SegmentNode<TSquare>>(ReferenceEqualityComparer<SegmentNode<TSquare>>.Instance);

        foreach (var node in Nodes)
        {
            var newNode = new SegmentNode<TSquare>(node.Position, new Stayables(node.Stayables.LastRelativeIndex, node.Stayables.All));
            nodeMap[node] = newNode;
            copy.AddToIndex(newNode);
            copy._totalStayablesCount += newNode.Stayables.Count;
        }

        foreach (var (original, newCopy) in nodeMap)
        {
            foreach (var origNeighbor in original.Neighbors)
            {
                if (nodeMap.TryGetValue(origNeighbor, out var newNeighbor))
                    newCopy.Neighbors.Add(newNeighbor);
            }
        }

        copy.OriginNode = nodeMap.TryGetValue(OriginNode, out var newOrigin)
            ? newOrigin
            : throw new InvalidOperationException("Не удалось скопировать OriginNode");

        foreach (var list in copy._lineIndex.Values)
            list.Sort((a, b) => a.Position.X.CompareTo(b.Position.X));

        return copy;
    }
    private Dictionary<SegmentNode<TSquare>, SegmentNode<TSquare>> CreateAbsoluteNodes(TSquare startSquare)
    {
        var map = new Dictionary<SegmentNode<TSquare>, SegmentNode<TSquare>>(ReferenceEqualityComparer<SegmentNode<TSquare>>.Instance);

        foreach (var node in Nodes)
        {
            var absolutePos = startSquare.Add(node.Position);
            var absoluteNode = new SegmentNode<TSquare>(absolutePos, node.Stayables);
            map[node] = absoluteNode;
        }

        foreach (var (original, absolute) in map)
            foreach (var originalNeighbor in original.Neighbors)
                if (map.TryGetValue(originalNeighbor, out var absoluteNeighbor))
                    absolute.Neighbors.Add(absoluteNeighbor);
        return map;
    }
    private Dictionary<LineKey, List<SegmentNode<TSquare>>> GetNodesFromPosition(TSquare startSquare)
    {
        var absoluteNodesMap = CreateAbsoluteNodes(startSquare);
        var result = new Dictionary<LineKey, List<SegmentNode<TSquare>>>();

        foreach (var (original, absolute) in absoluteNodesMap)
        {
            var key = original.Position.GetLineKey().Shift(startSquare);
            if (!result.TryGetValue(key, out var list))
                result[key] = list = new List<SegmentNode<TSquare>>();
            list.Add(absolute);
        }
        return result;
    }
    private (List<SegmentNode<TSquare>> overlapping, List<SegmentNode<TSquare>> adjacent) FindOverlappingAndAdjacent(SegmentNode<TSquare> newNode)
    {
        var overlapping = new List<SegmentNode<TSquare>>();
        var adjacent = new List<SegmentNode<TSquare>>();

        var sameKey = newNode.Position.GetLineKey();
        if (_lineIndex.TryGetValue(sameKey, out var sameLineNodes))
        {
            foreach (var node in sameLineNodes)
                if (node.TouchesX(newNode.Position.X, newNode.LastRelativeIndex))
                    overlapping.Add(node);
        }

        var neighborKeys = RelativeGraph<TSquare>.GetNeighborLineKeys(newNode.Position);
        foreach (var key in neighborKeys)
            if (_lineIndex.TryGetValue(key, out var nodes))
                foreach (var node in nodes)
                    if (node.TouchesX(newNode.Position.X, newNode.LastRelativeIndex))
                        adjacent.Add(node);

        return (overlapping, adjacent);
    }

    private static void RemoveConnections(SegmentNode<TSquare> node)
    {
        foreach (var neighbor in node.Neighbors.ToList())
        {
            neighbor.Neighbors.Remove(node);
            node.Neighbors.Remove(neighbor);
        }
    }
    private static List<LineKey> GetNeighborLineKeys(TSquare position)
    {
        var axes = position.ActiveAxes;

        var dyRange = axes.HasFlag(MultipleAxes.Two) ? _fullRange : _zeroRange;
        var dzRange = axes.HasFlag(MultipleAxes.Three) ? _fullRange : _zeroRange;
        var dwRange = axes.HasFlag(MultipleAxes.Four) ? _fullRange : _zeroRange;

        var neighborKeys = new List<LineKey>(26);

        foreach (int dy in dyRange)
            foreach (int dz in dzRange)
                foreach (int dw in dwRange)
                {
                    if (dy == 0 && dz == 0 && dw == 0) continue;
                    neighborKeys.Add(position.WithOffset(0, dy, dz, dw).GetLineKey());
                }

        return neighborKeys;
    }
    private static HashSet<int> MergeStayables(IEnumerable<SegmentNode<TSquare>> nodes, int newMinX)
    {
        var result = new HashSet<int>();
        foreach (var node in nodes)
        {
            int offset = node.Position.X - newMinX;
            foreach (int idx in node.Stayables.All)
                result.Add(idx + offset);
        }
        return result;
    }
    private static SegmentNode<TSquare> CreateSubNode(SegmentNode<TSquare> source, int startOffset, int endOffset)
    {
        int newLength = endOffset - startOffset;
        var newStayables = new Stayables(newLength);
        for (int i = startOffset; i <= endOffset; i++)
        {
            if (source.Stayables.Contains(i))
                newStayables.AddStayable(i - startOffset);
        }

        var newSquare = source.Position.WithValue(Axis.X, source.Position.X + startOffset);
        return new SegmentNode<TSquare>(newSquare, newStayables);
    }
    private static (int minX, int maxX, HashSet<int> mergedIndices) MergeOverlappingNodes(SegmentNode<TSquare> newNode, List<SegmentNode<TSquare>> overlapping)
    {
        int minX = newNode.Position.X;
        int maxX = newNode.XMax;
        var nodesToMerge = new List<SegmentNode<TSquare>> { newNode };
        nodesToMerge.AddRange(overlapping);
        foreach (var node in overlapping)
        {
            if (node.Position.X < minX) minX = node.Position.X;
            if (node.XMax > maxX) maxX = node.XMax;
        }
        var mergedIndices = MergeStayables(nodesToMerge, minX);
        return (minX, maxX, mergedIndices);
    }
    private static void AppendNeighbor(StringBuilder sb, SegmentNode<TSquare> node)
    {
        sb.Append('[');

        bool first = true;
        foreach (var pair in node.Position.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (!first) sb.Append(", ");
            first = false;
            sb.Append(pair.Key).Append('=').Append(pair.Value);
        }

        if (!first) sb.Append(", ");
        sb.Append("X=").Append(node.Position.X).Append("..").Append(node.XMax).Append(']');
    }

    public bool Add(TSquare square) => Add(square, 0);
    public bool Add(TSquare square, int length) => Add(square, length, Array.Empty<int>());
    public bool Add(TSquare square, int length, IEnumerable<int> ints)
    {
        if (length < 0) return false;
        return Add(square, new Stayables(length, ints));
    }
    public bool SetStayable(TSquare square, Stayable stayable)
    {
        if (!TryToGet(square, out var node)) return false;
        int offset = square.X - node.Position.X;
        if (offset < 0 || offset > node.Stayables.LastRelativeIndex) return false;
        if (stayable == Stayable.Stay)
        {
            if (node.Stayables.AddStayable(offset))
            {
                _totalStayablesCount++;
                return true;
            }
            return false;
        }
        else
        {
            if (_totalStayablesCount == 0 || (GuaranteesAtLeastOneStayable && _totalStayablesCount <= 1)) return false;
            if (node.Stayables.RemoveStayable(offset))
            {
                _totalStayablesCount--;
                return true;
            }
            return false;
        }
    }
    public bool Remove(TSquare square)
    {
        if (!TryToGet(square, out var node)) return false;

        int offset = square.X - node.Position.X;
        if (node.LastRelativeIndex == 0) return Remove(node);

        var leftNode = (offset > 0) ? RelativeGraph<TSquare>.CreateSubNode(node, 0, offset - 1) : null;
        var rightNode = (offset < node.LastRelativeIndex) ? RelativeGraph<TSquare>.CreateSubNode(node, offset + 1, node.LastRelativeIndex) : null;

        if (!Remove(node)) return false;

        if (leftNode != null && !Add(leftNode.Position, leftNode.Stayables)) throw new Exception($"Элемент {leftNode} должен суметь добавиться");
        if (rightNode != null && !Add(rightNode.Position, rightNode.Stayables)) throw new Exception($"Элемент {rightNode} должен суметь добавиться");

        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append("Count: ").Append(Count)
          .Append(", TotalStayables: ").Append(_totalStayablesCount)
          .AppendLine();

        var sortedLines = _lineIndex
            .OrderBy(kvp => kvp.Key.Y)
            .ThenBy(kvp => kvp.Key.Z)
            .ThenBy(kvp => kvp.Key.W);

        foreach (var line in sortedLines)
        {
            sb.Append("Line ").Append(line.Key).Append(':').AppendLine();

            var sortedNodes = line.Value.OrderBy(n => n.Position.X);
            foreach (var node in sortedNodes)
            {
                sb.Append("  PatternNode: ").Append(node.ToString()).AppendLine();
                sb.Append("    Neighbors: ");

                bool first = true;
                foreach (var neighbor in node.Neighbors)
                {
                    if (!first) sb.Append(", ");
                    first = false;
                    AppendNeighbor(sb, neighbor);
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public ISequenceEnumerator<TSquare> GetEnumerator(TSquare start) => new Enumerable(this, start);

    public RelativeGraph<TSquare> Copy() => PrivateCopy();
    ISequence ICopyable<ISequence>.Copy() => PrivateCopy();

    private class Enumerable : SequenceEnumerable<TSquare>
    {
        private readonly Dictionary<LineKey, List<SegmentNode<TSquare>>> _lineIndex;
        private Dictionary<TSquare, Stayable>? _allPositionsCache;
        private List<TSquare>? _onlyStayablesCache;

        public Enumerable(RelativeGraph<TSquare> graph, TSquare startSquare) : base(startSquare)
        {
            _lineIndex = graph.GetNodesFromPosition(startSquare);
        }

        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;

                var result = new Dictionary<TSquare, Stayable>
                {
                    [StartPosition] = Stayable.NotStay
                };

                var startNode = FindNodeContainingSquare(StartPosition);

                if (startNode == null)
                {
                    _allPositionsCache = result;
                    return result;
                }

                var visitedNodes = new HashSet<SegmentNode<TSquare>>();
                var queue = new Queue<SegmentNode<TSquare>>();
                queue.Enqueue(startNode);
                visitedNodes.Add(startNode);

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    for (int x = node.Position.X; x <= node.XMax; x++)
                    {
                        var square = node.Position.WithValue(Axis.X, x);
                        if (!square.Equals(StartPosition))
                        {
                            int offset = x - node.Position.X;
                            bool isStayable = node.Stayables.Contains(offset);
                            result[square] = isStayable ? Stayable.Stay : Stayable.NotStay;
                        }
                    }
                    foreach (var neighbor in node.Neighbors)
                    {
                        if (!visitedNodes.Contains(neighbor))
                        {
                            visitedNodes.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                _allPositionsCache = result;
                return result;
            }
        }
        protected override List<TSquare> CreateOnlyStayablesPositions
        {
            get
            {
                if (_onlyStayablesCache != null) return _onlyStayablesCache;
                _onlyStayablesCache = CreateAllPossiblePositions
                    .Where(kvp => kvp.Value == Stayable.Stay)
                    .Select(kvp => kvp.Key)
                    .ToList();
                return _onlyStayablesCache;
            }
        }

        private SegmentNode<TSquare>? FindNodeContainingSquare(TSquare square)
        {
            if (!_lineIndex.TryGetValue(square.GetLineKey(), out var candidates))
                return null;

            foreach (var node in candidates)
            {
                if (node.Position.X <= square.X && square.X <= node.XMax)
                    return node;
            }
            return null;
        }

        private static bool IsInsideSameNode(SegmentNode<TSquare> node, TSquare from, TSquare to)
        {
            if (!node.Position.IsSameLine(to)) return false;
            if (node.Position.X > to.X || to.X > node.XMax) return false;
            return Math.Abs(to.X - from.X) == 1 && from.IsSameLine(to);
        }
        private static SegmentNode<TSquare>? FindNeighborNodeForTransition(SegmentNode<TSquare> currentNode, TSquare from, TSquare to)
        {
            if (!from.IsAdjacent(to)) return null;

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (neighbor.Position.IsSameLine(to) &&
                    neighbor.Position.X <= to.X && to.X <= neighbor.XMax)
                    return neighbor;
            }
            return null;
        }
        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            var steps = path.Positions;
            if (steps.Count < 2) return false;

            var currentNode = FindNodeContainingSquare(steps[0]);
            if (currentNode == null) return false;

            for (int i = 0; i < steps.Count - 1; i++)
            {
                var from = steps[i];
                var to = steps[i + 1];
                bool stepValid = false;

                if (IsInsideSameNode(currentNode, from, to))
                    stepValid = true;
                else
                {
                    var nextNode = FindNeighborNodeForTransition(currentNode, from, to);
                    if (nextNode != null)
                    {
                        stepValid = true;
                        currentNode = nextNode;
                    }
                }

                if (!stepValid) return false;
            }

            var lastSquare = steps[^1];
            var lastNode = FindNodeContainingSquare(lastSquare);
            if (lastNode == null) return false;

            int offset = lastSquare.X - lastNode.Position.X;
            return lastNode.Stayables.Contains(offset);
        }
        public override bool CanMoveTo(TSquare target)
        {
            return CreateAllPossiblePositions.TryGetValue(target, out var stayable) && stayable == Stayable.Stay;
        }
    }
}