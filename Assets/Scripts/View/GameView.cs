#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour, IGameView
{
    [SerializeField] private GameObject? _piecePrefab;
    private readonly Dictionary<IPiece, GameObject> _pieces = new();

    private void Awake()
    {
        if (_piecePrefab == null) throw new NullReferenceException(nameof(_piecePrefab));
    }

    public void ShowPiece(IPiece piece)
    {
        var go = Instantiate(_piecePrefab, transform);
        if (go != null)
            _pieces[piece] = go;
    }

    public void HidePiece(IPiece piece)
    {
        if (_pieces.TryGetValue(piece, out var go))
        {
            Destroy(go);
            _pieces.Remove(piece);
        }
    }
}