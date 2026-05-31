using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageMoveManager : MonoBehaviour
{
    [Header("씬 이름")]
    [SerializeField] private string firstStageSceneName = "FirstStage";
    [SerializeField] private string moonStageSceneName = "MoonStage";

    [Header("버튼")]
    [SerializeField] private Button goToMoonButton;
    [SerializeField] private Button goToFirstStageButton;

    [Header("현재 씬 설정")]
    [SerializeField] private bool thisIsFirstStage = false;
    [SerializeField] private bool thisIsMoonStage = false;

    [Header("설정")]
    [SerializeField] private bool saveBeforeMove = true;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        SetupButtons();
        UpdateButtons();
    }

    private void OnEnable()
    {
        UpdateButtons();
    }

    private void SetupButtons()
    {
        if (goToMoonButton != null)
        {
            goToMoonButton.onClick.RemoveAllListeners();
            goToMoonButton.onClick.AddListener(GoToMoonStage);
        }

        if (goToFirstStageButton != null)
        {
            goToFirstStageButton.onClick.RemoveAllListeners();
            goToFirstStageButton.onClick.AddListener(GoToFirstStage);
        }
    }

    public void UpdateButtons()
    {
        // 1스테이지: 달 해금 후에만 달 이동 버튼 표시
        if (goToMoonButton != null)
        {
            bool showMoonButton = thisIsFirstStage && UpgradeManager.IsMoonUnlocked;

            goToMoonButton.gameObject.SetActive(showMoonButton);
            goToMoonButton.interactable = showMoonButton;
        }

        // 2스테이지: 1스테이지 이동 버튼은 항상 표시
        if (goToFirstStageButton != null)
        {
            bool showFirstStageButton = thisIsMoonStage;

            goToFirstStageButton.gameObject.SetActive(showFirstStageButton);
            goToFirstStageButton.interactable = showFirstStageButton;
        }
    }

    public void GoToMoonStage()
    {
        if (!UpgradeManager.IsMoonUnlocked)
        {
            if (GameLogUI.Instance != null)
                GameLogUI.Instance.Show("아직 달 스테이지가 해금되지 않았습니다.");

            return;
        }

        SaveIfPossible(moonStageSceneName);

        Time.timeScale = 1f;

        if (GameLogUI.Instance != null)
            GameLogUI.Instance.Show("달 스테이지로 이동합니다.");

        SceneManager.LoadScene(moonStageSceneName);
    }

    public void GoToFirstStage()
    {
        SaveIfPossible(firstStageSceneName);

        Time.timeScale = 1f;

        if (GameLogUI.Instance != null)
            GameLogUI.Instance.Show("1스테이지로 돌아갑니다.");

        SceneManager.LoadScene(firstStageSceneName);
    }

    private void SaveIfPossible(string targetSceneName)
    {
        if (!saveBeforeMove)
            return;

        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.SaveGameWithSceneName(targetSceneName);
    }
}