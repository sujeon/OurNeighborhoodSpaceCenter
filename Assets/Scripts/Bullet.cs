using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private Vector2 startPosition;
    private bool isLanded = false;

    [HideInInspector] public ProjectileLauncher launcher;

    public GameObject distancePopupPrefab;
    public GameObject labBuildingPrefab;
    public float labYOffset = 0.5f;

    [Header("포탄 회전 설정")]
    [SerializeField] private bool rotateToVelocity = true;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private float rotationLerpSpeed = 15f;
    [SerializeField] private float minRotateSpeed = 0.1f;

    [Header("착지 사운드 / 이펙트")]
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private GameObject landingEffectPrefab;

    [Header("연구소 생성 사운드 / 이펙트")]
    [SerializeField] private AudioClip buildLabSound;
    [SerializeField] private GameObject buildLabEffectPrefab;

    [Header("사운드 볼륨")]
    [SerializeField] private float landingSoundVolume = 1f;
    [SerializeField] private float buildSoundVolume = 1f;

    private Rigidbody2D rb;
    private Coroutine finishRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!isLanded && rotateToVelocity)
            UpdateProjectileRotation();
    }

    private void UpdateProjectileRotation()
    {
        if (rb == null) return;

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < minRotateSpeed * minRotateSpeed)
            return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return;

        string hitTag = collision.gameObject.tag;

        if (hitTag == "MountainTop")
        {
            Land(false);
            int reward = 5 + UpgradeManager.BonusAmount;
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.AddAllResources(reward);
            GameLogUI.Log($"산 정상 도착! 모든 자원 +{reward}");
        }
        else if (hitTag == "Mountain")
        {
            Land(true);
            GameLogUI.Log("산에 충돌했습니다.");
        }
        else if (hitTag == "MapBoundary")
        {
            Land(true);
            GameLogUI.Log("맵 경계에 도달했습니다.");
        }
        else if (hitTag.Contains("Spot"))
        {
            Land(false);
            PlayLandingFeedback(transform.position);
            HandleScienceSpot(collision);
        }
        else if (hitTag == "Ground")
        {
            Land(false);
            PlayLandingFeedback(transform.position);
            HandleGroundLanding();
        }
    }

    private void HandleScienceSpot(Collision2D collision)
    {
        LabGenerator lab = collision.gameObject.GetComponent<LabGenerator>();
        ResourceType resourceType = ResourceManager.TagToResourceType(collision.gameObject.tag);
        int score = 3 + UpgradeManager.BonusAmount;

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.AddResource(resourceType, score);

        if (lab == null)
        {
            GameLogUI.Log($"{ResourceManager.GetResourceName(resourceType)} 포인트 도착! +{score}");
            StartFinishRoutine(true);
            return;
        }

        bool isNew = lab.ActivateLab();

        if (isNew && labBuildingPrefab != null)
        {
            Vector3 spawnPos = collision.transform.position + new Vector3(0, labYOffset, 0);
            GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity);
            builtLab.transform.SetParent(collision.transform);
            PlayBuildLabFeedback(spawnPos);
            GameLogUI.Log($"{ResourceManager.GetResourceName(resourceType)} 연구소 건설! 포인트 +{score}");
        }
        else
        {
            GameLogUI.Log($"{ResourceManager.GetResourceName(resourceType)} 포인트 도착! +{score}");
        }

        StartFinishRoutine(!isNew);
    }

    private void HandleGroundLanding()
    {
        float distance = Vector2.Distance(startPosition, transform.position);
        int reward = 1 + (int)(distance / 10f) + UpgradeManager.BonusAmount;

        if (distancePopupPrefab != null)
        {
            GameObject popup = Instantiate(distancePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            if (popup.TryGetComponent(out DistancePopup distancePopup))
                distancePopup.Setup(distance);
        }

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.AddResource(ResourceType.Physics, reward);

        GameLogUI.Log($"착지! 거리 {distance:F1}m, 물리 +{reward}");
        StartFinishRoutine(true);
    }

    private void Land(bool destroyAfterDelay)
    {
        isLanded = true;
        StopProjectile();
        StartFinishRoutine(destroyAfterDelay);
    }

    private void StartFinishRoutine(bool shouldDestroy)
    {
        if (finishRoutine != null)
            StopCoroutine(finishRoutine);

        finishRoutine = StartCoroutine(FinishTurn(shouldDestroy));
    }

    private void StopProjectile()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
            return;

        GameObject soundObject = new GameObject("OneShotSound");
        soundObject.transform.position = Camera.main != null ? Camera.main.transform.position : position;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(soundObject, clip.length + 0.1f);
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
            return;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
        Destroy(effect, 2f);
    }

    private void PlayLandingFeedback(Vector3 position)
    {
        PlaySoundAtPosition(landingSound, position, landingSoundVolume);
        SpawnEffect(landingEffectPrefab, position);
    }

    private void PlayBuildLabFeedback(Vector3 position)
    {
        PlaySoundAtPosition(buildLabSound, position, buildSoundVolume);
        SpawnEffect(buildLabEffectPrefab, position);
    }

    public void ForceLandWithoutDestroy()
    {
        if (isLanded) return;

        isLanded = true;
        StopProjectile();

        if (finishRoutine != null)
        {
            StopCoroutine(finishRoutine);
            finishRoutine = null;
        }

        if (launcher != null)
            launcher.ResetCamera();
    }

    public void ForceLandAndDestroy(float delay = 0.5f)
    {
        if (isLanded) return;

        isLanded = true;
        StopProjectile();

        if (launcher != null)
            launcher.ResetCamera();

        Destroy(gameObject, delay);
    }

    private IEnumerator FinishTurn(bool shouldDestroy)
    {
        yield return new WaitForSeconds(2.0f);

        if (launcher != null)
            launcher.ResetCamera();

        if (shouldDestroy)
            Destroy(gameObject);
    }
}
