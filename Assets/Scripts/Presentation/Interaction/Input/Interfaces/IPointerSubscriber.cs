#nullable enable

using UnityEngine;

public interface IPointerSubscriber : IMouseEventSubscriber
{
    void Point(Vector2 pointer);
}