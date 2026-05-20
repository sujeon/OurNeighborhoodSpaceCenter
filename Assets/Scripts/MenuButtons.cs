using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private float sceneLoadDelay = 0.2f;

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        if (soundManager != null)
            soundManager.PlayButtonClick();

        yield return new WaitForSeconds(sceneLoadDelay);

        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index + 1);
    }

    public void QuitGame()
    {
        if (soundManager != null)
            soundManager.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
