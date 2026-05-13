using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI 패널 연결")]
    public GameObject researchPanel; 
    public GameObject resourceHUD;   

    void Start()
    {
        if (researchPanel != null) researchPanel.SetActive(false);
        if (resourceHUD != null) resourceHUD.SetActive(true);
    }

    void Update()
    {
        // 연구소 창이 켜져 있을 때 ESC를 누르면 닫기
        if (researchPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleUpgradeUI();
        }
    }       

    public void ToggleUpgradeUI()
    {
    if (researchPanel == null) return;

    bool isActive = !researchPanel.activeSelf;
    researchPanel.SetActive(isActive);

    // ★ 이 부분을 확인하세요! 
    // HUD를 패널 상태의 반대(!isActive)로 설정하고 있다면, 이 줄을 지우거나 주석 처리하세요.
    // if (resourceHUD != null) resourceHUD.SetActive(!isActive); 

    // 연구소 안에서도 자원을 봐야 하므로 항상 켜져 있도록 강제합니다.
    if (resourceHUD != null) resourceHUD.SetActive(true);

    if (isActive && UpgradeManager.Instance != null)
    {
        UpgradeManager.Instance.UpdateUpgradeUI();
    }

    // 시간 및 커서 설정...
    Time.timeScale = isActive ? 0f : 1f;
    }
}