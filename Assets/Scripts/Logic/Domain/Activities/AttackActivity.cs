#nullable enable
using UnityEngine;

public sealed record AttackActivity(Vector3Int TargetCell) : Activity;