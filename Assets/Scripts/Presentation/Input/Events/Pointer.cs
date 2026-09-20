#nullable enable

using UnityEngine;

public class Pointer : MouseEvent<IPointerSubscriber>, IPointerHub
{
    public void TriggerPoint(Vector2 pos)
    {
        Notify(s => s.Point(pos));
    }
}