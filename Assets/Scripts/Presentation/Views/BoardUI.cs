#nullable enable

using UnityEngine;

public class BoardUI : MonoBehaviour
{
    private StateOfGame _stateOfGame = null!;


    public void Initialize(StateOfGame stateManager)
    {
        _stateOfGame = stateManager;
    }

    private void OnGUI()
    {
        if (_stateOfGame == null) return;
    }
}