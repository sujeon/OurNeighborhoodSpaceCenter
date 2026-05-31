using UnityEngine;

public class SceneLoadHandler : MonoBehaviour
{
    [SerializeField] private bool loadOnStart = true;

    private void Start()
    {
        if (!loadOnStart)
            return;

        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
            SaveManager.Instance.LoadGame();
    }
}
