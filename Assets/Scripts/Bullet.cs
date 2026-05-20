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

    [Tooltip("포탄 이미지가 오른쪽을 바라보면 0, 위를 바라보면 -90, 아래를 바라보면 90")]
    [SerializeField] private float spriteAngleOffset = 0f;

    [Tooltip("회전이 너무 딱딱하면 값을 낮추세요. 즉시 회전은 999")]
    [SerializeField] private float rotationLerpSpeed = 15f;

    [Tooltip("이 속도보다 느리면 회전을 멈춤")]
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isLanded && rotateToVelocity)
        {
            UpdateProjectileRotation();
        }
    }

    private void UpdateProjectileRotation()
    {
        if (rb == null) return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude < minRotateSpeed * minRotateSpeed)
            return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationLerpSpeed * Time.deltaTime
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return;

        string hitTag = collision.gameObject.tag;

        if (hitTag == "MountainTop")
        {
            isLanded = true;
            StopProjectile();
            ResourceManager.Instance.AddAllResources(5 + UpgradeManager.BonusAmount);
            StartCoroutine(FinishTurn(false));
        }
        else if (hitTag == "Mountain")
        {
            isLanded = true;
            StopProjectile();
            StartCoroutine(FinishTurn(true));
        }
        else if (hitTag == "MapBoundary")
        {
            isLanded = true;
            StopProjectile();

            Debug.Log("맵 끝에 도달했습니다!");

            StartCoroutine(FinishTurn(true));
        }
        else if (hitTag.Contains("Spot"))
        {
            isLanded = true;
            StopProjectile();
            PlayLandingFeedback(transform.position);

            LabGenerator lab = collision.gameObject.GetComponent<LabGenerator>();

            if (lab != null)
            {
                bool isNew = lab.ActivateLab();

                string subject = hitTag.Replace("Spot", "");
                int score = 3 + UpgradeManager.BonusAmount;

                ResourceManager.Instance.AddResource(subject, score);

                if (isNew)
                {
                    Vector3 spawnPos = collision.transform.position + new Vector3(0, labYOffset, 0);
                    GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity);
                    builtLab.transform.SetParent(collision.transform);
                    PlayBuildLabFeedback(spawnPos);
                }

                StartCoroutine(FinishTurn(!isNew));
            }
        }
        else if (hitTag == "Ground")
        {
            isLanded = true;
            StopProjectile();

            PlayLandingFeedback(transform.position);

            float distance = Vector2.Distance(startPosition, transform.position);

            if (distancePopupPrefab != null)
            {
                GameObject popup = Instantiate(
                    distancePopupPrefab,
                    transform.position + Vector3.up,
                    Quaternion.identity
                );

                popup.GetComponent<DistancePopup>().Setup(distance);
            }

            ResourceManager.Instance.AddResource(
                "Physics",
                1 + (int)(distance / 10f) + UpgradeManager.BonusAmount
            );

            StartCoroutine(FinishTurn(true));
        }
    }

    void StopProjectile()
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

        AudioSource.PlayClipAtPoint(clip, position, volume);
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
    IEnumerator FinishTurn(bool shouldDestroy)
    {
        yield return new WaitForSeconds(2.0f);

        if (launcher != null)
            launcher.ResetCamera();

        if (shouldDestroy)
            Destroy(gameObject);
    }
}