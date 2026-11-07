#nullable enable
using UnityEngine;

public sealed record MoveActivity(Vector3Int From, Vector3Int To) : Activity;
