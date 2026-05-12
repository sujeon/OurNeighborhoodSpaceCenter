using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("참조 스크립트 연결")]
    public ProjectileLauncher launcher; // 대포 성능 수정용
    public ResourceManager resManager;  // 자원 데이터 참조용

    // --- [전역 적용 스태틱 변수] ---
    // 다른 스크립트(Bullet, LabGenerator 등)에서 이 값을 참조하게 됩니다.
    public static float ProjectileMass = 1.0f;     // 포탄 무게
    public static int BonusResourceAmount = 0;    // 안착 보너스
    public static float BioSpeedMultiplier = 1.0f; // 생산 속도 배율 (작을수록 빠름)
    public static bool IsMoonUnlocked = false;    // 달 스테이지 해금 여부

    [Header("1. 대포 최대 파워 (물리 + 지구과학)")]
    public int powerLv = 1;
    public int powerCostPhys = 10;
    public int powerCostEarth = 5;
    public TextMeshProUGUI powerText;

    [Header("2. 관측 장비 경량화 (화학 + 물리)")]
    public int weightLv = 1;
    public int weightCostChem = 15;
    public int weightCostPhys = 5;
    public TextMeshProUGUI weightText;

    [Header("3. 궤적 예측 장비 (지구과학 + 화학)")]
    public int trajectoryLv = 1;
    public int trajectoryCostEarth = 10;
    public int trajectoryCostChem = 10;
    public TextMeshProUGUI trajectoryText;

    [Header("4. 안착 점수 효율 (물리 + 지구과학)")]
    public int scoreLv = 1;
    public int scoreCostPhys = 20;
    public int scoreCostEarth = 20;
    public TextMeshProUGUI scoreText;

    [Header("5. 자동 생산 속도 (생물)")]
    public int bioLv = 1;
    public int bioCostBio = 15;
    public TextMeshProUGUI bioText;

    [Header("6. 달 스테이지 해금 (복합 자원)")]
    public int moonCostAll = 50; // 모든 자원 50개씩
    public Button moonStageButton; 
    public TextMeshProUGUI moonText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateUpgradeUI(); // 초기 텍스트 세팅
    }

    // --- [업그레이드 함수들] ---

    // 1. 대포 파워 업
    public void UpgradeMaxPower()
    {
        if (resManager.physicsRes >= powerCostPhys && resManager.earthRes >= powerCostEarth)
        {
            resManager.physicsRes -= powerCostPhys;
            resManager.earthRes -= powerCostEarth;

            powerLv++;
            launcher.maxForce += 5f; // 실제 대포 파워 증가

            // 비용 지수적 증가 (1.5배)
            powerCostPhys = Mathf.RoundToInt(powerCostPhys * 1.5f);
            powerCostEarth = Mathf.RoundToInt(powerCostEarth * 1.5f);

            FinishUpgrade();
        }
    }

    // 2. 장비 경량화
    public void UpgradeWeight()
    {
        if (resManager.chemistryRes >= weightCostChem && resManager.physicsRes >= weightCostPhys)
        {
            resManager.chemistryRes -= weightCostChem;
            resManager.physicsRes -= weightCostPhys;

            weightLv++;
            // 포탄 무게 감소 (최소 0.4)
            ProjectileMass = Mathf.Max(0.4f, ProjectileMass - 0.05f);

            weightCostChem = Mathf.RoundToInt(weightCostChem * 1.6f);
            weightCostPhys = Mathf.RoundToInt(weightCostPhys * 1.4f);

            FinishUpgrade();
        }
    }

    // 3. 궤적 예측 업그레이드
    public void UpgradeTrajectory()
    {
        if (resManager.earthRes >= trajectoryCostEarth && resManager.chemistryRes >= trajectoryCostChem)
        {
            resManager.earthRes -= trajectoryCostEarth;
            resManager.chemistryRes -= trajectoryCostChem;

            trajectoryLv++;
            launcher.trajectoryStepCount += 10; // 점 개수 늘려서 더 멀리 예측

            trajectoryCostEarth = Mathf.RoundToInt(trajectoryCostEarth * 1.5f);
            trajectoryCostChem = Mathf.RoundToInt(trajectoryCostChem * 1.5f);

            FinishUpgrade();
        }
    }

    // 4. 점수 효율 증가
    public void UpgradeScoreEfficiency()
    {
        if (resManager.physicsRes >= scoreCostPhys && resManager.earthRes >= scoreCostEarth)
        {
            resManager.physicsRes -= scoreCostPhys;
            resManager.earthRes -= scoreCostEarth;

            scoreLv++;
            BonusResourceAmount += 2; // 안착 시 기본 3점 외 보너스 누적

            scoreCostPhys = Mathf.RoundToInt(scoreCostPhys * 1.7f);
            scoreCostEarth = Mathf.RoundToInt(scoreCostEarth * 1.7f);

            FinishUpgrade();
        }
    }

    // 5. 자동 생산 속도 향상
    public void UpgradeBioSpeed()
    {
        if (resManager.biologyRes >= bioCostBio)
        {
            resManager.biologyRes -= bioCostBio;

            bioLv++;
            BioSpeedMultiplier *= 0.8f; // 생산 간격 20%씩 단축

            bioCostBio = Mathf.RoundToInt(bioCostBio * 1.8f);

            FinishUpgrade();
        }
    }

    // 6. 달 스테이지 해금
    public void UpgradeUnlockMoon()
    {
        if (!IsMoonUnlocked && 
            resManager.physicsRes >= moonCostAll && 
            resManager.chemistryRes >= moonCostAll &&
            resManager.biologyRes >= moonCostAll &&
            resManager.earthRes >= moonCostAll)
        {
            resManager.physicsRes -= moonCostAll;
            resManager.chemistryRes -= moonCostAll;
            resManager.biologyRes -= moonCostAll;
            resManager.earthRes -= moonCostAll;

            IsMoonUnlocked = true;
            if (moonStageButton != null) moonStageButton.interactable = true;

            FinishUpgrade();
        }
    }

    // 공통 마무리 처리
    void FinishUpgrade()
    {
        resManager.UpdateUI();   // 상단 자원바 갱신
        UpdateUpgradeUI();       // 업그레이드 창 텍스트 갱신
        Debug.Log("연구 완료!");
    }

    // --- [UI 텍스트 실시간 업데이트] ---
    public void UpdateUpgradeUI()
    {
        SetText(powerText, powerLv, $"물리:{powerCostPhys} / 지구:{powerCostEarth}");
        SetText(weightText, weightLv, $"화학:{weightCostChem} / 물리:{weightCostPhys}");
        SetText(trajectoryText, trajectoryLv, $"지구:{trajectoryCostEarth} / 화학:{trajectoryCostChem}");
        SetText(scoreText, scoreLv, $"물리:{scoreCostPhys} / 지구:{scoreCostEarth}");
        SetText(bioText, bioLv, $"생물:{bioCostBio}");
        
        if (moonText != null)
        {
            moonText.text = IsMoonUnlocked ? "<color=yellow>[해금 완료]</color>" : $"모든 자원 {moonCostAll}개 필요";
        }
    }

    // 텍스트 색상 및 내용 설정 도우미
    void SetText(TextMeshProUGUI tmp, int lv, string costs)
    {
        if (tmp == null) return;
        tmp.text = $"<b>Lv.{lv}</b>\n<size=70%>{costs}</size>";
    }
}