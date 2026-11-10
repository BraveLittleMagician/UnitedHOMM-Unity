#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public sealed class WorldSnapshot
{
    private readonly List<PieceModelState> _modelStates;
    private readonly List<PieceMirrorState> _mirrorStates;

    private WorldSnapshot(List<PieceModelState> models, List<PieceMirrorState> mirrors)
    {
        _modelStates = models;
        _mirrorStates = mirrors;
    }

    public static WorldSnapshot Capture(PieceRegistry registry, IHouse house)
    {
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        if (house == null) throw new ArgumentNullException(nameof(house));

        var modelStates = new List<PieceModelState>(registry.AllModels.Count);
        foreach (var m in registry.AllModels)
            modelStates.Add(new PieceModelState(m));

        var mirrorStates = new List<PieceMirrorState>();
        foreach (var room in house.Rooms.Values)
        {
            foreach (var p in room.GetAllPieces())
                mirrorStates.Add(new PieceMirrorState(p));
        }

        return new WorldSnapshot(modelStates, mirrorStates);
    }

    public void Restore(PieceRegistry registry, IHouse house)
    {
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        if (house == null) throw new ArgumentNullException(nameof(house));

        // 1) PieceModel: позиция, HP, заряды, facing, флаги.
        foreach (var saved in _modelStates)
        {
            var current = registry.GetByPieceId(saved.PieceId);
            if (current is null) continue;   // модель не в реестре — пропускаем (edge: удалена)

            if (current.PositionInGrid != saved.Position)
                registry.Move(current, saved.Position);

            current.RestoreCombat(saved.Hp, saved.AttackCharges);
            current.RestoreFacing(saved.Facing);
            current.RestoreFirstActionOfGameTaken(saved.HasTakenFirstActionOfGame);
            current.RestoreMovedThisTurn(saved.HasMovedThisTurn);
            current.RestoreTotalRotationDegrees(saved.TotalRotationDegrees);
            current.RestorePromotedThisTurn(saved.HasPromotedThisTurn);
        }

        // 2) Piece (P1): Health, Position, Facing, флаги — через мосты.
        foreach (var saved in _mirrorStates)
        {
            if (!house.TryToFindPiece(saved.IndexInHouse, out var piece)) continue;
            piece.RestoreHealth(saved.Health);
            piece.SetPosition(saved.Position);
            piece.SetFacing(saved.Facing);
            piece.RestoreMovedThisTurn(saved.HasMovedThisTurn);
            piece.RestorePromotedThisTurn(saved.HasPromotedThisTurn);
        }
    }

    private readonly struct PieceModelState
    {
        public long PieceId { get; }
        public Vector3Int Position { get; }
        public FacingDirection Facing { get; }
        public int Hp { get; }
        public int AttackCharges { get; }
        public bool HasTakenFirstActionOfGame { get; }
        public bool HasMovedThisTurn { get; }
        public int TotalRotationDegrees { get; }
        public bool HasPromotedThisTurn { get; }

        public PieceModelState(PieceModel m)
        {
            PieceId = m.PieceId;
            Position = m.PositionInGrid;
            Facing = m.Facing;
            Hp = m.Hp;
            AttackCharges = m.AttackCharges;
            HasTakenFirstActionOfGame = m.HasTakenFirstActionOfGame;
            HasMovedThisTurn = m.HasMovedThisTurn;
            TotalRotationDegrees = m.TotalRotationDegreesThisTurn;
            HasPromotedThisTurn = m.HasPromotedThisTurn;
        }
    }

    private readonly struct PieceMirrorState
    {
        public BigInteger IndexInHouse { get; }
        public Vector3Int Position { get; }
        public FacingDirection Facing { get; }
        public int Health { get; }
        public bool HasMovedThisTurn { get; }
        public bool HasPromotedThisTurn { get; }

        public PieceMirrorState(IPiece p)
        {
            IndexInHouse = p.IndexInHouse;
            Position = p.PositionInGrid;
            Facing = p.Facing;
            Health = p.Health;
            HasMovedThisTurn = p.HasMovedThisTurn;
            HasPromotedThisTurn = p.HasPromotedThisTurn;
        }
    }
}