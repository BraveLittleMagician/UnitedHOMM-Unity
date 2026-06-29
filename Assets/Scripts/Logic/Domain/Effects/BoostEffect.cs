#nullable enable

public class BoostEffect : OneFieldEffect
{
    public BoostEffect(int amount) : base(new AddOperation(amount)) { }
}