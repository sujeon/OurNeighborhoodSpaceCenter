using UnityEngine;
using TMPro; // 텍스트 UI를 쓰기 위해 필요합니다.

public class ResourceManager : MonoBehaviour
{
    // 어디서든 접근할 수 있게 싱글톤으로 만듭니다.
    public static ResourceManager Instance;

    [Header("자원 데이터")]
    public int physicsRes = 0;   // 물리 (산스장)
    public int chemistryRes = 0; // 화학 (붕어빵)
    public int biologyRes = 0;   // 생물 (약수터)
    public int earthRes = 0;     // 지구과학 (솟대)

    [Header("UI 연결")]
    public TextMeshProUGUI resText; // 화면에 자원을 표시할 텍스트

    void Awake()
    {
        Instance = this;
    }

    // 자원을 추가하는 함수
    public void AddResource(string type, int amount)
    {
        switch (type)
        {
            case "Physics": physicsRes += amount; break;
            case "Chemistry": chemistryRes += amount; break;
            case "Biology": biologyRes += amount; break;
            case "Earth": earthRes += amount; break;
        }
        UpdateUI();
    }
    public void AddAllResources(int amount)
    {
        physicsRes += amount;
        chemistryRes += amount;
        biologyRes += amount;
        earthRes += amount;
        UpdateUI();
        Debug.Log($"보너스 구역 안착! 모든 자원 +{amount}");
    }

    public void UpdateUI()
    {
        resText.text = $"물리:{physicsRes} 화학:{chemistryRes} 생물:{biologyRes} 지구:{earthRes}";
    }
}
