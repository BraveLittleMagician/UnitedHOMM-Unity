#nullable enable

using UnityEngine;

public sealed class NoopObjectInjector : IObjectInjector
{
    public void InjectGameObject(GameObject go) { }
}