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
        if (hasRoverBase) return;

        if (ResourceManager.Instance.Spend(ResourceType.Physics, roverBaseCost))
        {
            hasRoverBase = true;
            UpdateBuildUI();
        }
    }

    public void BuyRareEarthMine()
    {
        if (hasRareEarthMine) return;

        if (ResourceManager.Instance.Spend(ResourceType.Earth, rareEarthMineCost))
        {
            hasRareEarthMine = true;
            UpdateBuildUI();
        }
    }

    public void BuyPlantDome()
    {
        if (hasPlantDome) return;

        if (ResourceManager.Instance.Spend(ResourceType.Biology, plantDomeCost))
        {
            hasPlantDome = true;
            UpdateBuildUI();
        }
    }

    public void BuySpaceTelescope()
    {
        if (hasSpaceTelescope) return;

        if (ResourceManager.Instance.Spend(ResourceType.Chemistry, spaceTelescopeCost))
        {
            hasSpaceTelescope = true;
            UpdateBuildUI();
        }
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

        GameObject prefab = GetLabPrefab(labType);

        if (prefab == null)
        {
            Debug.LogWarning("연구소 프리팹이 연결되지 않았습니다.");
            return;
        }

        if (!HasPurchasedLab(labType))
        {
            Debug.Log("아직 이 연구소를 구매하지 않았습니다.");
            return;
        }

        Instantiate(prefab, currentCrater.GetBuildPosition(), Quaternion.identity);

        currentCrater.MarkOccupied();

        if (buildPanel != null)
            buildPanel.SetActive(false);

        currentCrater = null;

        Debug.Log($"{labType} 연구소 건설 완료!");
    }

    private GameObject GetLabPrefab(MoonLabType labType)
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

    private void UpdateBuildUI()
    {
        if (currentCraterText != null)
        {
            if (currentCrater != null)
                currentCraterText.text = $"{currentCrater.craterName}\n건설 가능";
            else
                currentCraterText.text = "크레이터를 선택하세요";
        }

        if (roverBaseBuildButton != null)
            roverBaseBuildButton.interactable = currentCrater != null && hasRoverBase;

        if (rareEarthMineBuildButton != null)
            rareEarthMineBuildButton.interactable = currentCrater != null && hasRareEarthMine;

        if (plantDomeBuildButton != null)
            plantDomeBuildButton.interactable = currentCrater != null && hasPlantDome;

        if (spaceTelescopeBuildButton != null)
            spaceTelescopeBuildButton.interactable = currentCrater != null && hasSpaceTelescope;
    }
}