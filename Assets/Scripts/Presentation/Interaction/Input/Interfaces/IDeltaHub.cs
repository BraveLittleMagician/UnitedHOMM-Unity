#nullable enable

using UnityEngine;

public interface IDeltaHub
{
    void RegisterSubscriber(IDeltaSubscriber sub);
    void UnregisterSubscriber(IDeltaSubscriber sub);
    void TriggerDelta(Vector2 delta);
}
