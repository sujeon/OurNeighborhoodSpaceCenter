using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtons : MonoBehaviour
{
    [Header("씬 이름")]
    [SerializeField] private string newGameSceneName = "FirstStage";

    [Header("버튼 사운드")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float sceneLoadDelay = 0.2f;

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        PlayButtonSound();

        yield return new WaitForSeconds(sceneLoadDelay);

        string targetScene = newGameSceneName;

        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            targetScene = SaveManager.Instance.GetSavedSceneName(newGameSceneName);
        }

        SceneManager.LoadScene(targetScene);
    }

    public void StartNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        SceneManager.LoadScene(newGameSceneName);
    }

    public void QuitGame()
    {
        PlayButtonSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayButtonSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }
}