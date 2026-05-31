using UnityEngine;

public class MoonCraterZone : MonoBehaviour
{
    [Header("크레이터 정보")]
    [SerializeField] private string craterName = "Crater";
    [SerializeField] private int landingReward = 10;

    [Header("건설 위치")]
    [SerializeField] private Transform buildPoint;
    [SerializeField] private float buildYOffset = 1.0f;

    [Header("상태")]
    [SerializeField] private bool isDiscovered = false;
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private MoonLabType builtLabType = MoonLabType.None;
    [SerializeField] private GameObject builtLabObject;

    [Header("포탄 처리")]
    [SerializeField] private bool stopProjectileOnEnter = true;
    [SerializeField] private bool registerOnlyOnce = true;
    [SerializeField] private float destroyDelay = 0.5f;

    public string CraterName => craterName;
    public int LandingReward => landingReward;
    public bool IsDiscovered => isDiscovered;
    public bool IsOccupied => isOccupied;
    public MoonLabType BuiltLabType => builtLabType;
    public GameObject BuiltLabObject => builtLabObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball"))
            return;

        HandleProjectileArrived(other);
    }

    private void HandleProjectileArrived(Collider2D projectile)
    {
        if (stopProjectileOnEnter)
            StopProjectile(projectile);

        if (registerOnlyOnce && isDiscovered)
        {
            GameLogUI.Log($"{craterName}은 이미 발견한 크레이터입니다.");
            return;
        }

        DiscoverCrater();
    }

    private void DiscoverCrater()
    {
        isDiscovered = true;

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.AddAllResources(landingReward);

        if (MoonBuildManager.Instance != null)
            MoonBuildManager.Instance.RegisterCrater(this);

        GameLogUI.Log($"{craterName} 도착! 모든 자원 +{landingReward}, 건설 후보지 등록");

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    private void StopProjectile(Collider2D projectile)
    {
        Bullet bullet = projectile.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.ForceLandAndDestroy(destroyDelay);
            return;
        }

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        Destroy(projectile.gameObject, destroyDelay);
    }

    public Vector3 GetBuildPosition()
    {
        Vector3 basePosition = buildPoint != null ? buildPoint.position : transform.position;
        return basePosition + new Vector3(0f, buildYOffset, 0f);
    }

    public bool CanBuild()
    {
        return isDiscovered && !isOccupied;
    }

    public bool TrySetBuiltLab(MoonLabType labType, GameObject labObject)
    {
        if (!CanBuild())
            return false;

        builtLabType = labType;
        builtLabObject = labObject;
        isOccupied = true;
        return true;
    }

    public void RemoveBuiltLab()
    {
        if (builtLabObject != null)
            Destroy(builtLabObject);

        builtLabObject = null;
        builtLabType = MoonLabType.None;
        isOccupied = false;
    }

    public void LoadState(bool discovered, bool occupied, MoonLabType labType)
    {
        isDiscovered = discovered;
        isOccupied = occupied;
        builtLabType = labType;
    }

    public void SetBuiltLabFromSave(MoonLabType labType, GameObject labObject)
    {
        builtLabType = labType;
        builtLabObject = labObject;
        isOccupied = labType != MoonLabType.None && labObject != null;
        isDiscovered = true;
    }
}
