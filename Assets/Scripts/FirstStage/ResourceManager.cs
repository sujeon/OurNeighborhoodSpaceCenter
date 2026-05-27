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
        if (!CanSpend(type, amount)) return false;

        AddResource(type, -amount, false);
        return true;
    }

    public bool SpendPair(ResourceType firstType, int firstAmount, ResourceType secondType, int secondAmount)
    {
        if (!CanSpend(firstType, firstAmount) || !CanSpend(secondType, secondAmount)) return false;

        AddResource(firstType, -firstAmount, false);
        AddResource(secondType, -secondAmount, false);
        UpdateUI();
        return true;
    }

    public bool SpendAllTypes(int amount)
    {
        if (physicsRes < amount || chemistryRes < amount || biologyRes < amount || earthRes < amount)
        {
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

        if (updateUI) UpdateUI();
    }

    public void AddResource(string type, int amount)
    {
        AddResource(StringToResourceType(type), amount);
    }

    public void AddAllResources(int amount)
    {
        physicsRes += amount;
        chemistryRes += amount;
        biologyRes += amount;
        earthRes += amount; 
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

    public static ResourceType StringToResourceType(string type)
    {
        if (string.IsNullOrEmpty(type)) return ResourceType.Physics;

        if (type.Contains("Chemistry")) return ResourceType.Chemistry;
        if (type.Contains("Biology")) return ResourceType.Biology;
        if (type.Contains("Earth")) return ResourceType.Earth;
        return ResourceType.Physics;
    }

    public static ResourceType TagToResourceType(string tag)
    {
        return StringToResourceType(tag.Replace("Spot", string.Empty));
    }
}
