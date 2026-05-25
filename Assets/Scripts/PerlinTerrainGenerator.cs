using UnityEngine;
using UnityEngine.Tilemaps;

public class PerlinTerrainGenerator : MonoBehaviour
{
   [Header("Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase surfaceTile;

    [Header("맵 크기")]
    [SerializeField] private int mapWidth = 120;
    [SerializeField] private int baseHeight = 4;
    [SerializeField] private int heightMultiplier = 6;

    [Header("펄린 노이즈")]
    [SerializeField] private float noiseScale = 0.08f;
    [SerializeField] private float seed = 1234f;

    [Header("크레이터")]
    [SerializeField] private bool useCrater = false;
    [SerializeField] private int craterCenterX = 80;
    [SerializeField] private int craterRadius = 10;
    [SerializeField] private int craterDepth = 5;

    private int[] heightMap;

    [ContextMenu("Generate Terrain")]
    public void GenerateTerrain()
    {
        if (groundTilemap == null || groundTile == null)
        {
            Debug.LogWarning("Tilemap 또는 Tile이 연결되지 않았습니다.");
            return;
        }

        heightMap = new int[mapWidth];
        groundTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            int terrainHeight = CalculateHeight(x);
            heightMap[x] = terrainHeight;

            for (int y = 0; y <= terrainHeight; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (surfaceTile != null && y == terrainHeight)
                    groundTilemap.SetTile(position, surfaceTile);
                else
                    groundTilemap.SetTile(position, groundTile);
            }
        }

        Debug.Log("사이드뷰 지형 생성 완료");
    }

    private int CalculateHeight(int x)
    {
        float noiseValue = Mathf.PerlinNoise(x * noiseScale, seed);
        int height = Mathf.RoundToInt(noiseValue * heightMultiplier) + baseHeight;

        if (useCrater)
        {
            height -= GetCraterDepthAtX(x);
        }

        return Mathf.Max(1, height);
    }

    private int GetCraterDepthAtX(int x)
    {
        float distance = Mathf.Abs(x - craterCenterX);

        if (distance > craterRadius)
            return 0;

        float normalizedDistance = distance / craterRadius;
        float craterCurve = 1f - normalizedDistance * normalizedDistance;

        return Mathf.RoundToInt(craterCurve * craterDepth);
    }

    public int GetHeightAtX(int x)
    {
        if (heightMap == null || x < 0 || x >= heightMap.Length)
            return 0;

        return heightMap[x];
    }

    public Vector3 GetWorldPositionAtX(int x, float yOffset = 1f)
    {
        int height = GetHeightAtX(x);
        Vector3Int cellPosition = new Vector3Int(x, height, 0);
        Vector3 worldPosition = groundTilemap.CellToWorld(cellPosition);

        return worldPosition + new Vector3(0.5f, yOffset, 0f);
    }
}