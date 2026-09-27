#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

public class RelativeGraph<TSquare> : ISequence<Square, RelativeGraph<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    private readonly Dictionary<LineKey, List<SegmentNode<TSquare>>> _lineIndex = new();
    private int _totalStayablesCount;

    public RelativeGraph() : this (true) { }
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
        var key = GetLineKey(node.Position);
        if (!_lineIndex.TryGetValue(key, out var list))
            _lineIndex[key] = list = new List<SegmentNode<TSquare>>();
        list.Add(node);
        list.Sort((a, b) => a.Position.X.CompareTo(b.Position.X));
        Count++;
    }
    private void RemoveFromIndex(SegmentNode<TSquare> node)
    {
        var key = GetLineKey(node.Position);
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

        var sameKey = GetLineKey(node.Position);
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
        var key = GetLineKey(node.Position);
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

        var dict = new Dictionary<Axis, int>(square.Coordinates) { [Axis.X] = minX };
        var merged = new SegmentNode<TSquare>(square.CopyWith(dict), new Stayables(maxX - minX, mergedIndices.OrderBy(i => i).ToList()));
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

        var nodeMap = new Dictionary<SegmentNode<TSquare>, SegmentNode<TSquare>>();

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
    private Dictionary<SegmentNode<TSquare>, SegmentNode<Square>> CreateAbsoluteNodes(Square startSquare)
    {
        var map = new Dictionary<SegmentNode<TSquare>, SegmentNode<Square>>();
        foreach (var node in Nodes)
        {
            var absolutePos = startSquare + new Square(node.Position.Coordinates);
            var absoluteNode = new SegmentNode<Square>(absolutePos, node.Stayables);
            map[node] = absoluteNode;
        }
        foreach (var (original, absolute) in map)
        {
            foreach (var originalNeighbor in original.Neighbors)
            {
                if (map.TryGetValue(originalNeighbor, out var absoluteNeighbor))
                    absolute.Neighbors.Add(absoluteNeighbor);
            }
        }
        return map;
    }
    private Dictionary<LineKey, List<SegmentNode<Square>>> GetNodesFromPosition(Square startSquare)
    {
        var absoluteNodesMap = CreateAbsoluteNodes(startSquare);
        var result = new Dictionary<LineKey, List<SegmentNode<Square>>>();

        foreach (var (original, absolute) in absoluteNodesMap)
        {
            var key = GetLineKey(original.Position).Shift(startSquare);
            if (!result.TryGetValue(key, out var list))
                result[key] = list = new List<SegmentNode<Square>>();
            list.Add(absolute);
        }
        return result;
    }
    private (List<SegmentNode<TSquare>> overlapping, List<SegmentNode<TSquare>> adjacent) FindOverlappingAndAdjacent(SegmentNode<TSquare> newNode)
    {
        var overlapping = new List<SegmentNode<TSquare>>();
        var adjacent = new List<SegmentNode<TSquare>>();

        var sameKey = GetLineKey(newNode.Position);
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

    private static bool DictionaryEquals(Dictionary<LineKey, List<SegmentNode<TSquare>>> left, Dictionary<LineKey, List<SegmentNode<TSquare>>> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return false;
        if (left.Count != right.Count) return false;

        foreach (var kv in left)
        {
            if (!right.TryGetValue(kv.Key, out var rightList)) return false;
            if (!ListEquals(kv.Value, rightList)) return false;
        }
        return true;
    }
    private static bool ListEquals(List<SegmentNode<TSquare>> left, List<SegmentNode<TSquare>> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return false;
        if (left.Count != right.Count) return false;

        for (int i = 0; i < left.Count; i++)
        {
            if (!EqualityComparer<SegmentNode<TSquare>>.Default.Equals(left[i], right[i]))
                return false;
        }
        return true;
    }
    private static void RemoveConnections(SegmentNode<TSquare> node)
    {
        foreach (var neighbor in node.Neighbors.ToList())
        {
            neighbor.Neighbors.Remove(node);
            node.Neighbors.Remove(neighbor);
        }
    }
    private static TSquare OffsetPosition(TSquare pos, int dy, int dz, int dw)
    {
        var dict = new Dictionary<Axis, int>(pos.Coordinates);
        if (dict.ContainsKey(Axis.Y)) dict[Axis.Y] += dy;
        else if (dy != 0) dict.Add(Axis.Y, dy);
        if (dict.ContainsKey(Axis.Z)) dict[Axis.Z] += dz;
        else if (dz != 0) dict.Add(Axis.Z, dz);
        if (dict.ContainsKey(Axis.W)) dict[Axis.W] += dw;
        else if (dw != 0) dict.Add(Axis.W, dw);
        return pos.CopyWith(dict);
    }
    private static LineKey GetLineKey(TSquare pos) => pos is Square s ? new(s) : pos is Square3D s3 ? new(s3) : pos is Square2D s2 ? new(s2) : throw new ArgumentException($"Неверный тип TSquare {pos}");
    private static HashSet<LineKey> GetNeighborLineKeys(TSquare position)
    {
        var axes = position.ActiveAxes;
        var deltas = new List<(int dy, int dz, int dw)>();
        foreach (int dy in axes.HasFlag(MultipleAxes.Two) ? new[] { -1, 0, 1 } : new[] { 0 })
            foreach (int dz in axes.HasFlag(MultipleAxes.Three) ? new[] { -1, 0, 1 } : new[] { 0 })
                foreach (int dw in axes.HasFlag(MultipleAxes.Four) ? new[] { -1, 0, 1 } : new[] { 0 })
                    if (dy != 0 || dz != 0 || dw != 0)
                        deltas.Add((dy, dz, dw));

        var neighborKeys = new HashSet<LineKey>();
        foreach (var (dy, dz, dw) in deltas)
        {
            var offsetPos = OffsetPosition(position, dy, dz, dw);
            neighborKeys.Add(GetLineKey(offsetPos));
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

        var newPos = source.Position;
        Dictionary<Axis, int> dict = new(newPos.Coordinates)
        {
            [Axis.X] = source.Position.X + startOffset
        };
        var newSquare = newPos.CopyWith(dict);
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
        sb.AppendLine($"Count: {Count}, TotalStayables: {_totalStayablesCount}");
        foreach (var line in _lineIndex.OrderBy(kvp => kvp.Key.Y).ThenBy(kvp => kvp.Key.Z).ThenBy(kvp => kvp.Key.W))
        {
            sb.AppendLine($"Line {line.Key}:");
            foreach (var node in line.Value.OrderBy(n => n.Position.X))
            {
                sb.AppendLine($"  PatternNode: {node}");
                sb.AppendLine($"    Neighbors: {string.Join(", ", node.Neighbors.Select(nk => $"{nk.Position.Coordinates.Where(e => e.Key != Axis.X).Aggregate("[", (all, cur) => $"{all}{cur.Key}={cur.Value}, ")}X={nk.Position.X}..{nk.XMax}]"))}");
            }
        }
        return sb.ToString();
    }
    public ISequenceEnumerator<Square> GetEnumerator(Square start) => new Enumerable(this, start);

    public RelativeGraph<TSquare> Copy() => PrivateCopy();
    ISequence ICopyable<ISequence>.Copy() => PrivateCopy();
    public bool Equals(RelativeGraph<TSquare>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Count != other.Count) return false;
        if (GuaranteesAtLeastOneStayable != other.GuaranteesAtLeastOneStayable) return false;
        if (!EqualityComparer<SegmentNode<TSquare>>.Default.Equals(OriginNode, other.OriginNode)) return false;

        return DictionaryEquals(_lineIndex, other._lineIndex);
    }
    public override bool Equals(object? obj) => Equals(obj as RelativeGraph<TSquare>);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Count);
        hash.Add(GuaranteesAtLeastOneStayable);
        hash.Add(OriginNode);

        foreach (var kv in _lineIndex)
        {
            hash.Add(kv.Key);
            foreach (var node in kv.Value)
                hash.Add(node);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(RelativeGraph<TSquare> left, RelativeGraph<TSquare> right) => left.Equals(right);
    public static bool operator !=(RelativeGraph<TSquare> left, RelativeGraph<TSquare> right) => !(left == right);

    private class Enumerable : SequenceEnumerable<Square>
    {
        private readonly List<SegmentNode<Square>> _nodes;
        private Dictionary<Square, Stayable>? _allPositionsCache;
        private List<Square>? _onlyStayablesCache;

        public Enumerable(RelativeGraph<TSquare> graph, Square startSquare) : base(startSquare)
        {
            _nodes = graph.GetNodesFromPosition(startSquare).Values.SelectMany(list => list).ToList();
        }

        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;

                var result = new Dictionary<Square, Stayable>
                {
                    [StartPosition] = Stayable.NotStay
                };

                var startNode = _nodes.FirstOrDefault(node =>
                    node.Position.X <= StartPosition.X && StartPosition.X <= node.XMax &&
                    node.Position.IsSameLine(StartPosition));

                if (startNode == null)
                {
                    _allPositionsCache = result;
                    return result;
                }

                var visitedNodes = new HashSet<SegmentNode<Square>>();
                var queue = new Queue<SegmentNode<Square>>();
                queue.Enqueue(startNode);
                visitedNodes.Add(startNode);

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    for (int x = node.Position.X; x <= node.XMax; x++)
                    {
                        var square = RelativeGraph<Square>.Enumerable.CreateSquareFromNode(node, x);
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
        protected override List<Square> CreateOnlyStayablesPositions
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

        private SegmentNode<Square>? FindNodeContainingSquare(Square square)
        {
            return _nodes.FirstOrDefault(node =>
                node.Position.IsSameLine(square) &&
                node.Position.X <= square.X && square.X <= node.XMax);
        }

        private static bool IsInsideSameNode(SegmentNode<Square> node, Square from, Square to)
        {
            if (!node.Position.IsSameLine(to)) return false;
            if (node.Position.X > to.X || to.X > node.XMax) return false;
            return Math.Abs(to.X - from.X) == 1 &&
                   from.Y == to.Y && from.Z == to.Z && from.W == to.W;
        }
        private static Square CreateSquareFromNode(SegmentNode<Square> node, int x)
        {
            Dictionary<Axis, int> dict = new(node.Position.Coordinates)
            {
                [Axis.X] = x
            };
            return new Square(dict);
        }
        private static SegmentNode<Square>? FindNeighborNodeForTransition(SegmentNode<Square> currentNode, Square from, Square to)
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

        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
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

                    if (RelativeGraph<TSquare>.Enumerable.IsInsideSameNode(currentNode, from, to))
                        stepValid = true;
                    else
                    {
                        var nextNode = RelativeGraph<TSquare>.Enumerable.FindNeighborNodeForTransition(currentNode, from, to);
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
        }
        public override bool CanMoveTo(Square target)
        {
            return CreateAllPossiblePositions.TryGetValue(target, out var stayable) && stayable == Stayable.Stay;
        }
    }
}