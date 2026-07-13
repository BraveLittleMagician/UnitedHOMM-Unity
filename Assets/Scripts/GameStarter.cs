#nullable enable

using System;

public class GameStarter : IGameStarter
{
    private readonly GameStarterDependencies _deps;
    private bool _started;

    public GameStarter(GameStarterDependencies deps) 
    {
        _deps = deps ?? throw new ArgumentNullException(nameof(deps));
    }

    public void StartGame()
    {
        if (_started) return;
        _started = true;

        _deps.House.AddRoom(_deps.Board);
        var decks = new Decks(_deps.Seats, _deps.EventBus, _deps.Logger);
        _deps.House.AddRoom(decks);

        if (_deps.GameView is GameView view)
        {
            view.Initialize(_deps.EventBus);
            var axes = (MultipleAxes)_deps.BoardConfig.Axes;
            var box = new AxisAlignedBox(axes, _deps.BoardConfig.FieldSize, 0);
            view.BuildGrid(box, axes);
        }

        _deps.Camera.FitToBoard(_deps.Board.Bounds);

        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _deps.Flow.AddPiece<Board, Square>(definition, Square.Zero);
    }
}