using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class CraterData
{
    [Header("크레이터 위치와 크기")]
    public int centerX = 50;
    public int radius = 8;
    public int depth = 4;

    [Header("중앙 평탄화")]
    public bool flattenCenter = false;
    public int flatRadius = 2;
}

public class MoonTerrainGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase surfaceTile;

    [Header("맵 크기")]
    [SerializeField] private int mapWidth = 150;
    [SerializeField] private int baseHeight = 4;
    [SerializeField] private int heightMultiplier = 6;

    [Header("펄린 노이즈")]
    [SerializeField] private float noiseScale = 0.08f;
    [SerializeField] private float seed = 1234f;

    [Header("크레이터 목록")]
    [SerializeField] private CraterData[] craters;

    private int[] heightMap;

    [ContextMenu("Generate Moon Terrain")]
    public void GenerateTerrain()
    {
        if (groundTilemap == null || groundTile == null)
        {
            GameLogUI.Warning("달 지형 생성 실패: Tilemap 또는 Tile이 연결되지 않았습니다.");
            return;
        }

        groundTilemap.ClearAllTiles();
        heightMap = new int[mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            int terrainHeight = CalculateHeight(x);
            heightMap[x] = terrainHeight;

            for (int y = 0; y <= terrainHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (surfaceTile != null && y == terrainHeight)
                    groundTilemap.SetTile(pos, surfaceTile);
                else
                    groundTilemap.SetTile(pos, groundTile);
            }
        }

        GameLogUI.Log($"달 지형 생성 완료: 폭 {mapWidth}, 크레이터 {(craters != null ? craters.Length : 0)}개");
    }

    private int CalculateHeight(int x)
    {
        float noise = Mathf.PerlinNoise(x * noiseScale, seed);
        int height = Mathf.RoundToInt(noise * heightMultiplier) + baseHeight;
        height -= GetTotalCraterDepthAtX(x);
        height = ApplyCraterFlatCenter(x, height);
        return Mathf.Max(1, height);
    }

    private int GetTotalCraterDepthAtX(int x)
    {
        int totalDepth = 0;
        if (craters == null)
            return totalDepth;

        foreach (CraterData crater in craters)
        {
            float distance = Mathf.Abs(x - crater.centerX);
            if (distance > crater.radius)
                continue;

            float normalizedDistance = distance / crater.radius;
            float craterCurve = 1f - normalizedDistance * normalizedDistance;
            totalDepth += Mathf.RoundToInt(craterCurve * crater.depth);
        }

        return totalDepth;
    }

    private int ApplyCraterFlatCenter(int x, int currentHeight)
    {
        if (craters == null)
            return currentHeight;

        foreach (CraterData crater in craters)
        {
            if (!crater.flattenCenter)
                continue;

            int distance = Mathf.Abs(x - crater.centerX);
            if (distance <= crater.flatRadius)
                return GetBaseHeightWithCrater(crater.centerX, crater);
        }

        return currentHeight;
    }

    private int GetBaseHeightWithCrater(int x, CraterData targetCrater)
    {
        float noise = Mathf.PerlinNoise(x * noiseScale, seed);
        int height = Mathf.RoundToInt(noise * heightMultiplier) + baseHeight;

        float distance = Mathf.Abs(x - targetCrater.centerX);
        float normalizedDistance = distance / targetCrater.radius;
        float craterCurve = 1f - normalizedDistance * normalizedDistance;
        int depth = Mathf.RoundToInt(craterCurve * targetCrater.depth);

        return Mathf.Max(1, height - depth);
    }

    public int GetHeightAtX(int x)
    {
        if (heightMap == null || x < 0 || x >= heightMap.Length)
            return 0;

        return heightMap[x];
    }
}
