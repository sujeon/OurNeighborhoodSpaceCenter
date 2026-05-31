using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraterBuildItemUI : MonoBehaviour
{
    [Header("크레이터 정보 UI")]
    [SerializeField] private TextMeshProUGUI craterNameText;
    [SerializeField] private TextMeshProUGUI craterRewardText;
    [SerializeField] private TextMeshProUGUI builtLabText;

    [Header("기지 건설 버튼")]
    [SerializeField] private Button roverBaseBuildButton;
    [SerializeField] private Button rareEarthMineBuildButton;
    [SerializeField] private Button plantDomeBuildButton;
    [SerializeField] private Button spaceTelescopeBuildButton;

    [Header("제거 버튼")]
    [SerializeField] private Button removeButton;

    private MoonCraterZone crater;
    private MoonBuildManager buildManager;

    public void Setup(MoonCraterZone targetCrater, MoonBuildManager manager)
    {
        crater = targetCrater;
        buildManager = manager;
        SetupButtonEvents();
        Refresh();
    }

    private void SetupButtonEvents()
    {
        if (roverBaseBuildButton != null)
        {
            roverBaseBuildButton.onClick.RemoveAllListeners();
            roverBaseBuildButton.onClick.AddListener(() => Build(MoonLabType.RoverBase));
        }

        if (rareEarthMineBuildButton != null)
        {
            rareEarthMineBuildButton.onClick.RemoveAllListeners();
            rareEarthMineBuildButton.onClick.AddListener(() => Build(MoonLabType.RareEarthMine));
        }

        if (plantDomeBuildButton != null)
        {
            plantDomeBuildButton.onClick.RemoveAllListeners();
            plantDomeBuildButton.onClick.AddListener(() => Build(MoonLabType.PlantDome));
        }

        if (spaceTelescopeBuildButton != null)
        {
            spaceTelescopeBuildButton.onClick.RemoveAllListeners();
            spaceTelescopeBuildButton.onClick.AddListener(() => Build(MoonLabType.SpaceTelescope));
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(RemoveBuiltLab);
        }
    }

    public void Refresh()
    {
        if (crater == null || buildManager == null)
            return;

        if (craterNameText != null)
            craterNameText.text = crater.CraterName;

        if (craterRewardText != null)
            craterRewardText.text = crater.IsDiscovered ? $"보상: +{crater.LandingReward}" : "보상: ???";

        if (builtLabText != null)
        {
            if (!crater.IsDiscovered)
                builtLabText.text = "상태: 미발견";
            else if (crater.IsOccupied)
                builtLabText.text = $"건설됨: {buildManager.GetLabName(crater.BuiltLabType)}";
            else
                builtLabText.text = "건설됨: 없음";
        }

        RefreshBuildButton(roverBaseBuildButton, MoonLabType.RoverBase);
        RefreshBuildButton(rareEarthMineBuildButton, MoonLabType.RareEarthMine);
        RefreshBuildButton(plantDomeBuildButton, MoonLabType.PlantDome);
        RefreshBuildButton(spaceTelescopeBuildButton, MoonLabType.SpaceTelescope);

        if (removeButton != null)
            removeButton.interactable = crater.IsOccupied;
    }

    private void RefreshBuildButton(Button button, MoonLabType labType)
    {
        if (button == null)
            return;

        button.interactable = buildManager.CanBuildLab(labType, crater);
    }

    private void Build(MoonLabType labType)
    {
        if (buildManager == null || crater == null)
            return;

        buildManager.BuildLabAtCrater(labType, crater);
        Refresh();
    }

    private void RemoveBuiltLab()
    {
        if (buildManager == null || crater == null)
            return;

        buildManager.RemoveLabAtCrater(crater);
        Refresh();
    }
}
