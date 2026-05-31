using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject pausePanel;

    [Header("버튼")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeButton;

    [Header("설정")]
    [SerializeField] private bool pauseGameWhenOpen = true;
    [SerializeField] private bool saveBeforeQuit = true;

    private bool isOpen;

    private void Awake()
    {
        // 씬 시작 시 혹시 이전 씬에서 timeScale이 0으로 남아있으면 복구
        Time.timeScale = 1f;
    }

    private void Start()
    {
        SetupButtons();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void SetupButtons()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(SaveGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning("PauseMenuManager: pausePanel이 연결되지 않았습니다.");
            return;
        }

        pausePanel.SetActive(true);
        isOpen = true;

        if (pauseGameWhenOpen)
            Time.timeScale = 0f;

        if (GameLogUI.Instance != null)
            GameLogUI.Instance.Show("일시정지 메뉴를 열었습니다.");
    }

    public void CloseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        isOpen = false;

        if (pauseGameWhenOpen)
            Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();

            if (GameLogUI.Instance != null)
                GameLogUI.Instance.Show("게임 저장 완료");
        }
        else
        {
            Debug.LogWarning("SaveManager가 없습니다.");

            if (GameLogUI.Instance != null)
                GameLogUI.Instance.Show("저장 실패: SaveManager 없음");
        }
    }

    public void QuitGame()
    {
        if (saveBeforeQuit)
            SaveGame();

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}