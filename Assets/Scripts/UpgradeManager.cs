using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("참조 스크립트 연결")]
    public ProjectileLauncher launcher; 
    public ResourceManager resManager;  

    // --- [전역 적용 스태틱 변수] ---
    public static float ProjectileMass = 1.0f;     
    public static int BonusAmount = 0;             // ★ Bullet.cs에서 이 이름을 사용합니다.
    public static float BioSpeedMultiplier = 1.0f; 
    public static bool IsMoonUnlocked = false;    

    [Header("1. 대포 최대 파워 (물리 + 지구과학)")]
    public int rangeLevel = 1;      // ★ ProjectileLauncher.cs와 이름 통일
    public int powerCostPhys = 10;
    public int powerCostEarth = 5;
    public TextMeshProUGUI powerText;

    [Header("2. 관측 장비 경량화 (화학 + 물리)")]
    public int weightLevel = 1;     // ★ ProjectileLauncher.cs와 이름 통일
    public int weightCostChem = 15;
    public int weightCostPhys = 5;
    public TextMeshProUGUI weightText;

    [Header("3. 궤적 예측 장비 (지구과학 + 화학)")]
    public int trajectoryLevel = 1; // ★ ProjectileLauncher.cs와 이름 통일
    public int trajectoryCostEarth = 10;
    public int trajectoryCostChem = 10;
    public TextMeshProUGUI trajectoryText;

    [Header("4. 안착 점수 효율 (물리 + 지구과학)")]
    public int scoreLevel = 1;
    public int scoreCostPhys = 20;
    public int scoreCostEarth = 20;
    public TextMeshProUGUI scoreText;

    [Header("5. 자동 생산 속도 (생물)")]
    public int bioLevel = 1;
    public int bioCostBio = 15;
    public TextMeshProUGUI bioText;

    [Header("6. 달 스테이지 해금")]
    public int reqRangeLevelForMoon = 5; 
    public int reqWeightLevelForMoon = 3; 
    public int moonCostAll = 50; 
    public Button moonStageButton; 
    public TextMeshProUGUI moonText;
    public GameObject missionPopup;       

    void Awake() { if (Instance == null) Instance = this; }
    void Start() { UpdateUpgradeUI(); }

    public void UpgradeMaxPower()
    {
        if (resManager.physicsRes >= powerCostPhys && resManager.earthRes >= powerCostEarth)
        {
            resManager.physicsRes -= powerCostPhys;
            resManager.earthRes -= powerCostEarth;
            rangeLevel++;
            launcher.maxForce += 5f; // 대포의 maxForce 직접 수정
            powerCostPhys = Mathf.RoundToInt(powerCostPhys * 1.5f);
            powerCostEarth = Mathf.RoundToInt(powerCostEarth * 1.5f);
            FinishUpgrade();
        }
    }

    public void UpgradeWeight()
    {
        if (resManager.chemistryRes >= weightCostChem && resManager.physicsRes >= weightCostPhys)
        {
            resManager.chemistryRes -= weightCostChem;
            resManager.physicsRes -= weightCostPhys;
            weightLevel++;
            ProjectileMass = Mathf.Max(0.4f, 1.0f - (weightLevel * 0.05f));
            weightCostChem = Mathf.RoundToInt(weightCostChem * 1.6f);
            weightCostPhys = Mathf.RoundToInt(weightCostPhys * 1.4f);
            FinishUpgrade();
        }
    }

    public void UpgradeTrajectory()
    {
        if (resManager.earthRes >= trajectoryCostEarth && resManager.chemistryRes >= trajectoryCostChem)
        {
            resManager.earthRes -= trajectoryCostEarth;
            resManager.chemistryRes -= trajectoryCostChem;
            trajectoryLevel++;
            launcher.trajectoryStepCount += 10; // 대포의 궤적 길이 직접 수정
            trajectoryCostEarth = Mathf.RoundToInt(trajectoryCostEarth * 1.5f);
            trajectoryCostChem = Mathf.RoundToInt(trajectoryCostChem * 1.5f);
            FinishUpgrade();
        }
    }

    public void UpgradeScoreEfficiency()
    {
        if (resManager.physicsRes >= scoreCostPhys && resManager.earthRes >= scoreCostEarth)
        {
            resManager.physicsRes -= scoreCostPhys;
            resManager.earthRes -= scoreCostEarth;
            scoreLevel++;
            BonusAmount += 2; // 스태틱 변수 업데이트
            scoreCostPhys = Mathf.RoundToInt(scoreCostPhys * 1.7f);
            scoreCostEarth = Mathf.RoundToInt(scoreCostEarth * 1.7f);
            FinishUpgrade();
        }
    }

    public void UpgradeBioSpeed()
    {
        if (resManager.biologyRes >= bioCostBio)
        {
            resManager.biologyRes -= bioCostBio;
            bioLevel++;
            BioSpeedMultiplier = Mathf.Max(0.2f, 1.0f - (bioLevel * 0.1f));
            bioCostBio = Mathf.RoundToInt(bioCostBio * 1.8f);
            FinishUpgrade();
        }
    }

    public void UpgradeUnlockMoon()
    {
        bool isLevelMet = rangeLevel >= reqRangeLevelForMoon && weightLevel >= reqWeightLevelForMoon;
        bool hasResources = resManager.physicsRes >= moonCostAll && resManager.chemistryRes >= moonCostAll &&
                            resManager.biologyRes >= moonCostAll && resManager.earthRes >= moonCostAll;

        if (!IsMoonUnlocked && isLevelMet && hasResources)
        {
            resManager.physicsRes -= moonCostAll;
            resManager.chemistryRes -= moonCostAll;
            resManager.biologyRes -= moonCostAll;
            resManager.earthRes -= moonCostAll;
            IsMoonUnlocked = true;
            if (moonStageButton != null) moonStageButton.interactable = true;
            if (missionPopup != null) missionPopup.SetActive(true);
            FinishUpgrade();
        }
    }

    void FinishUpgrade() { resManager.UpdateUI(); UpdateUpgradeUI(); }

    public void UpdateUpgradeUI()
    {
        SetText(powerText, rangeLevel, $"물리:{powerCostPhys} / 지구:{powerCostEarth}");
        SetText(weightText, weightLevel, $"화학:{weightCostChem} / 물리:{weightCostPhys}");
        SetText(trajectoryText, trajectoryLevel, $"지구:{trajectoryCostEarth} / 화학:{trajectoryCostChem}");
        SetText(scoreText, scoreLevel, $"물리:{scoreCostPhys} / 지구:{scoreCostEarth}");
        SetText(bioText, bioLevel, $"생물:{bioCostBio}");
        if (moonText != null) moonText.text = IsMoonUnlocked ? "[해금 완료]" : $"각 {moonCostAll}개 필요";
    }

    void SetText(TextMeshProUGUI tmp, int lv, string costs)
    {
        if (tmp == null) return;
        tmp.text = $"<b>Lv.{lv}</b>\n<size=70%>{costs}</size>";
    }
}