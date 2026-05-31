using UnityEngine;
using TMPro;

public enum ResourceType
{
    Physics,
    Chemistry,
    Biology,
    Earth
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("자원 수치")]
    public int physicsRes;
    public int chemistryRes;
    public int biologyRes;
    public int earthRes;

    [Header("UI 개별 텍스트 연결 (TMP)")]
    public TextMeshProUGUI physicsText;
    public TextMeshProUGUI chemistryText;
    public TextMeshProUGUI biologyText;
    public TextMeshProUGUI earthText;

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
        UpdateUI();
    }

    public bool CanSpend(ResourceType type, int amount)
    {
        return GetResource(type) >= amount;
    }

    public bool Spend(ResourceType type, int amount)
    {
        if (amount <= 0)
            return true;

        if (GetResource(type) < amount)
        {
            GameLogUI.Log($"{GetResourceName(type)} 자원이 부족합니다. 필요: {amount}, 보유: {GetResource(type)}");
            return false;
        }

        AddResource(type, -amount, false);
        UpdateUI();
        return true;
    }

    public bool SpendPair(ResourceType firstType, int firstAmount, ResourceType secondType, int secondAmount)
    {
        if (!CanSpend(firstType, firstAmount) || !CanSpend(secondType, secondAmount))
        {
            GameLogUI.Log($"자원이 부족합니다. {GetResourceName(firstType)} {firstAmount}, {GetResourceName(secondType)} {secondAmount} 필요");
            return false;
        }

        AddResource(firstType, -firstAmount, false);
        AddResource(secondType, -secondAmount, false);
        UpdateUI();
        return true;
    }

    public bool SpendAllTypes(int amount)
    {
        if (physicsRes < amount || chemistryRes < amount || biologyRes < amount || earthRes < amount)
        {
            GameLogUI.Log($"모든 자원이 {amount}개 이상 필요합니다.");
            return false;
        }

        physicsRes -= amount;
        chemistryRes -= amount;
        biologyRes -= amount;
        earthRes -= amount;
        UpdateUI();
        return true;
    }

    public void AddResource(ResourceType type, int amount, bool updateUI = true)
    {
        switch (type)
        {
            case ResourceType.Physics:
                physicsRes = Mathf.Max(0, physicsRes + amount);
                break;
            case ResourceType.Chemistry:
                chemistryRes = Mathf.Max(0, chemistryRes + amount);
                break;
            case ResourceType.Biology:
                biologyRes = Mathf.Max(0, biologyRes + amount);
                break;
            case ResourceType.Earth:
                earthRes = Mathf.Max(0, earthRes + amount);
                break;
        }

        if (updateUI)
            UpdateUI();
    }

    public void AddResource(string type, int amount)
    {
        AddResource(StringToResourceType(type), amount);
    }

    public void AddAllResources(int amount)
    {
        physicsRes = Mathf.Max(0, physicsRes + amount);
        chemistryRes = Mathf.Max(0, chemistryRes + amount);
        biologyRes = Mathf.Max(0, biologyRes + amount);
        earthRes = Mathf.Max(0, earthRes + amount);
        UpdateUI();
    }

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Physics: return physicsRes;
            case ResourceType.Chemistry: return chemistryRes;
            case ResourceType.Biology: return biologyRes;
            case ResourceType.Earth: return earthRes;
            default: return 0;
        }
    }

    public void UpdateUI()
    {
        if (physicsText != null) physicsText.text = physicsRes.ToString();
        if (chemistryText != null) chemistryText.text = chemistryRes.ToString();
        if (biologyText != null) biologyText.text = biologyRes.ToString();
        if (earthText != null) earthText.text = earthRes.ToString();
    }

    public static string GetResourceName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Physics: return "물리";
            case ResourceType.Chemistry: return "화학";
            case ResourceType.Biology: return "생물";
            case ResourceType.Earth: return "지구";
            default: return "자원";
        }
    }

    public static ResourceType StringToResourceType(string type)
    {
        if (string.IsNullOrEmpty(type)) return ResourceType.Physics;

        if (type.Contains("Chemistry") || type.Contains("Chemical") || type.Contains("화학")) return ResourceType.Chemistry;
        if (type.Contains("Biology") || type.Contains("생물")) return ResourceType.Biology;
        if (type.Contains("Earth") || type.Contains("지구")) return ResourceType.Earth;
        return ResourceType.Physics;
    }

    public static ResourceType TagToResourceType(string tag)
    {
        return StringToResourceType(tag.Replace("Spot", string.Empty));
    }
}
