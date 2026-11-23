#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class PieceValidationTests
{
    private sealed class Fixture
    {
        public House House = null!;
        public Board Board = null!;
        public Registry Registry = null!;
        public Flow Flow = null!;
    }

    private static Fixture Setup()
    {
        var logger = new LoggerNull();
        var bus = new EventBus(logger);
        var host = new ModifierHost(bus, logger);
        var seats = new Seats(2, 1);
        var pieceFactory = new PieceFactory(bus, host, logger);
        var house = new House(seats, bus, logger, pieceFactory);
        var registry = new Registry(house);
        var board = new Board(seats, new Square { X = 8, Y = 8, Z = 1, W = 1 }, wwOffset: 0, bus, logger);
        house.AddRoom(board);
        host.Registry = registry;

        var ab = new AbilityService(bus, logger);
        var atp = new AttackTargetProvider(host);
        var cs = new CombatService(logger, bus, atp, host);
        var flow = new Flow(house, registry, bus, logger, ab, cs, host);

        return new Fixture { House = house, Board = board, Registry = registry, Flow = flow };
    }

    // Pattern с пустым словарём после конструктора = { origin: NotStay } — 0 stayables.
    private static Pattern<Square> EmptyPattern()
        => new Pattern<Square>(ImmutableDictionary<Square, Stayable>.Empty);

    [Test]
    public void EmptyPattern_PieceIsRejected()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);

        var def = new PieceDefinition(
            "Immobile", white, 10, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<Pattern<Square>>(EmptyPattern()) });

        var result = f.Flow.AddPiece<Board, Square>(def, new Square { X = 3, Y = 3 });

        Assert.IsFalse(result.IsSuccess);
        Assert.That(result.Error, Does.Contain("no move cells"));
    }

    [Test]
    public void ValidPattern_PieceIsAccepted()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);

        var def = new PieceDefinition(
            "Walker", white, 10, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<Pattern<Square>>(new Pattern<Square>(new Square { X = 1, Y = 0 })) });

        var result = f.Flow.AddPiece<Board, Square>(def, new Square { X = 3, Y = 3 });

        Assert.IsTrue(result.IsSuccess, result.Error);
    }

    [Test]
    public void InvalidPiece_FreesIndex_SoNextValidPieceSucceeds()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);

        var badDef = new PieceDefinition(
            "Immobile", white, 10, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<Pattern<Square>>(EmptyPattern()) });

        var r1 = f.Flow.AddPiece<Board, Square>(badDef, new Square { X = 3, Y = 3 });
        Assert.IsFalse(r1.IsSuccess);

        var goodDef = new PieceDefinition(
            "Walker", white, 10, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<Pattern<Square>>(new Pattern<Square>(new Square { X = 1, Y = 0 })) });

        var r2 = f.Flow.AddPiece<Board, Square>(goodDef, new Square { X = 3, Y = 3 });
        Assert.IsTrue(r2.IsSuccess, r2.Error);
    }

    [Test]
    public void SeparatedPiece_WithEmptyMove_Fails()
    {
        // MovementSeparated с пустым move, непустым attack — комбинированный
        // PositionsStayables пуст (stayables приходят только из move-формы),
        // значит фигура обездвижена — config error.
        var f = Setup();
        var white = new IdOfPlayer(0, 0);

        var def = new PieceDefinition(
            "Turret", white, 10, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<Pattern<Square>>(
                EmptyPattern(),
                new Pattern<Square>(new Square { X = 1, Y = 0 })) });

        var result = f.Flow.AddPiece<Board, Square>(def, new Square { X = 3, Y = 3 });

        Assert.IsFalse(result.IsSuccess);
        Assert.That(result.Error, Does.Contain("no move cells"));
    }
}