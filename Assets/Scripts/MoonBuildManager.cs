using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MoonLabType
{
    None,
    RoverBase,
    RareEarthMine,
    PlantDome,
    SpaceTelescope
}

public class MoonBuildManager : MonoBehaviour
{
    public static MoonBuildManager Instance { get; private set; }

    [Header("기지 프리팹")]
    [SerializeField] private GameObject roverBasePrefab;
    [SerializeField] private GameObject rareEarthMinePrefab;
    [SerializeField] private GameObject plantDomePrefab;
    [SerializeField] private GameObject spaceTelescopePrefab;

    [Header("구매 상태")]
    [SerializeField] private bool hasRoverBase;
    [SerializeField] private bool hasRareEarthMine;
    [SerializeField] private bool hasPlantDome;
    [SerializeField] private bool hasSpaceTelescope;

    [Header("구매 비용")]
    [SerializeField] private int roverBaseCost = 30;
    [SerializeField] private int rareEarthMineCost = 40;
    [SerializeField] private int plantDomeCost = 35;
    [SerializeField] private int spaceTelescopeCost = 45;

    [Header("건설 UI")]
    [SerializeField] private GameObject constructionPanel;
    [SerializeField] private Button openConstructionButton;

    [Header("고정 크레이터 UI 방식")]
    [SerializeField] private MoonCraterZone[] craterZones;
    [SerializeField] private CraterBuildItemUI[] craterItemUIs;

    [Header("동적 프리팹 방식도 사용할 경우")]
    [SerializeField] private Transform craterItemContent;
    [SerializeField] private CraterBuildItemUI craterBuildItemPrefab;

    private readonly List<MoonCraterZone> discoveredCraters = new List<MoonCraterZone>();
    private readonly Dictionary<MoonCraterZone, CraterBuildItemUI> craterItemMap = new Dictionary<MoonCraterZone, CraterBuildItemUI>();

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
        if (constructionPanel != null)
            constructionPanel.SetActive(false);

