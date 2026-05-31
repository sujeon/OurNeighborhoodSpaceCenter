using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("참조 스크립트 연결")]
    public ProjectileLauncher launcher;
    public ResourceManager resManager;

    public static float ProjectileMass = 1.0f;
    public static int BonusAmount = 0;
    public static bool IsMoonUnlocked = false;

    [Header("대포 기본값")]
    [SerializeField] private float baseMaxForce = 10f;
    [SerializeField] private int baseTrajectoryStepCount = 50;
    [SerializeField] private float baseTrajectoryTimeStep = 0.05f;

    [Header("현재 적용 중인 배율")]
    [SerializeField] private float projectileGravityScale = 1.0f;
    [SerializeField] private float bioSpeedMultiplier = 1.0f;

    public float ProjectileGravityScale => projectileGravityScale;
    public float BioSpeedMultiplier => bioSpeedMultiplier;

    [Header("1. 대포 최대 파워 (물리 + 지구과학)")]
    public int rangeLevel = 1;
    public int powerCostPhys = 10;
    public int powerCostEarth = 5;
    public TextMeshProUGUI powerText;

    [Header("2. 관측 장비 경량화 / 중력 저항 감소 (화학 + 물리)")]
    public int weightLevel = 1;
    public int weightCostChem = 15;
    public int weightCostPhys = 5;
    public TextMeshProUGUI weightText;

    [Header("3. 궤적 예측 장비 (지구과학 + 화학)")]
    public int trajectoryLevel = 1;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        EnsureResourceManager();
        ApplyUpgradeToLauncher(launcher);
        UpdateUpgradeUI();
    }

    private void EnsureResourceManager()
    {
        if (resManager == null)
            resManager = ResourceManager.Instance;
    }

    public void ApplyUpgradeToLauncher(ProjectileLauncher targetLauncher)
    {
        if (targetLauncher == null) return;

        launcher = targetLauncher;
        launcher.maxForce = baseMaxForce + ((rangeLevel - 1) * 5f);
        launcher.trajectoryStepCount = Mathf.Min(200, baseTrajectoryStepCount + ((trajectoryLevel - 1) * 10));
        launcher.timeStep = Mathf.Max(0.035f, baseTrajectoryTimeStep - ((trajectoryLevel - 1) * 0.002f));

        if (launcher.powerSlider != null)
            launcher.powerSlider.maxValue = launcher.maxForce;
    }

    public void UpgradeMaxPower()
    {
        EnsureResourceManager();
        if (resManager == null || launcher == null)
        {
            GameLogUI.Warning("업그레이드 실패: 대포 또는 자원 매니저 연결이 없습니다.");
            return;
        }

        if (!resManager.SpendPair(ResourceType.Physics, powerCostPhys, ResourceType.Earth, powerCostEarth))
            return;

        rangeLevel++;
        ApplyUpgradeToLauncher(launcher);
        powerCostPhys = ScaleCost(powerCostPhys, 1.5f);
        powerCostEarth = ScaleCost(powerCostEarth, 1.5f);

        FinishUpgrade("대포 최대 파워", rangeLevel);
    }

    public void UpgradeWeight()
    {
        EnsureResourceManager();
        if (resManager == null)
        {
            GameLogUI.Warning("업그레이드 실패: 자원 매니저 연결이 없습니다.");
            return;
        }

        if (!resManager.SpendPair(ResourceType.Chemistry, weightCostChem, ResourceType.Physics, weightCostPhys))
            return;

        weightLevel++;
        ProjectileMass = Mathf.Max(0.4f, 1.0f - ((weightLevel - 1) * 0.05f));
        projectileGravityScale = Mathf.Max(0.55f, 1.0f - ((weightLevel - 1) * 0.04f));
        weightCostChem = ScaleCost(weightCostChem, 1.6f);
        weightCostPhys = ScaleCost(weightCostPhys, 1.4f);

        FinishUpgrade("관측 장비 경량화", weightLevel);
    }

    public void UpgradeTrajectory()
    {
        EnsureResourceManager();
        if (resManager == null || launcher == null)
        {
            GameLogUI.Warning("업그레이드 실패: 대포 또는 자원 매니저 연결이 없습니다.");
            return;
        }

        if (!resManager.SpendPair(ResourceType.Earth, trajectoryCostEarth, ResourceType.Chemistry, trajectoryCostChem))
            return;

        trajectoryLevel++;
        ApplyUpgradeToLauncher(launcher);
        trajectoryCostEarth = ScaleCost(trajectoryCostEarth, 1.5f);
        trajectoryCostChem = ScaleCost(trajectoryCostChem, 1.5f);

        FinishUpgrade("궤적 예측 장비", trajectoryLevel);
    }

    public void UpgradeScoreEfficiency()
    {
        EnsureResourceManager();
        if (resManager == null)
        {
            GameLogUI.Warning("업그레이드 실패: 자원 매니저 연결이 없습니다.");
            return;
        }

        if (!resManager.SpendPair(ResourceType.Physics, scoreCostPhys, ResourceType.Earth, scoreCostEarth))
            return;

        scoreLevel++;
        BonusAmount += 2;
        scoreCostPhys = ScaleCost(scoreCostPhys, 1.7f);
        scoreCostEarth = ScaleCost(scoreCostEarth, 1.7f);

        FinishUpgrade("포인트 효율", scoreLevel);
    }

    public void UpgradeBioSpeed()
    {
        EnsureResourceManager();
        if (resManager == null)
        {
            GameLogUI.Warning("업그레이드 실패: 자원 매니저 연결이 없습니다.");
            return;
        }

        if (!resManager.Spend(ResourceType.Biology, bioCostBio))
            return;

        bioLevel++;
        bioSpeedMultiplier = Mathf.Max(0.25f, 1.0f - ((bioLevel - 1) * 0.1f));
        bioCostBio = ScaleCost(bioCostBio, 1.8f);

        FinishUpgrade("자원 생산 속도", bioLevel);
    }

    public bool CanLaunchToMoon()
    {
        EnsureResourceManager();
        if (resManager == null)
            return false;

        bool hasRequiredLevels =
            rangeLevel >= reqRangeLevelForMoon &&
            weightLevel >= reqWeightLevelForMoon;

        bool hasEnoughResources =
            resManager.physicsRes >= moonCostAll &&
            resManager.chemistryRes >= moonCostAll &&
            resManager.biologyRes >= moonCostAll &&
            resManager.earthRes >= moonCostAll;

        return hasRequiredLevels && hasEnoughResources;
    }

    public bool TryConsumeMoonLaunchCost()
    {
        EnsureResourceManager();

        if (!CanLaunchToMoon())
        {
            GameLogUI.Log("달 발사 조건이 부족합니다.");
            return false;
        }

        bool success = resManager.SpendAllTypes(moonCostAll);

        if (success)
        {
            IsMoonUnlocked = true;
            UpdateUpgradeUI();
            GameLogUI.Log($"달 발사 비용으로 모든 자원 {moonCostAll}개를 사용했습니다.");
            StageMoveManager stageMoveManager = FindFirstObjectByType<StageMoveManager>();
                if (stageMoveManager != null)
                {
                    stageMoveManager.UpdateButtons();
                }
        }

        return success;
    }

    public void UpgradeUnlockMoon()
    {
        if (CanLaunchToMoon())
        {
            if (missionPopup != null)
                missionPopup.SetActive(true);

            GameLogUI.Log("달 발사 조건 만족! 각도 90도와 최대 힘으로 발사하세요.");
        }
        else
        {
            GameLogUI.Log($"달 발사 조건 부족: 파워 Lv.{reqRangeLevelForMoon}, 경량화 Lv.{reqWeightLevelForMoon}, 모든 자원 {moonCostAll}개 필요");
        }

        UpdateUpgradeUI();
    }

    private int ScaleCost(int currentCost, float multiplier)
    {
        return Mathf.Max(currentCost + 1, Mathf.RoundToInt(currentCost * multiplier));
    }

    private void FinishUpgrade(string upgradeName, int newLevel)
    {
        if (resManager != null)
            resManager.UpdateUI();

        UpdateUpgradeUI();
        GameLogUI.Log($"{upgradeName} 업그레이드 완료! Lv.{newLevel}");

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    public void UpdateUpgradeUI()
    {
        if (powerText != null)
            powerText.text = $"Lv.{rangeLevel}\n물리 {powerCostPhys} / 지구 {powerCostEarth}";

        if (weightText != null)
            weightText.text = $"Lv.{weightLevel}\n화학 {weightCostChem} / 물리 {weightCostPhys}";

        if (trajectoryText != null)
            trajectoryText.text = $"Lv.{trajectoryLevel}\n지구 {trajectoryCostEarth} / 화학 {trajectoryCostChem}";

        if (scoreText != null)
            scoreText.text = $"Lv.{scoreLevel}\n물리 {scoreCostPhys} / 지구 {scoreCostEarth}";

        if (bioText != null)
            bioText.text = $"Lv.{bioLevel}\n생물 {bioCostBio}";

        if (moonText != null)
        {
            if (CanLaunchToMoon())
                moonText.text = "달 발사 가능!\n90도 + 최대 파워";
            else
                moonText.text = $"파워 Lv.{reqRangeLevelForMoon}\n경량화 Lv.{reqWeightLevelForMoon}\n모든 자원 {moonCostAll}";
        }

        if (moonStageButton != null)
            moonStageButton.interactable = CanLaunchToMoon();
    }
}
