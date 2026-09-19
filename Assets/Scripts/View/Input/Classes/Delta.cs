#nullable enable

using UnityEngine;

public class Delta : MouseEvent<IDeltaSubscriber>, IDeltaHub
{
    public void TriggerDelta(Vector2 delta)
    {
        Notify(s => s.Delta(delta));
    }
}