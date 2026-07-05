#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour, IPointerUpHandler
{
    private Camera? _camera;
    [SerializeField] private GridRenderer? _gridRenderer;
    [SerializeField] private PieceRenderer? _pieceRenderer;

    private void Awake()
    {
        _camera = Camera.main != null ? Camera.main : throw new NullReferenceException(nameof(_camera));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_camera == null) return;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<CellIdentifier>(out var cell))
            {
                CellClicked?.Invoke(cell.Square);
                return;
            }

            var pieceId = hit.collider.GetComponent<PieceIdentifier>();
            if (pieceId != null && pieceId.Piece != null)
                PieceClicked?.Invoke(pieceId.Piece);
        }
    }

    public class CellIdentifier : MonoBehaviour
    {
        [field: SerializeField]
        public Square Square { get; set; }
    }

    public class PieceIdentifier : MonoBehaviour
    {
        [field: SerializeField]
        public IPiece? Piece { get; set; }
    }

    public event Action<Square>? CellClicked;
    public event Action<IPiece>? PieceClicked;
}