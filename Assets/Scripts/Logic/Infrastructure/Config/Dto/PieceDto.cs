#nullable enable
using System.Collections.Generic;

public record PieceDto(string Name, string PathToPrefab)
{
    public int Hp { get; init; } = 1;
    public int AttackDamage { get; init; } = 1;
    public int AttackCharges { get; init; } = 1;
    public string? MoveShape { get; init; }
    public string? AttackShape { get; init; }
    public IReadOnlyList<ModifierDto>? Modifiers { get; init; }
}