#nullable enable

using UnityEngine;

public class BoardUI : MonoBehaviour
{
    private StateOfGame _stateOfGame = null!;


    public void Initialize(StateOfGame stateOfGame)
    {
        _stateOfGame = stateOfGame;
    }

    private void OnGUI()
    {
        if (_stateOfGame == null) return;
    }
}