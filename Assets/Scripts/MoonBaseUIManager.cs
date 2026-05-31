using UnityEngine;

public class MoonBaseUIManager : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject moonPurchasePanel;
    [SerializeField] private GameObject moonConstructionPanel;

    [Header("게임 일시정지")]
    [SerializeField] private bool pauseGameWhenOpen = true;

    private void Start()
    {
        CloseAllPanels();
    }

    public void OpenPurchasePanel()
    {
        if (moonPurchasePanel != null)
            moonPurchasePanel.SetActive(true);

        if (moonConstructionPanel != null)
            moonConstructionPanel.SetActive(false);

        SetPause(true);
        GameLogUI.Log("달 기지 구매 UI를 열었습니다.");
    }

    public void OpenConstructionPanel()
    {
        if (MoonBuildManager.Instance != null)
            MoonBuildManager.Instance.OpenConstructionPanel();

        if (moonPurchasePanel != null)
            moonPurchasePanel.SetActive(false);

        if (moonConstructionPanel != null)
            moonConstructionPanel.SetActive(true);

        if (MoonBuildManager.Instance != null)
            MoonBuildManager.Instance.RefreshConstructionUI();

        SetPause(true);
        GameLogUI.Log("기지 건설 UI를 열었습니다.");
    }

    public void BackToPurchasePanel()
    {
        if (moonPurchasePanel != null)
            moonPurchasePanel.SetActive(true);

        if (moonConstructionPanel != null)
            moonConstructionPanel.SetActive(false);

        SetPause(true);
    }

    public void CloseAllPanels()
    {
        if (moonPurchasePanel != null)
            moonPurchasePanel.SetActive(false);

        if (moonConstructionPanel != null)
            moonConstructionPanel.SetActive(false);

        SetPause(false);
    }

    private void SetPause(bool pause)
    {
        if (!pauseGameWhenOpen)
            return;

        Time.timeScale = pause ? 0f : 1f;
    }
}
