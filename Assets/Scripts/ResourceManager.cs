using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 필수

public class ResourceManager : MonoBehaviour
{
    // 다른 스크립트에서 접근할 수 있도록 싱글톤 설정
    public static ResourceManager Instance;

    [Header("자원 수치")]
    public int physicsRes = 0;
    public int chemistryRes = 0;
    public int biologyRes = 0;
    public int earthRes = 0;

    [Header("UI 개별 텍스트 연결 (TMP)")]
    public TextMeshProUGUI physicsText;
    public TextMeshProUGUI chemistryText;
    public TextMeshProUGUI biologyText;
    public TextMeshProUGUI earthText;

    void Awake()
    {
        // 씬에 리소스 매니저가 하나만 존재하도록 보장
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI(); // 시작 시 0점으로 초기화 표시
    }

    // [함수 1] 특정 한 종류의 자원만 추가할 때 (일반 연구소 안착 시)
    public void AddResource(string type, int amount)
    {
        if (type.Contains("Physics")) physicsRes += amount;
        else if (type.Contains("Chemistry")) chemistryRes += amount;
        else if (type.Contains("Biology")) biologyRes += amount;
        else if (type.Contains("Earth")) earthRes += amount;

        UpdateUI();
    }

    // ★ [함수 2] 모든 자원을 동시에 추가할 때 (산 꼭대기 안착 시)
    public void AddAllResources(int amount)
    {
        physicsRes += amount;
        chemistryRes += amount;
        biologyRes += amount;
        earthRes += amount;

        Debug.Log($"<color=yellow>산 꼭대기 정복! 모든 자원 +{amount} 획득!</color>");
        UpdateUI();
    }

    // UI 텍스트를 실시간 데이터로 갱신
    public void UpdateUI()
    {
        if (physicsText != null) physicsText.text = physicsRes.ToString();
        if (chemistryText != null) chemistryText.text = chemistryRes.ToString();
        if (biologyText != null) biologyText.text = biologyRes.ToString();
        if (earthText != null) earthText.text = earthRes.ToString();
    }
}