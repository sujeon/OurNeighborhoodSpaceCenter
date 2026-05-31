using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

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

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.lastSceneName = SceneManager.GetActiveScene().name;

        SaveResources(data);
        SaveUpgrades(data);
        SaveMoonBuildData(data);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        GameLogUI.Log("게임이 저장되었습니다.");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            GameLogUI.Log("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        LoadResources(data);
        LoadUpgrades(data);
        LoadMoonBuildData(data);

        GameLogUI.Log("저장 데이터를 불러왔습니다.");
    }
    public void SaveGameWithSceneName(string sceneName)
    {
        SaveData data = new SaveData();

        data.lastSceneName = sceneName;

        SaveResources(data);
        SaveUpgrades(data);
        SaveMoonBuildData(data);

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath, json);

        Debug.Log($"게임 저장 완료: {sceneName}");

        if (GameLogUI.Instance != null)
            GameLogUI.Instance.Show("게임 저장 완료");
    }
    public string GetSavedSceneName(string defaultSceneName = "FirstStage")
    {
        if (!File.Exists(SavePath))
            return defaultSceneName;

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (string.IsNullOrEmpty(data.lastSceneName))
            return defaultSceneName;

        return data.lastSceneName;
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            GameLogUI.Log("저장 파일을 삭제했습니다.");
        }
    }

    private void SaveResources(SaveData data)
    {
        if (ResourceManager.Instance == null) return;

        data.physicsRes = ResourceManager.Instance.physicsRes;
        data.chemistryRes = ResourceManager.Instance.chemistryRes;
        data.biologyRes = ResourceManager.Instance.biologyRes;
        data.earthRes = ResourceManager.Instance.earthRes;
    }

    private void LoadResources(SaveData data)
    {
        if (ResourceManager.Instance == null) return;

        ResourceManager.Instance.physicsRes = data.physicsRes;
        ResourceManager.Instance.chemistryRes = data.chemistryRes;
        ResourceManager.Instance.biologyRes = data.biologyRes;
        ResourceManager.Instance.earthRes = data.earthRes;
        ResourceManager.Instance.UpdateUI();
    }

    private void SaveUpgrades(SaveData data)
    {
        if (UpgradeManager.Instance == null) return;

        data.rangeLevel = UpgradeManager.Instance.rangeLevel;
        data.weightLevel = UpgradeManager.Instance.weightLevel;
        data.trajectoryLevel = UpgradeManager.Instance.trajectoryLevel;
        data.scoreLevel = UpgradeManager.Instance.scoreLevel;
        data.bioLevel = UpgradeManager.Instance.bioLevel;

        data.projectileMass = UpgradeManager.ProjectileMass;
        data.bonusAmount = UpgradeManager.BonusAmount;
        data.isMoonUnlocked = UpgradeManager.IsMoonUnlocked;
    }

    private void LoadUpgrades(SaveData data)
    {
        if (UpgradeManager.Instance == null) return;

        UpgradeManager.Instance.rangeLevel = data.rangeLevel;
        UpgradeManager.Instance.weightLevel = data.weightLevel;
        UpgradeManager.Instance.trajectoryLevel = data.trajectoryLevel;
        UpgradeManager.Instance.scoreLevel = data.scoreLevel;
        UpgradeManager.Instance.bioLevel = data.bioLevel;

        UpgradeManager.ProjectileMass = data.projectileMass;
        UpgradeManager.BonusAmount = data.bonusAmount;
        UpgradeManager.IsMoonUnlocked = data.isMoonUnlocked;
        UpgradeManager.Instance.UpdateUpgradeUI();
    }

    private void SaveMoonBuildData(SaveData data)
    {
        if (MoonBuildManager.Instance == null) return;
        MoonBuildManager.Instance.WriteSaveData(data);
    }

    private void LoadMoonBuildData(SaveData data)
    {
        if (MoonBuildManager.Instance == null) return;
        MoonBuildManager.Instance.LoadSaveData(data);
    }
}
