#nullable enable

using NUnit.Framework;

public sealed class SquareInfoTests
{
    [Test]
    public void CanStopAt_TrueOnlyForStay()
    {
        var stay = new SquareInfo(Stayable.Stay, Attackable.NotAttack);
        var notStay = new SquareInfo(Stayable.NotStay, Attackable.NotAttack);
        Assert.That(stay.CanStopAt, Is.True);
        Assert.That(notStay.CanStopAt, Is.False);
    }

    [Test]
    public void IsAttackable_TrueOnlyForAttack()
    {
        var atk = new SquareInfo(Stayable.Stay, Attackable.Attack);
        var notAtk = new SquareInfo(Stayable.Stay, Attackable.NotAttack);
        Assert.That(atk.IsAttackable, Is.True);
        Assert.That(notAtk.IsAttackable, Is.False);
    }

    [Test]
    public void Equality_ByBothFields()
    {
        var a = new SquareInfo(Stayable.Stay, Attackable.Attack);
        var b = new SquareInfo(Stayable.Stay, Attackable.Attack);
        var c = new SquareInfo(Stayable.NotStay, Attackable.Attack);
        Assert.That(a == b, Is.True);
        Assert.That(a == c, Is.False);
    }
}