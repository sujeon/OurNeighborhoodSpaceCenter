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

    // ★ 이 함수를 연구소 버튼에 연결하세요.
    public void ToggleUpgradeUI()
    {
        if (researchPanel == null || resourceHUD == null) return;

        // 현재 상태의 반대를 설정 (켜져있으면 false, 꺼져있으면 true)
        bool isActive = !researchPanel.activeSelf;

        researchPanel.SetActive(isActive);
        
        // 자원 HUD는 연구소 창과 반대로 작동하게 함
        // (단, 버튼이 HUD 안에 있다면 HUD는 계속 켜두는 것이 좋습니다 - 아래 주의사항 참고)
        resourceHUD.SetActive(!isActive);

        // 시간 정지/재개 조절
        Time.timeScale = isActive ? 0f : 1f;

        Debug.Log(isActive ? "연구소 입장" : "필드로 복귀");
    }
}