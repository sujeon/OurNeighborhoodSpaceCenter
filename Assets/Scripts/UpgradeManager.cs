using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("업그레이드 레벨")]
    public int rangeLevel = 1;
    public int weightLevel = 1;
    public int trajectoryLevel = 1;
    public int pointEffLevel = 1;
    public int autoCollectLevel = 0; // 0은 미보유 상태

    void Awake() { Instance = this; }

    // 예시: 사거리 업그레이드 함수 (물리 자원 소모)
    public void UpgradeRange()
    {
        int cost = rangeLevel * 10; // 레벨당 비용 증가
        if (ResourceManager.Instance.physicsRes >= cost)
        {
            ResourceManager.Instance.AddResource("Physics", -cost);
            rangeLevel++;
            Debug.Log("사거리 업그레이드 완료!");
        }
    }

    // 다음 스테이지 해금 (모든 자원이 50개 이상일 때)
    public bool CanUnlockNextStage()
    {
        return ResourceManager.Instance.physicsRes >= 50 && 
               ResourceManager.Instance.chemistryRes >= 50; 
    }
}