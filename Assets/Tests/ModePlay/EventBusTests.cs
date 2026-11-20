#nullable enable

using NUnit.Framework;
using System;

public sealed class EventBusTests
{
    private interface ITestEvent { }
    private sealed class TestEvent { public int Value; }
    private sealed class TestInterfaceEvent : ITestEvent { }
    private sealed class OtherEvent { }

    private sealed class NullLogger : ILogger
    {
        public void Log(string message, LogLevel level) { }
        public void LogDebug(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
    }

    [Test]
    public void Subscribe_Publish_DeliversEvent()
    {
        var bus = new EventBus(new NullLogger());
        int received = 0;
        bus.Subscribe<TestEvent>(e => received = e.Value);

        bus.Publish(new TestEvent { Value = 42 });

        Assert.That(received, Is.EqualTo(42));
    }

    [Test]
    public void Unsubscribe_StopsDelivery()
    {
        var bus = new EventBus(new NullLogger());
        int count = 0;
        Action<TestEvent> handler = _ => count++;
        bus.Subscribe(handler);
        bus.Unsubscribe(handler);

        bus.Publish(new TestEvent());

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void DisposeOfSubscription_StopsDelivery()
    {
        var bus = new EventBus(new NullLogger());
        int count = 0;
        var sub = bus.Subscribe<TestEvent>(_ => count++);
        sub.Dispose();

        bus.Publish(new TestEvent());

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void DuplicateSubscription_OnlyDeliveredOnce()
    {
        var bus = new EventBus(new NullLogger());
        int count = 0;
        Action<TestEvent> handler = _ => count++;
        bus.Subscribe(handler);
        bus.Subscribe(handler);

        bus.Publish(new TestEvent());

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void InterfaceSubscription_ReceivesImplementation()  // BASELINE CHECK
    {
        var bus = new EventBus(new NullLogger());
        int count = 0;
        bus.Subscribe<ITestEvent>(_ => count++);

        bus.Publish(new TestInterfaceEvent());

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void UnrelatedEvent_DoesNotFireHandler()
    {
        var bus = new EventBus(new NullLogger());
        int count = 0;
        bus.Subscribe<TestEvent>(_ => count++);

        bus.Publish(new OtherEvent());

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void HandlerException_DoesNotStopOtherHandlers()
    {
        var bus = new EventBus(new NullLogger());
        int received = 0;
        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("test"));
        bus.Subscribe<TestEvent>(_ => received = 1);

        Assert.DoesNotThrow(() => bus.Publish(new TestEvent()));
        Assert.That(received, Is.EqualTo(1));
    }

    [Test]
    public void NullLogger_ImplementsAllMethodsUsedByLogging()  // sanity: interface shape matches
    {
        // Если этот тест не компилируется — интерфейс ILogger имеет другие методы.
        ILogger logger = new NullLogger();
        logger.Log("x");
        logger.LogDebug("x");
        logger.LogWarning("x");
        logger.LogError("x");
    }
}