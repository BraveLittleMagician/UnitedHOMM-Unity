#nullable enable
using System;
using UnityEngine;

public sealed class PieceSpawner
{
    private readonly Transform _piecesRoot;
    private readonly Material _pieceMaterial;
    private readonly PiecePrefabRegistry _prefabs;
    private readonly int _teamCount;
    private readonly int _subteamCount;

    public PieceSpawner(
        Transform piecesRoot,
        Material pieceMaterial,
        PiecePrefabRegistry prefabs,
        int teamCount,
        int subteamCount)
    {
        _piecesRoot = piecesRoot != null ? piecesRoot : throw new ArgumentNullException(nameof(piecesRoot));
        _pieceMaterial = pieceMaterial != null ? pieceMaterial : throw new ArgumentNullException(nameof(pieceMaterial));
        _prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
        _teamCount = teamCount;
        _subteamCount = subteamCount;
    }

    public InfoOfPiece Spawn(IPiece piece, Square position, Vector3 localPos, int layerIndex)
    {
        var prefab = _prefabs.GetPrefab(piece.Name);
        var go = UnityEngine.Object.Instantiate(prefab, _piecesRoot);
        go.SetLayerRecursively(layerIndex);
        go.name = $"{position.X}_{position.Y}_{position.Z}_{position.W}_{piece.Name}";
        go.transform.localPosition = localPos;

        var color = PieceColorPalette.GetColor(piece.Owner, _teamCount, _subteamCount);
        var view = go.GetOrAddComponent<InfoOfPiece>();
        view.Initialize(piece, _pieceMaterial, color, position);
        return view;
    }
}