#nullable enable

using System;
using System.Numerics;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InfoOfPiece : MonoBehaviour, ISelectable, IHoverable
{
    private IPiece _piece = null!;
    private PieceVisual _visual = null!;

    public IPiece Piece => _piece;
    public string Name => _piece.Name;
    public IdOfPlayer Player => _piece.Owner;
    public BigInteger IndexInHouse => _piece.IndexInHouse;
    public bool IsDead => _piece.IsDead;

    public Square PositionInGrid { get; set; }
    public bool IsSelected { get; private set; }
    public bool IsHovered { get; private set; }

    public void Initialize(IPiece piece, Material pieceMaterial, Color teamColor, Square position)
    {
        if (_piece != null) throw new InvalidOperationException($"InfoOfPiece '{name}' already initialized.");
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        PositionInGrid = position;

        _visual = gameObject.GetOrAddComponent<PieceVisual>();
        _visual.Initialize(pieceMaterial);
        _visual.ApplyTeamColor(teamColor);

        ApplyFacingRotation(_piece.Facing);
        ApplyDeadState();
    }

    public void SetPosition(Square position)
    {
        PositionInGrid = position;
    }

    public void SetFacing(FacingDirection facing) => ApplyFacingRotation(facing);

    public void SetDead(bool dead) => gameObject.SetActive(!dead);

    public void OnSelected() => SetSelected(true);
    public void OnDeselected() => SetSelected(false);

    public void SetSelected(bool value)
    {
        if (IsSelected == value) return;
        IsSelected = value;
        _visual?.SetState(IsSelected, highlighted: false, IsHovered);
    }

    public void SetHovered(bool value)
    {
        if (IsHovered == value) return;
        IsHovered = value;
        _visual?.SetState(IsSelected, highlighted: false, IsHovered);
    }

    private void ApplyFacingRotation(FacingDirection facing)
    {
        float angle = facing switch
        {
            FacingDirection.PlusY => 0f,
            FacingDirection.PlusX => 90f,
            FacingDirection.MinusY => 180f,
            FacingDirection.MinusX => -90f,
            _ => 0f,
        };
        transform.localRotation = UnityEngine.Quaternion.Euler(0f, angle, 0f);
    }

    private void ApplyDeadState()
    {
        if (_piece != null) gameObject.SetActive(!_piece.IsDead);
    }
}