using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string lastSceneName = "FirstStage";
    // 자원
    public int physicsRes;
    public int chemistryRes;
    public int biologyRes;
    public int earthRes;

    // 업그레이드 레벨
    public int rangeLevel;
    public int weightLevel;
    public int trajectoryLevel;
    public int scoreLevel;
    public int bioLevel;

    // 업그레이드 적용값
    public float projectileMass;
    public int bonusAmount;
    public bool isMoonUnlocked;

    // 달 기지 구매 상태
    public bool hasRoverBase;
    public bool hasRareEarthMine;
    public bool hasPlantDome;
    public bool hasSpaceTelescope;

    // 크레이터 상태
    public List<CraterSaveData> craterStates = new List<CraterSaveData>();
}

[Serializable]
public class CraterSaveData
{
    public string craterName;
    public bool isDiscovered;
    public bool isOccupied;
    public MoonLabType builtLabType;
}