#nullable enable

using UnityEngine;

public class AttackMarker : MonoBehaviour, IHoverable
{
    private MaterialPropertyBlock _block = null!;

    public Square Cell { get; private set; }
    public bool IsHovered { get; private set; }

    public void Initialize(Square cell, MaterialPropertyBlock block)
    {
        Cell = cell;
        _block = block;
        ApplyBlock();
    }

    public void SetHovered(bool value)
    {
        if (IsHovered == value) return;
        IsHovered = value;
        ApplyBlock();
    }

    private void ApplyBlock()
    {
        if (_block == null) return;
        _block.SetFloat("_IsHovered", IsHovered ? 1f : 0f);
        _block.SetFloat("_IsStay", 0f);
        _block.SetFloat("_IsAttack", 1f);
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(_block);
    }
}