using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [HideInInspector] public ProjectileLauncher launcher;

    [Header("착륙 연출")]
    public GameObject distancePopupPrefab;
    public GameObject labBuildingPrefab;
    public float labYOffset = 0.5f;
    public float finishDelay = 2.0f;

    [Header("보상 설정")]
    public int mountainTopReward = 5;
    public int spotReward = 3;
    public int groundBaseReward = 1;
    public float groundDistanceRewardUnit = 10f;

    private Vector2 startPosition;
    private Rigidbody2D rb;
    private bool isLanded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return;
        if (ResourceManager.Instance == null) return;

        GameObject hitObject = collision.gameObject;

        if (hitObject.CompareTag("MountainTop"))
        {
            Land(false);
            ResourceManager.Instance.AddAllResources(mountainTopReward + UpgradeManager.BonusAmount);
            StartCoroutine(FinishTurn(false));
            return;
        }

        if (hitObject.CompareTag("Mountain"))
        {
            Land(true);
            return;
        }

        if (hitObject.CompareTag("Ground"))
        {
            LandOnGround();
            return;
        }

        if (hitObject.CompareTag("PhysicsSpot") || hitObject.CompareTag("ChemistrySpot") ||
            hitObject.CompareTag("BiologySpot") || hitObject.CompareTag("EarthSpot"))
        {
            LandOnResourceSpot(hitObject);
        }
    }

    private void Land(bool destroyAfterDelay)
    {
        isLanded = true;
        StopProjectile();
        StartCoroutine(FinishTurn(destroyAfterDelay));
    }

    private void LandOnGround()
    {
        isLanded = true;
        StopProjectile();

        float distance = Vector2.Distance(startPosition, transform.position);
        SpawnDistancePopup(distance);

        int reward = groundBaseReward + Mathf.FloorToInt(distance / groundDistanceRewardUnit) + UpgradeManager.BonusAmount;
        ResourceManager.Instance.AddResource(ResourceType.Physics, reward);
        StartCoroutine(FinishTurn(true));
    }

    private void LandOnResourceSpot(GameObject hitObject)
    {
        isLanded = true;
        StopProjectile();

        ResourceType type = ResourceManager.TagToResourceType(hitObject.tag);
        int reward = spotReward + UpgradeManager.BonusAmount;
        ResourceManager.Instance.AddResource(type, reward);

        bool isNewLab = false;
        if (hitObject.TryGetComponent(out LabGenerator lab))
        {
            isNewLab = lab.ActivateLab();
        }

        if (isNewLab && labBuildingPrefab != null)
        {
            Vector3 spawnPos = hitObject.transform.position + new Vector3(0f, labYOffset, 0f);
            GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity, hitObject.transform);
            builtLab.name = $"{type} Lab Building";
        }

        StartCoroutine(FinishTurn(!isNewLab));
    }

    private void SpawnDistancePopup(float distance)
    {
        if (distancePopupPrefab == null) return;

        GameObject popup = Instantiate(distancePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
        if (popup.TryGetComponent(out DistancePopup popupScript))
        {
            popupScript.Setup(distance);
        }
    }

    private void StopProjectile()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
    }

    private IEnumerator FinishTurn(bool shouldDestroy)
    {
        yield return new WaitForSeconds(finishDelay);

        if (launcher != null)
        {
            launcher.ResetCamera();
        }

        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }
}
