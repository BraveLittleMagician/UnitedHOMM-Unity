#nullable enable

using System.Collections.Generic;
using UnityEngine;

public record BoardLayoutDto(Vector3Int GridSize, IReadOnlyList<PlacementOfPiece> Placements);