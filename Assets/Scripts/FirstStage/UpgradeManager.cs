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
        if (resManager == null) resManager = ResourceManager.Instance;
        UpdateUpgradeUI();
    }

    public void UpgradeMaxPower()
    {
        if (resManager == null || launcher == null) return;
        if (!resManager.SpendPair(ResourceType.Physics, powerCostPhys, ResourceType.Earth, powerCostEarth)) return;

        rangeLevel++;
        launcher.maxForce += 5f;
        powerCostPhys = ScaleCost(powerCostPhys, 1.5f);
        powerCostEarth = ScaleCost(powerCostEarth, 1.5f);
        FinishUpgrade();
    }

    public void UpgradeWeight()
    {
        if (resManager == null) return;
        if (!resManager.SpendPair(ResourceType.Chemistry, weightCostChem, ResourceType.Physics, weightCostPhys)) return;

        weightLevel++;

        // 질량만 줄이면 체감이 약할 수 있어서, 발사체가 받는 중력 배율도 조금씩 낮춥니다.
        ProjectileMass = Mathf.Max(0.4f, 1.0f - ((weightLevel - 1) * 0.05f));
        projectileGravityScale = Mathf.Max(0.55f, 1.0f - ((weightLevel - 1) * 0.04f));

        weightCostChem = ScaleCost(weightCostChem, 1.6f);
        weightCostPhys = ScaleCost(weightCostPhys, 1.4f);
        FinishUpgrade();
    }

    public void UpgradeTrajectory()
    {
        if (resManager == null || launcher == null) return;
        if (!resManager.SpendPair(ResourceType.Earth, trajectoryCostEarth, ResourceType.Chemistry, trajectoryCostChem)) return;

        trajectoryLevel++;
        launcher.trajectoryStepCount = Mathf.Min(200, launcher.trajectoryStepCount + 10);
        launcher.timeStep = Mathf.Max(0.035f, launcher.timeStep - 0.002f);

        trajectoryCostEarth = ScaleCost(trajectoryCostEarth, 1.5f);
        trajectoryCostChem = ScaleCost(trajectoryCostChem, 1.5f);
        FinishUpgrade();
    }

    public void UpgradeScoreEfficiency()
    {
        if (resManager == null) return;
        if (!resManager.SpendPair(ResourceType.Physics, scoreCostPhys, ResourceType.Earth, scoreCostEarth)) return;

        scoreLevel++;
        BonusAmount += 2;
        scoreCostPhys = ScaleCost(scoreCostPhys, 1.7f);
        scoreCostEarth = ScaleCost(scoreCostEarth, 1.7f);
        FinishUpgrade();
    }

    public void UpgradeBioSpeed()
    {
        if (resManager == null) return;
        if (!resManager.Spend(ResourceType.Biology, bioCostBio)) return;

        bioLevel++;
        bioSpeedMultiplier = Mathf.Max(0.25f, 1.0f - ((bioLevel - 1) * 0.1f));
        bioCostBio = ScaleCost(bioCostBio, 1.8f);
        FinishUpgrade();
    }
    public bool CanLaunchToMoon()
    {
        if (resManager == null)
            resManager = ResourceManager.Instance;

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

    public void UpgradeUnlockMoon()
    {
        if (CanLaunchToMoon())
        {
            if (missionPopup != null)
                missionPopup.SetActive(true);

            Debug.Log("달 발사 조건 만족! 대포를 90도로 맞추고 최대 힘으로 발사하세요.");
        }
        else
        {
            Debug.Log("아직 달 발사 조건이 부족합니다.");
        }

        UpdateUpgradeUI();
    }


    private int ScaleCost(int currentCost, float multiplier)
    {
        return Mathf.Max(currentCost + 1, Mathf.RoundToInt(currentCost * multiplier));
    }

    private void FinishUpgrade()
    {
        if (resManager != null) resManager.UpdateUI();
        UpdateUpgradeUI();
    }

    public void UpdateUpgradeUI()
    {
        SetText(powerText, rangeLevel, $"물리:{powerCostPhys} / 지구:{powerCostEarth}");
        SetText(weightText, weightLevel, $"화학:{weightCostChem} / 물리:{weightCostPhys}\n중력:{projectileGravityScale:0.00}x");
        SetText(trajectoryText, trajectoryLevel, $"지구:{trajectoryCostEarth} / 화학:{trajectoryCostChem}");
        SetText(scoreText, scoreLevel, $"물리:{scoreCostPhys} / 지구:{scoreCostEarth}");
        SetText(bioText, bioLevel, $"생물:{bioCostBio}\n속도:{1f / bioSpeedMultiplier:0.0}x");

        if (moonText != null)
        {
            if (CanLaunchToMoon())
            {
                moonText.text = $"달 발사 가능!\n각도 90도 + 최대 파워";
            }
            else
            {
                moonText.text =
                    $"달 발사 조건\n" +
                    $"파워 Lv.{reqRangeLevelForMoon}\n" +
                    $"경량화 Lv.{reqWeightLevelForMoon}\n" +
                    $"모든 자원 {moonCostAll}개";
            }
        }
    }

    private void SetText(TextMeshProUGUI tmp, int lv, string costs)
    {
        if (tmp == null) return;
        tmp.text = $"<b>Lv.{lv}</b>\n<size=70%>{costs}</size>";
    }
}
