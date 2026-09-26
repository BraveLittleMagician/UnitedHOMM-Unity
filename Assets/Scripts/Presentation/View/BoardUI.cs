#nullable enable

using UnityEngine;

public class BoardUI : MonoBehaviour
{
    private IStateOfGame _stateManager = null!;


    public void Initialize(IStateOfGame stateManager)
    {
        _stateManager = stateManager;
    }

    private void OnGUI()
    {
        if (_stateManager == null) return;
    }
}