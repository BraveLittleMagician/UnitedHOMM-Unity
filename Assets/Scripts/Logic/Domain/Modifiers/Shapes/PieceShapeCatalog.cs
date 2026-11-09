#nullable enable

using System;
using System.Collections.Generic;

public sealed class PieceShapeCatalog
{
    private readonly Dictionary<string, Pattern<Square>> _patterns = new();
    private readonly Dictionary<string, PathSetPattern> _pathSets = new();
    private readonly Dictionary<string, StepBudgetPattern> _stepBudgets = new();
    private readonly Dictionary<string, RelativeArea<Square>> _areas = new();
    private readonly Dictionary<string, RelativeGraph<Square>> _graphs = new();

    public void RegisterPattern(string name, Pattern<Square> shape)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (shape.Equals(default))
            throw new ArgumentException($"Pattern for '{name}' is uninitialized.", nameof(shape));
        _patterns[name] = shape;
    }

    public void RegisterPathSet(string name, PathSetPattern shape)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (shape.Equals(default))
            throw new ArgumentException($"PathSetPattern for '{name}' is uninitialized.", nameof(shape));
        _pathSets[name] = shape;
    }

    public void RegisterStepBudget(string name, StepBudgetPattern shape)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (shape.Equals(default))
            throw new ArgumentException($"StepBudgetPattern for '{name}' is uninitialized.", nameof(shape));
        _stepBudgets[name] = shape;
    }
    public void RegisterArea(string name, RelativeArea<Square> shape)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (shape.GuaranteesAtLeastOneStayable == false && shape.Length == 0)
            throw new ArgumentException($"RelativeArea for '{name}' is uninitialized.", nameof(shape));
        _areas[name] = shape;
    }

    public void RegisterGraph(string name, RelativeGraph<Square> shape)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (shape.Nodes == null)
            throw new ArgumentException($"RelativeGraph for '{name}' is uninitialized.", nameof(shape));
        _graphs[name] = shape;
    }

    public IMovementResolver BuildResolver(string moveShapeName, string? attackShapeName, PieceRegistry registry, ModifierHost? modifierHost)
    {
        if (string.IsNullOrEmpty(moveShapeName))
            throw new ArgumentException("Move shape name must not be empty.", nameof(moveShapeName));

        if (attackShapeName == null)
            return BuildSingleForm(moveShapeName, registry, modifierHost);

        return BuildComposite(moveShapeName, attackShapeName, registry, modifierHost);
    }

    private IMovementResolver BuildSingleForm(string moveShapeName, PieceRegistry registry, ModifierHost? modifierHost)
    {
        if (_patterns.TryGetValue(moveShapeName, out var pattern)) return SequenceMovementResolver.ForShape(registry, modifierHost, pattern);
        if (_pathSets.TryGetValue(moveShapeName, out var pathSet)) return SequenceMovementResolver.ForShape(registry, modifierHost, pathSet);
        if (_stepBudgets.TryGetValue(moveShapeName, out var stepBudget)) return SequenceMovementResolver.ForShape(registry, modifierHost, stepBudget);
        if (_areas.TryGetValue(moveShapeName, out var area)) return SequenceMovementResolver.ForShape(registry, modifierHost, area);
        if (_graphs.TryGetValue(moveShapeName, out var graph)) return SequenceMovementResolver.ForShape(registry, modifierHost, graph);

        throw new InvalidOperationException(
            $"Move shape '{moveShapeName}' is not registered in PieceShapeCatalog. " +
            $"Available: patterns=[{Join(_patterns.Keys)}], " +
            $"pathSets=[{Join(_pathSets.Keys)}], " +
            $"stepBudgets=[{Join(_stepBudgets.Keys)}].");
    }

    private IMovementResolver BuildComposite(string moveShapeName, string attackShapeName, PieceRegistry registry, ModifierHost? modifierHost)
    {
        if (_patterns.TryGetValue(moveShapeName, out var movePattern))
        {
            if (!_patterns.TryGetValue(attackShapeName, out var attackPattern))
                throw MixedType(moveShapeName, attackShapeName, "Pattern", FindKind(attackShapeName));

            return SequenceMovementResolver.ForCombined(
                registry, modifierHost, movePattern, attackPattern);
        }

        if (_pathSets.TryGetValue(moveShapeName, out var movePathSet))
        {
            if (!_pathSets.TryGetValue(attackShapeName, out var attackPathSet))
                throw MixedType(moveShapeName, attackShapeName, "PathSetPattern", FindKind(attackShapeName));

            return SequenceMovementResolver.ForCombined(
                registry, modifierHost, movePathSet, attackPathSet);
        }

        if (_stepBudgets.TryGetValue(moveShapeName, out var moveStepBudget))
        {
            if (!_stepBudgets.TryGetValue(attackShapeName, out var attackStepBudget))
                throw MixedType(moveShapeName, attackShapeName, "StepBudgetPattern", FindKind(attackShapeName));

            return SequenceMovementResolver.ForCombined(
                registry, modifierHost, moveStepBudget, attackStepBudget);
        }
        if (_areas.TryGetValue(moveShapeName, out var moveArea))
        {
            if (!_areas.TryGetValue(attackShapeName, out var attackArea))
                throw MixedType(moveShapeName, attackShapeName, "RelativeArea", FindKind(attackShapeName));
            return SequenceMovementResolver.ForCombined(registry, modifierHost, moveArea, attackArea);
        }

        if (_graphs.TryGetValue(moveShapeName, out var moveGraph))
        {
            if (!_graphs.TryGetValue(attackShapeName, out var attackGraph))
                throw MixedType(moveShapeName, attackShapeName, "RelativeGraph", FindKind(attackShapeName));
            return SequenceMovementResolver.ForCombined(registry, modifierHost, moveGraph, attackGraph);
        }
        throw new InvalidOperationException(
            $"Move shape '{moveShapeName}' is not registered in PieceShapeCatalog.");
    }

    private string FindKind(string name)
    {
        if (_patterns.ContainsKey(name)) return "Pattern";
        if (_pathSets.ContainsKey(name)) return "PathSetPattern";
        if (_stepBudgets.ContainsKey(name)) return "StepBudgetPattern";
        if (_areas.ContainsKey(name)) return "RelativeArea";
        if (_graphs.ContainsKey(name)) return "RelativeGraph";
        return "unknown";
    }

    private static InvalidOperationException MixedType(
        string moveName, string attackName, string moveKind, string attackKind)
    {
        return new InvalidOperationException(
            $"Cannot build composite resolver: move shape '{moveName}' is {moveKind}, " +
            $"attack shape '{attackName}' is {attackKind}. " +
            $"Both must be the same shape type (mixed-type composite is not supported yet).");
    }

    private static string Join(IEnumerable<string> keys)
    {
        var list = new List<string>(keys);
        return list.Count == 0 ? "(none)" : string.Join(", ", list);
    }
}