#nullable enable

using UnityEngine;

public interface IPointerHub
{
    void RegisterSubscriber(IPointerSubscriber sub);
    void UnregisterSubscriber(IPointerSubscriber sub);
    void TriggerPoint(Vector2 pos);
}