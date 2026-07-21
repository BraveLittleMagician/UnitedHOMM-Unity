#nullable enable

using System;
using VContainer.Unity;
using UnityEngine;

public class GamePresenter : IStartable, IDisposable
{
    private readonly IFlow _flow;
    private readonly IGameView _view;
    private readonly IEventBus _eventBus;
    private readonly IControllerOfCamera _camera;
    private readonly IRegistry _registry;
    private readonly BoardConfigUpdater _configUpdater;
    private BoardConfig _currentConfig;

    public GamePresenter(IFlow flow, IGameView view, IEventBus eventBus, IControllerOfCamera camera, IRegistry registry, BoardConfigUpdater configUpdater, BoardConfig initialConfig)
    {
        _flow = flow;
        _view = view;
        _eventBus = eventBus;
        _camera = camera;
        _registry = registry;
        _configUpdater = configUpdater;
        _currentConfig = initialConfig;

        _view.BoardSizeInputChanged += OnBoardSizeInputChanged;
        
        _eventBus.Subscribe<PieceSelectedEvent>(OnPieceSelectedEvent);
        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnPieceSelectedEvent(PieceSelectedEvent e)
    {
        var piece = e.Piece;
        if (!_registry.TryToGetRoom(piece.IndexInHouse, out var room))
        {
            Debug.Log($"Фигура {piece} не найдена в реестре.");
            return;
        }

        if (room is Board)
        {
            if (_registry.TryToGetPosition<Square>(piece.IndexInHouse, out var pos))
            {
                Debug.Log($"Фигура {piece} находится в комнате {room.Name} на позиции {pos}");
            }
            else
            {
                Debug.Log($"Фигура {piece} в комнате {room.Name}, но позиция не определена.");
            }
        }
        else if (room is Decks)
        {
            if (_registry.TryToGetPosition<int>(piece.IndexInHouse, out var deckPos))
            {
                Debug.Log($"Фигура {piece} находится в колоде {room.Name} на позиции {deckPos}");
            }
            else
            {
                Debug.Log($"Фигура {piece} в колоде {room.Name}, но позиция не определена.");
            }
        }
        else
        {
            Debug.Log($"Фигура {piece} в комнате {room?.Name} неизвестного типа.");
        }
    }
    private void OnBoardSizeInputChanged(int newSize)
    {
        if (newSize <= 0) return;
        var newConfig = _currentConfig with { FieldSize = newSize };
        _configUpdater.UpdateConfig(newConfig);
    }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        _currentConfig = e.NewConfig;
        _view.SetBoardSize(e.NewConfig.FieldSize);
        var axes = (MultipleAxes)_currentConfig.Axes;
        var box = new AxisAlignedBox(axes, _currentConfig.FieldSize, e.NewConfig.WUp, e.NewConfig.WDown);
        _camera.FitToBoard(box.Bounds);
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e) => _view.CreatePiece(e.Piece, e.Position);
    private void OnPieceDeployed(PieceDeployedEvent<int> e) => _view.CreatePieceInDeck(e.Piece, e.Position);
    private void OnPieceMoved(PieceMovedEvent<Square> e) => _view.UpdatePiecePosition(e.Piece, e.ToPosition);
    private void OnPieceMoved(PieceMovedEvent<int> e) => _view.UpdatePieceInDeckPosition(e.Piece, e.ToPosition);
    private void OnPieceDied(PieceDiedEvent e) => _view.DestroyPiece(e.Piece);
    
    public void Start()
    {
        _view.SetBoardSize(_currentConfig.FieldSize);
    }
    public void Dispose()
    {
        _view.BoardSizeInputChanged -= OnBoardSizeInputChanged;
        _eventBus.Unsubscribe<PieceSelectedEvent>(OnPieceSelectedEvent);
        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }
}