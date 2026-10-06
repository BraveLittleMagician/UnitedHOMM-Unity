#nullable enable

using System;
using System.Numerics;
using UnityEngine;

public class InfoOfPiece : MonoBehaviour
{
    [field: SerializeField] public string Name { get; private set; } = "";
    [field: SerializeField] public IndexOfPlayer Player { get; private set; }
    [field: SerializeField] public BigInteger IndexInHouse { get; private set; }
    [field: SerializeField] public Vector3Int PositionInGrid { get; set; }
    public MaterialPropertyBlock Block { get; set; } = null!;

    public void Start()
    {
        Block ??= new MaterialPropertyBlock();
        if (Name == "") throw new InvalidOperationException(nameof(Name));
    }

    public void Initialize(string name, IndexOfPlayer player, BigInteger indexInHouse)
    {
        Name = name;
        Player = player;
        IndexInHouse = indexInHouse;
    }
}