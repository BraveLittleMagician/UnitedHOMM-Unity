#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PieceRenderer : MonoBehaviour
{
    private IGlobalGeneratedDynamicObjectsHolder _holder = null!;
    private IObjectResolver _resolver = null!;
    private GameObject? _piecesRoot = null;
    private readonly Dictionary<IPiece, Vector3> _pendingPieces = new();
    private readonly Dictionary<IPiece, GameObject> _pieceObjects = new();

    [SerializeField] private GameObject _piecePrefab = null!;

    private void Awake()
    {
        if (_piecePrefab == null) throw new ArgumentNullException(nameof(_piecePrefab));
    }
    private float Offset => _piecePrefab.transform.lossyScale.y;

    private void BuildPiecesRoot()
    {
        _piecesRoot = new GameObject("Pieces");
        _piecesRoot.transform.SetParent(_holder.Transform, false);
        _resolver.InjectGameObject(_piecesRoot);
    }

    [Inject]
    public void Construct(IGlobalGeneratedDynamicObjectsHolder holder, IObjectResolver resolver)
    {
        _holder = holder ?? throw new ArgumentNullException(nameof(holder));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        if (_piecesRoot == null)
        {
            BuildPiecesRoot();
            BuildPieces();
        }
    }

    private void BuildPieces()
    {
        foreach (var pair in _pendingPieces) CreatePiece(pair.Key, pair.Value);
        _pendingPieces.Clear();
    }

    public void CreatePiece(IPiece piece, Vector3 worldPosition)
    {
        if (_piecesRoot == null)
        {
            if (_pendingPieces.ContainsKey(piece)) return;
            _pendingPieces.Add(piece, new Vector3(worldPosition.x, worldPosition.y + Offset, worldPosition.z));
        }
        else
        {
            if (_pieceObjects.ContainsKey(piece)) return;
            
            var go = Instantiate(_piecePrefab, new Vector3(worldPosition.x, worldPosition.y + Offset, worldPosition.z), Quaternion.identity, _piecesRoot.transform);
            _resolver.InjectGameObject(go);
            _pieceObjects[piece] = go;
        }
    }
    public void DestroyPiece(IPiece piece)
    {
        if (_pieceObjects.TryGetValue(piece, out var go))
        {
            Destroy(go);
            _pieceObjects.Remove(piece);
        }
        _pendingPieces?.Remove(piece);
    }
    public void UpdatePiecePosition(IPiece piece, Vector3 worldPosition)
    {
        if (_piecesRoot == null)
        {
            if (_pendingPieces.ContainsKey(piece)) return;
            _pendingPieces[piece] = new Vector3(worldPosition.x, worldPosition.y + Offset, worldPosition.z);
        }
        else
        {
            if (_pieceObjects.TryGetValue(piece, out var go))
                go.transform.position = new Vector3(worldPosition.x, worldPosition.y + Offset, worldPosition.z);
        }
    }
}