#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class PieceRenderer : MonoBehaviour
{
    [SerializeField] private GameObject? _piecePrefab;
    [SerializeField] private GameObject? _selectedMarkerPrefab;

    private readonly Dictionary<IPiece, GameObject> _pieceObjects = new();
    private GameObject? _selectedMarkerInstance;
    private IPiece? _selectedPiece;

    private void Awake()
    {
        if (_selectedMarkerPrefab != null)
            _selectedMarkerInstance = Instantiate(_selectedMarkerPrefab, transform);
        else
            _selectedMarkerInstance = new GameObject("SelectedMarker");
        _selectedMarkerInstance.SetActive(false);
    }

    public void ShowPiece(IPiece piece, Vector3 worldPosition)
    {
        if (_pieceObjects.ContainsKey(piece)) return;
        var go = Instantiate(_piecePrefab, worldPosition, Quaternion.identity, transform);
        if (go != null)
            _pieceObjects[piece] = go;
        else
        {
            Destroy(go);
        }
    }

    public void HidePiece(IPiece piece)
    {
        if (_pieceObjects.TryGetValue(piece, out var go))
        {
            Destroy(go);
            _pieceObjects.Remove(piece);
            if (_selectedPiece == piece)
                DeselectPiece();
        }
    }

    public void UpdatePiecePosition(IPiece piece, Vector3 newPosition)
    {
        if (_pieceObjects.TryGetValue(piece, out var go))
            go.transform.position = newPosition;
    }

    public void SelectPiece(IPiece piece)
    {
        if (_selectedPiece == piece) return;
        DeselectPiece();
        _selectedPiece = piece;
        if (_pieceObjects.TryGetValue(piece, out var go) && _selectedMarkerInstance != null)
        {
            _selectedMarkerInstance.transform.position = go.transform.position;
            _selectedMarkerInstance.SetActive(true);
        }
    }

    public void DeselectPiece()
    {
        _selectedPiece = null;
        if (_selectedMarkerInstance != null)
            _selectedMarkerInstance.SetActive(false);
    }

    public bool TryGetPieceAtPosition(Vector3 position, out IPiece? piece)
    {
        // Упрощённо: можно искать по коллайдерам, но проще использовать отдельный словарь позиций
        // Пока заглушка
        piece = null;
        return false;
    }
}