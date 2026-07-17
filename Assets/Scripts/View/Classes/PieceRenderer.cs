#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _piecePrefab = null!;
    private readonly Dictionary<IPiece, GameObject> _pieceObjects = new();

    private void Awake()
    {
        if (_piecePrefab == null) throw new ArgumentNullException(nameof(_piecePrefab));
    }
    private float Offset => _piecePrefab.transform.lossyScale.y;
    
    public void ShowPiece(IPiece piece, Vector3 worldPosition)
    {
        if (_pieceObjects.ContainsKey(piece)) return;
        var go = Instantiate(_piecePrefab, new Vector3(worldPosition.x, worldPosition.y + Offset, worldPosition.z), Quaternion.identity, transform);
        _pieceObjects[piece] = go;
    }

    public void HidePiece(IPiece piece)
    {
        if (_pieceObjects.TryGetValue(piece, out var go))
        {
            Destroy(go);
            _pieceObjects.Remove(piece);
        }
    }

    public void UpdatePiecePosition(IPiece piece, Vector3 newPosition)
    {
        if (_pieceObjects.TryGetValue(piece, out var go))
            go.transform.position = new Vector3(newPosition.x, newPosition.y + Offset, newPosition.z);
    }
}