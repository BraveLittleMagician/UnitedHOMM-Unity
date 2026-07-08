#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DontDestroyOnLoad : MonoBehaviour
{
    private GlobalDeselector _deselector = null!;
    private Camera _camera = null!;

    private void Awake()
    {
        _deselector = GetComponentInChildren<GlobalDeselector>() ?? throw new NullReferenceException(nameof(_deselector));
        _camera = GetComponentInChildren<Camera>() ?? throw new NullReferenceException(nameof(_camera));
        DontDestroyOnLoad(gameObject);
    }
}