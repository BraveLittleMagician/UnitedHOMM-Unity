#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static class ModifierFactory
{
    public static IModifier Create(string name, JObject? args = null) => name switch
    {
        "DoubleMove" => new OpeningDoubleMoveModifier(),
        "SelfHeal" => new SelfHealModifier(),
        "RotateRight" => new RotateModifier(isRight: true),
        "RotateLeft" => new RotateModifier(isRight: false),
        "AttackBoost" => new AttackBoostModifier(),
        "AttackBoostBuff" => new AttackBoostBuffModifier(),
        "Blind" => new BlindModifier(),
        "BlindBuff" => new BlindBuffModifier(),
        "Stun" => new StunModifier(),
        "StunBuff" => new StunBuffModifier(),
        "Fireball" => new FireballModifier(),
        "CounterAttack" => new CounterAttackModifier(),
        "Ambush" => new AmbushModifier(),
        "EnPassant" => new EnPassantModifier(),

        "AdderStayableOfArea" => new AdderStayableOfArea<Square>(args?["Index"]?.Value<int>() ?? 0),
        "RemoverStayableOfArea" => new RemoverStayableOfArea<Square>(args?["Index"]?.Value<int>() ?? 0),
        "IncreaserRadiusOfArea" => new IncreaserRadiusOfArea<Square>(),
        "DecreaserRadiusOfArea" => new DecreaserRadiusOfArea<Square>(),
        "AdderStayableOfPattern" => new AdderStayableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "RemoverStayableOfPattern" => new RemoverStayableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "DecreaserOfPattern" => new DecreaserOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "IncreaserNotStayableOfPattern" => new IncreaserNotStayableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "IncreaserStayableOfPattern" => new IncreaserStayableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "AdderAttackableOfPattern" => new AdderAttackableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "RemoverAttackableOfPattern" => new RemoverAttackableOfPattern<Square>(ParseSquaresArray(args?["Squares"])),
        "AdderAttackableOfPathSet" => new AdderAttackableOfPathSet(ParseSquaresArray(args?["Squares"])),
        "RemoverAttackableOfPathSet" => new RemoverAttackableOfPathSet(ParseSquaresArray(args?["Squares"])),
        "AdderAttackableOfStepBudget" => new AdderAttackableOfStepBudget(ParseSquaresArray(args?["Squares"])),
        "RemoverAttackableOfStepBudget" => new RemoverAttackableOfStepBudget(ParseSquaresArray(args?["Squares"])),
        "DecreaserOfGraph" => new DecreaserOfGraph<Square>(ParseSquaresArray(args?["Positions"])),
        "SetStayableOfGraph" => new SetStayableOfGraph<Square>(ParseStayableTuples(args?["Cells"])),
        "IncreaserOfGraph" => new IncreaserOfGraph<Square>(ParseGraphNodes(args?["Nodes"])),

        _ => throw new InvalidOperationException($"Unknown modifier '{name}'."),
    };

    private static Square ParseSquare(JToken? token)
    {
        if (token == null) return Square.Zero;
        return new Square
        {
            X = token["X"]?.Value<int>() ?? 0,
            Y = token["Y"]?.Value<int>() ?? 0,
            Z = token["Z"]?.Value<int>() ?? 0,
            W = token["W"]?.Value<int>() ?? 0,
        };
    }

    private static IEnumerable<Square> ParseSquaresArray(JToken? token)
    {
        if (token is not JArray array) return Array.Empty<Square>();
        var list = new List<Square>(array.Count);
        foreach (var item in array) list.Add(ParseSquare(item));
        return list;
    }

    private static IEnumerable<(Square position, Stayable stayable)> ParseStayableTuples(JToken? token)
    {
        if (token is not JArray array) return Array.Empty<(Square, Stayable)>();
        var list = new List<(Square, Stayable)>(array.Count);
        foreach (var item in array)
        {
            var pos = ParseSquare(item["Position"]);
            var stayableStr = item["Stayable"]?.Value<string>();
            var stayable = Enum.TryParse<Stayable>(stayableStr, out var s) ? s : Stayable.NotStay;
            list.Add((pos, stayable));
        }
        return list;
    }

    private static IEnumerable<(Square position, int length, IEnumerable<int> stayableIndices)> ParseGraphNodes(JToken? token)
    {
        if (token is not JArray array) return Array.Empty<(Square, int, IEnumerable<int>)>();
        var list = new List<(Square, int, IEnumerable<int>)>(array.Count);
        foreach (var item in array)
        {
            var pos = ParseSquare(item["Position"]);
            var length = item["Length"]?.Value<int>() ?? 0;
            var indices = item["StayableIndices"]?.ToObject<List<int>>() ?? new List<int>();
            list.Add((pos, length, indices));
        }
        return list;
    }
}