#nullable enable

using UnityEngine;

public interface IDeltaSubscriber : IMouseEventSubscriber
{
    void Delta(Vector2 delta);
}