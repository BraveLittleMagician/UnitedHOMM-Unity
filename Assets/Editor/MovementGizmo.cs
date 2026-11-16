#if UNITY_EDITOR
using System;
using UnityEngine;
#endif

public sealed class MovementGizmo : MonoBehaviour
{
    public Func<ISequenceEnumerator<Square>> Factory;
    public Vector3 WorldOrigin;
    public float CellSize = 1f;
    public bool DrawInPlayMode = true;

    private void OnDrawGizmos()
    {
        if (!DrawInPlayMode && Application.isPlaying) return;
        if (Factory == null) return;

        var e = Factory();
        foreach (var kv in e.PositionsAll)
        {
            var p = WorldOrigin + new Vector3(kv.Key.X * CellSize, 0, kv.Key.Y * CellSize);
            Gizmos.color = kv.Value == Stayable.Stay
                ? new Color(0f, 1f, 0f, 0.35f)
                : new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Gizmos.DrawCube(p, Vector3.one * CellSize * 0.9f);
        }
        foreach (var kv in e.PositionsAttackable)
        {
            if (kv.Value != Attackable.Attack) continue;
            var p = WorldOrigin + new Vector3(kv.Key.X * CellSize, 0, kv.Key.Y * CellSize);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Gizmos.DrawSphere(p, CellSize * 0.3f);
        }
    }
}