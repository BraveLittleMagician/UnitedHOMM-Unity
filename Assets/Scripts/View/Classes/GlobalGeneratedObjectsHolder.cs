#nullable enable

using UnityEngine;

public class GlobalGeneratedObjectsHolder : MonoBehaviour
{
    public static GlobalGeneratedObjectsHolder Instance { get; private set; } = null!;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
