#nullable enable

using NUnit.Framework;

public sealed class SquareInfoTests
{
    [Test]
    public void CanStopAt_TrueOnlyForStay()
    {
        var stay = new PositionInfo(Stayable.Stay, Attackable.NotAttack);
        var notStay = new PositionInfo(Stayable.NotStay, Attackable.NotAttack);
        Assert.That(stay.CanStopAt, Is.True);
        Assert.That(notStay.CanStopAt, Is.False);
    }

    [Test]
    public void IsAttackable_TrueOnlyForAttack()
    {
        var atk = new PositionInfo(Stayable.Stay, Attackable.Attack);
        var notAtk = new PositionInfo(Stayable.Stay, Attackable.NotAttack);
        Assert.That(atk.IsAttackable, Is.True);
        Assert.That(notAtk.IsAttackable, Is.False);
    }

    [Test]
    public void Equality_ByBothFields()
    {
        var a = new PositionInfo(Stayable.Stay, Attackable.Attack);
        var b = new PositionInfo(Stayable.Stay, Attackable.Attack);
        var c = new PositionInfo(Stayable.NotStay, Attackable.Attack);
        Assert.That(a == b, Is.True);
        Assert.That(a == c, Is.False);
    }
}