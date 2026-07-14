#nullable enable

using UnityEngine;
using VContainer;
using System;

[RequireComponent(typeof(GameLifetimeScope))]
public class BoardResizer : MonoBehaviour
{
    [SerializeField] private int _newSize = 10;

    private BoardConfigUpdater _configManager = null!;
    private BoardConfig _currentConfig = null!;

    private void Start()
    {
        var scope = GetComponent<GameLifetimeScope>() ?? throw new NullReferenceException(nameof(GameLifetimeScope));

        var container = scope.Container;
        _configManager = container.Resolve<BoardConfigUpdater>();
        _currentConfig = container.Resolve<BoardConfig>();
    }
}