        SetupFixedCraterItems();
        RefreshConstructionUI();
        UpdateOpenConstructionButton();
    }

    private void SetupFixedCraterItems()
    {
        if (craterZones == null || craterItemUIs == null)
            return;

        int count = Mathf.Min(craterZones.Length, craterItemUIs.Length);
        for (int i = 0; i < count; i++)
        {
            if (craterZones[i] == null || craterItemUIs[i] == null)
                continue;

            craterItemUIs[i].Setup(craterZones[i], this);
            if (!craterItemMap.ContainsKey(craterZones[i]))
                craterItemMap.Add(craterZones[i], craterItemUIs[i]);
        }
    }

    public void OpenConstructionPanel()
    {
        if (discoveredCraters.Count <= 0)
        {
            GameLogUI.Log("아직 도착한 크레이터가 없습니다.");
            return;
        }

        if (constructionPanel != null)
            constructionPanel.SetActive(true);

        RefreshConstructionUI();
    }

    public void CloseConstructionPanel()
    {
        if (constructionPanel != null)
            constructionPanel.SetActive(false);
    }

    public void RegisterCrater(MoonCraterZone crater)
    {
        if (crater == null)
            return;

        if (!discoveredCraters.Contains(crater))
        {
            discoveredCraters.Add(crater);
            CreateDynamicCraterItemIfNeeded(crater);
        }

        RefreshConstructionUI();
        UpdateOpenConstructionButton();
    }

    private void CreateDynamicCraterItemIfNeeded(MoonCraterZone crater)
    {
        if (craterItemMap.ContainsKey(crater))
            return;

        if (craterItemContent == null || craterBuildItemPrefab == null)
            return;

        CraterBuildItemUI item = Instantiate(craterBuildItemPrefab, craterItemContent);
        item.Setup(crater, this);
        craterItemMap.Add(crater, item);
    }

    public void RefreshConstructionUI()
    {
        foreach (CraterBuildItemUI item in craterItemMap.Values)
        {
            if (item != null)
                item.Refresh();
        }
    }

    private void UpdateOpenConstructionButton()
    {
        if (openConstructionButton != null)
            openConstructionButton.gameObject.SetActive(discoveredCraters.Count > 0);
    }

    public void BuyRoverBase() => TryBuyLab(MoonLabType.RoverBase);
    public void BuyRareEarthMine() => TryBuyLab(MoonLabType.RareEarthMine);
    public void BuyPlantDome() => TryBuyLab(MoonLabType.PlantDome);
    public void BuySpaceTelescope() => TryBuyLab(MoonLabType.SpaceTelescope);

    private void TryBuyLab(MoonLabType labType)
    {
        if (HasPurchased(labType))
        {
            GameLogUI.Log($"{GetLabName(labType)}는 이미 구매했습니다.");
            return;
        }

        if (ResourceManager.Instance == null)
        {
            GameLogUI.Warning("기지 구매 실패: ResourceManager가 없습니다.");
            return;
        }

        int cost = GetCost(labType);
        ResourceType costType = GetCostResourceType(labType);

        if (!ResourceManager.Instance.Spend(costType, cost))
            return;

        SetPurchased(labType, true);
        ResourceManager.Instance.UpdateUI();

        GameLogUI.Log($"{GetLabName(labType)} 구매 완료! {ResourceManager.GetResourceName(costType)} -{cost}");
        RefreshConstructionUI();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    public bool CanBuildLab(MoonLabType labType, MoonCraterZone crater)
    {
        if (crater == null || !crater.CanBuild())
            return false;

        if (!HasPurchased(labType))
            return false;

        return GetPrefab(labType) != null;
    }

    public void BuildLabAtCrater(MoonLabType labType, MoonCraterZone crater)
    {
        if (!CanBuildLab(labType, crater))
        {
            GameLogUI.Log("건설할 수 없습니다. 크레이터 상태나 구매 여부를 확인하세요.");
            return;
        }

        GameObject prefab = GetPrefab(labType);
        GameObject builtLab = Instantiate(prefab, crater.GetBuildPosition(), Quaternion.identity);

        if (!crater.TrySetBuiltLab(labType, builtLab))
        {
            Destroy(builtLab);
            GameLogUI.Warning("크레이터 상태 문제로 건설에 실패했습니다.");
            return;
        }

        GameLogUI.Log($"{crater.CraterName}에 {GetLabName(labType)} 건설 완료!");
        RefreshConstructionUI();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    public void RemoveLabAtCrater(MoonCraterZone crater)
    {
        if (crater == null)
            return;

        if (!crater.IsOccupied)
        {
            GameLogUI.Log("제거할 기지가 없습니다.");
            return;
        }

        string removedName = GetLabName(crater.BuiltLabType);
        crater.RemoveBuiltLab();
        GameLogUI.Log($"{crater.CraterName}의 {removedName} 제거 완료");
        RefreshConstructionUI();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    public bool HasPurchased(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase: return hasRoverBase;
            case MoonLabType.RareEarthMine: return hasRareEarthMine;
            case MoonLabType.PlantDome: return hasPlantDome;
            case MoonLabType.SpaceTelescope: return hasSpaceTelescope;
            default: return false;
        }
    }

    private void SetPurchased(MoonLabType labType, bool value)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase:
                hasRoverBase = value;
                break;
            case MoonLabType.RareEarthMine:
                hasRareEarthMine = value;
                break;
            case MoonLabType.PlantDome:
                hasPlantDome = value;
                break;
            case MoonLabType.SpaceTelescope:
                hasSpaceTelescope = value;
                break;
        }
    }

    private int GetCost(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase: return roverBaseCost;
            case MoonLabType.RareEarthMine: return rareEarthMineCost;
            case MoonLabType.PlantDome: return plantDomeCost;
            case MoonLabType.SpaceTelescope: return spaceTelescopeCost;
            default: return 0;
        }
    }

    private ResourceType GetCostResourceType(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase: return ResourceType.Physics;
            case MoonLabType.RareEarthMine: return ResourceType.Earth;
            case MoonLabType.PlantDome: return ResourceType.Biology;
            case MoonLabType.SpaceTelescope: return ResourceType.Chemistry;
            default: return ResourceType.Physics;
        }
    }

    private GameObject GetPrefab(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase: return roverBasePrefab;
            case MoonLabType.RareEarthMine: return rareEarthMinePrefab;
            case MoonLabType.PlantDome: return plantDomePrefab;
            case MoonLabType.SpaceTelescope: return spaceTelescopePrefab;
            default: return null;
        }
    }

    public string GetLabName(MoonLabType labType)
    {
        switch (labType)
        {
            case MoonLabType.RoverBase: return "월면차 기지";
            case MoonLabType.RareEarthMine: return "희토류 기지";
            case MoonLabType.PlantDome: return "우주 식물 돔";
            case MoonLabType.SpaceTelescope: return "우주 망원경";
            default: return "없음";
        }
    }

    public void WriteSaveData(SaveData data)
    {
        data.hasRoverBase = hasRoverBase;
        data.hasRareEarthMine = hasRareEarthMine;
        data.hasPlantDome = hasPlantDome;
        data.hasSpaceTelescope = hasSpaceTelescope;
        data.craterStates.Clear();

        MoonCraterZone[] craters = craterZones != null && craterZones.Length > 0
            ? craterZones
            : FindObjectsByType<MoonCraterZone>(FindObjectsSortMode.None);

        foreach (MoonCraterZone crater in craters)
        {
            if (crater == null) continue;

            CraterSaveData craterData = new CraterSaveData
            {
                craterName = crater.CraterName,
                isDiscovered = crater.IsDiscovered,
                isOccupied = crater.IsOccupied,
                builtLabType = crater.BuiltLabType
            };

            data.craterStates.Add(craterData);
        }
    }

    public void LoadSaveData(SaveData data)
    {
        hasRoverBase = data.hasRoverBase;
        hasRareEarthMine = data.hasRareEarthMine;
        hasPlantDome = data.hasPlantDome;
        hasSpaceTelescope = data.hasSpaceTelescope;

        foreach (CraterSaveData craterData in data.craterStates)
        {
            MoonCraterZone crater = FindCraterByName(craterData.craterName);
            if (crater == null) continue;

            crater.LoadState(craterData.isDiscovered, false, MoonLabType.None);

            if (craterData.isDiscovered && !discoveredCraters.Contains(crater))
                discoveredCraters.Add(crater);

            if (craterData.isOccupied)
                RebuildLabFromSave(crater, craterData.builtLabType);
        }

        RefreshConstructionUI();
        UpdateOpenConstructionButton();
    }

    private MoonCraterZone FindCraterByName(string craterName)
    {
        MoonCraterZone[] craters = craterZones != null && craterZones.Length > 0
            ? craterZones
            : FindObjectsByType<MoonCraterZone>(FindObjectsSortMode.None);

        foreach (MoonCraterZone crater in craters)
        {
            if (crater != null && crater.CraterName == craterName)
                return crater;
        }

        return null;
    }

    private void RebuildLabFromSave(MoonCraterZone crater, MoonLabType labType)
    {
        if (crater == null || labType == MoonLabType.None)
            return;

        GameObject prefab = GetPrefab(labType);
        if (prefab == null)
            return;

        GameObject labObject = Instantiate(prefab, crater.GetBuildPosition(), Quaternion.identity);
        crater.SetBuiltLabFromSave(labType, labObject);
    }
}
