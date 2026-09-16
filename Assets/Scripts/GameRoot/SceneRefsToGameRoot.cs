#nullable enable

using System;
using UnityEngine;

public sealed class SceneRefsToGameRoot : MonoBehaviour
{
    [SerializeField] private bool _autoStart = true;

    [Header("Scene Components")]
    [SerializeField] private GameView _gameView = null!;
    [SerializeField] private GridRenderer _gridRenderer = null!;
    [SerializeField] private ClickEventer _clickEventer = null!;
    [SerializeField] private ControllerOfCamera _cameraController = null!;
    [SerializeField] private UIService _uiService = null!;
    [SerializeField] private ButtonColors _buttonColors = null!;
    [SerializeField] private GlobalDeselector _globalDeselector = null!;
    [SerializeField] private GlobalGeneratedSurroundingsHolder _globalSurroundingsHolder = null!;
    [SerializeField] private GameObject _layerPrefab = null!;

    private GameRoot _composition = null!;

    private void ValidateReferences()
    {
        if (_gameView == null) throw new ArgumentNullException(nameof(_gameView));
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
        if (_clickEventer == null) throw new ArgumentNullException(nameof(_clickEventer));
        if (_cameraController == null) throw new ArgumentNullException(nameof(_cameraController));
        if (_uiService == null) throw new ArgumentNullException(nameof(_uiService));
        if (_buttonColors == null) throw new ArgumentNullException(nameof(_buttonColors));
        if (_globalDeselector == null) throw new ArgumentNullException(nameof(_globalDeselector));
        if (_globalSurroundingsHolder == null) throw new ArgumentNullException(nameof(_globalSurroundingsHolder));
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
    }
    private void Awake()
    {
        ValidateReferences();
        _composition = new(_globalDeselector, _buttonColors, _globalSurroundingsHolder, _uiService, _clickEventer, _cameraController, _gameView, _gridRenderer, _layerPrefab, _autoStart);
    }
    private void Start()
    {
        _composition.Start();
    }
    private void OnDestroy()
    {
        _composition.Dispose();
        _composition = null!;
    }
}