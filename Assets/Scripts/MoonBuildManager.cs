using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoonBuildManager : MonoBehaviour
{
    public static MoonBuildManager Instance { get; private set; }

    [Header("연구소 프리팹")]
    public GameObject roverBasePrefab;
    public GameObject rareEarthMinePrefab;
    public GameObject plantDomePrefab;
    public GameObject spaceTelescopePrefab;

    [Header("구매 상태")]
    public bool hasRoverBase;
    public bool hasRareEarthMine;
    public bool hasPlantDome;
    public bool hasSpaceTelescope;

    [Header("구매 비용")]
    public int roverBaseCost = 30;
    public int rareEarthMineCost = 40;
    public int plantDomeCost = 35;
    public int spaceTelescopeCost = 45;

    [Header("UI")]
    public GameObject buildPanel;
    public Button roverBaseBuildButton;
    public Button rareEarthMineBuildButton;
    public Button plantDomeBuildButton;
    public Button spaceTelescopeBuildButton;
    public TextMeshProUGUI currentCraterText;

    private MoonCraterZone currentCrater;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (buildPanel != null)
            buildPanel.SetActive(false);

        UpdateBuildUI();
    }

    public void SetCurrentCrater(MoonCraterZone crater)
    {
        currentCrater = crater;

        if (buildPanel != null)
            buildPanel.SetActive(true);

        UpdateBuildUI();
    }

    public void BuyRoverBase()
    {
        BuyLab(MoonLabType.RoverBase);
    }

    public void BuyRareEarthMine()
    {
        BuyLab(MoonLabType.RareEarthMine);
    }

    public void BuyPlantDome()
    {
        BuyLab(MoonLabType.PlantDome);
    }

    public void BuySpaceTelescope()
    {
        BuyLab(MoonLabType.SpaceTelescope);
    }

    public void BuildRoverBase()
    {
        BuildLab(MoonLabType.RoverBase);
    }

    public void BuildRareEarthMine()
    {
        BuildLab(MoonLabType.RareEarthMine);
    }

    public void BuildPlantDome()
    {
        BuildLab(MoonLabType.PlantDome);
    }

    public void BuildSpaceTelescope()
    {
        BuildLab(MoonLabType.SpaceTelescope);
    }

    public void CloseBuildPanel()
    {
        currentCrater = null;
        SetBuildPanelVisible(false);
        UpdateBuildUI();
    }

    private void BuyLab(MoonLabType labType)
    {
        if (HasPurchasedLab(labType))
        {
            Debug.Log($"{GetLabDisplayName(labType)}은 이미 구매했습니다.");
            return;
        }

        ResourceManager resourceManager = ResourceManager.Instance;
        if (resourceManager == null)
        {
            Debug.LogWarning("ResourceManager가 씬에 없습니다.");
            return;
        }

        ResourceType costType = GetCostType(labType);
        int cost = GetCost(labType);

        if (!resourceManager.Spend(costType, cost))
        {
            Debug.Log($"{GetLabDisplayName(labType)} 구매에 필요한 {costType} 자원이 부족합니다.");
            return;
        }

        SetPurchasedLab(labType, true);
        resourceManager.UpdateUI();
        UpdateBuildUI();
    }

    private void BuildLab(MoonLabType labType)
    {
        if (currentCrater == null)
        {
            Debug.Log("선택된 크레이터가 없습니다.");
            return;
        }

        if (!currentCrater.IsBuildable || currentCrater.IsOccupied)
        {
            Debug.Log("이 크레이터에는 건설할 수 없습니다.");
            return;
        }

        GameObject prefab = GetPrefab(labType);
        if (prefab == null)
        {
            Debug.LogWarning($"{GetLabDisplayName(labType)} 프리팹이 연결되지 않았습니다.");
            return;
        }

        if (!HasPurchasedLab(labType))
        {
            Debug.Log("아직 이 연구소를 구매하지 않았습니다.");
            return;
        }

        Instantiate(prefab, currentCrater.GetBuildPosition(), Quaternion.identity);

        currentCrater.MarkOccupied();

        currentCrater = null;
        SetBuildPanelVisible(false);
        UpdateBuildUI();

        Debug.Log($"{GetLabDisplayName(labType)} 건설 완료!");
    }

    private GameObject GetPrefab(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                return roverBasePrefab;

            case MoonLabType.RareEarthMine:
                return rareEarthMinePrefab;

            case MoonLabType.PlantDome:
                return plantDomePrefab;

            case MoonLabType.SpaceTelescope:
                return spaceTelescopePrefab;

            default:
                return null;
        }
    }

    private int GetCost(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                return roverBaseCost;

            case MoonLabType.RareEarthMine:
                return rareEarthMineCost;

            case MoonLabType.PlantDome:
                return plantDomeCost;

            case MoonLabType.SpaceTelescope:
                return spaceTelescopeCost;

            default:
                return 0;
        }
    }

    private ResourceType GetCostType(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                return ResourceType.Physics;

            case MoonLabType.RareEarthMine:
                return ResourceType.Earth;

            case MoonLabType.PlantDome:
                return ResourceType.Biology;

            case MoonLabType.SpaceTelescope:
                return ResourceType.Chemistry;

            default:
                return ResourceType.Physics;
        }
    }

    private bool HasPurchasedLab(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                return hasRoverBase;

            case MoonLabType.RareEarthMine:
                return hasRareEarthMine;

            case MoonLabType.PlantDome:
                return hasPlantDome;

            case MoonLabType.SpaceTelescope:
                return hasSpaceTelescope;

            default:
                return false;
        }
    }

    private void SetPurchasedLab(MoonLabType labType, bool purchased)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                hasRoverBase = purchased;
                break;

            case MoonLabType.RareEarthMine:
                hasRareEarthMine = purchased;
                break;

            case MoonLabType.PlantDome:
                hasPlantDome = purchased;
                break;

            case MoonLabType.SpaceTelescope:
                hasSpaceTelescope = purchased;
                break;
        }
    }

    private string GetLabDisplayName(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                return "탐사 로버 기지";

            case MoonLabType.RareEarthMine:
                return "희토류 채굴장";

            case MoonLabType.PlantDome:
                return "식물 돔";

            case MoonLabType.SpaceTelescope:
                return "우주 망원경";

            default:
                return "달 연구 시설";
        }
    }

    private void UpdateBuildUI()
    {
        if (currentCraterText != null)
        {
            if (currentCrater != null)
                currentCraterText.text = $"{currentCrater.craterName}\n건설 가능";
            else
                currentCraterText.text = "크레이터를 선택하세요";
        }

        SetBuildButtonState(roverBaseBuildButton, MoonLabType.RoverBase);
        SetBuildButtonState(rareEarthMineBuildButton, MoonLabType.RareEarthMine);
        SetBuildButtonState(plantDomeBuildButton, MoonLabType.PlantDome);
        SetBuildButtonState(spaceTelescopeBuildButton, MoonLabType.SpaceTelescope);
    }

    private void SetBuildPanelVisible(bool visible)
    {
        if (buildPanel != null)
            buildPanel.SetActive(visible);
    }

    private void SetBuildButtonState(Button button, MoonLabType labType)
    {
        if (button == null)
            return;

        button.interactable = CanBuildLab(labType);
    }

    private bool CanBuildLab(MoonLabType labType)
    {
        return currentCrater != null &&
               currentCrater.IsBuildable &&
               !currentCrater.IsOccupied &&
               HasPurchasedLab(labType) &&
               GetPrefab(labType) != null;
    }
}
