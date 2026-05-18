using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI 패널 연결")]
    public GameObject researchPanel;
    public GameObject resourceHUD;

    [Header("설정")]
    public bool pauseGameWhenOpen = true;
    public bool keepResourceHudVisible = true;

    private void Start()
    {
        SetPanel(false);
    }

    private void Update()
    {
        if (researchPanel != null && researchPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleUpgradeUI();
        }
    }

    public void ToggleUpgradeUI()
    {
        if (researchPanel == null) return;
        SetPanel(!researchPanel.activeSelf);
    }

    public void SetPanel(bool active)
    {
        if (researchPanel != null) researchPanel.SetActive(active);

        if (resourceHUD != null)
        {
            resourceHUD.SetActive(keepResourceHudVisible || !active);
        }

        if (active && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.UpdateUpgradeUI();
        }

        if (pauseGameWhenOpen)
        {
            Time.timeScale = active ? 0f : 1f;
        }
    }

    private void OnDisable()
    {
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
        }
    }
}
