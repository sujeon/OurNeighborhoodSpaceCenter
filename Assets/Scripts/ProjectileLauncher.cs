using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("참조 객체")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Slider powerSlider;
    public CinemachineBrain camBrain;
    public CinemachineCamera vcamProjectile;
    public CinemachineCamera vcamLauncher;

    [Header("사운드")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip launchSound;

    [Header("발사 설정")]
    public float minPower = 5f;
    public float maxForce = 10f;
    public float rotationSpeed = 50f;
    public KeyCode fireKey = KeyCode.Space;

    [Header("파워 차징 속도")]
    [SerializeField] private float minChargeSpeed = 8f;
    [SerializeField] private float maxChargeSpeed = 35f;
    [SerializeField] private AnimationCurve chargeSpeedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("중력 / 탄도 설정")]
    public float projectileGravityScale = 1f;
    public string moonSceneName = "MoonStage";

    [Header("궤적 설정")]
    [Range(10, 200)] public int trajectoryStepCount = 50;
    [Range(0.02f, 0.2f)] public float timeStep = 0.05f;
    public LayerMask groundLayer;

    [Header("각도 설정")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    [SerializeField] private float currentAngle = 0f;

    [Header("점선 궤적 설정")]
    [SerializeField] private GameObject trajectoryDotPrefab;
    [SerializeField] private int maxDotCount = 80;
    [SerializeField] private int dotSpacing = 3;
    [SerializeField] private Transform trajectoryDotParent;

    private readonly List<GameObject> trajectoryDots = new List<GameObject>();
    private readonly Vector3[] trajectoryPoints = new Vector3[200];

    private float currentPower;
    private bool isCharging;
    private bool hasPlayedChargeSound;
    private Rigidbody2D projectileRbTemplate;

    private void Awake()
    {
        CacheProjectileTemplate();
    }

    private void Start()
    {
        SetupSlider();
        CreateTrajectoryDots();
        SetLineVisible(false);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ApplyUpgradeToLauncher(this);
    }

    private void Update()
    {
        if (Time.timeScale == 0f || IsCameraMoving()) return;

        RotateLauncher();
        HandleCharging();
    }

    private void CacheProjectileTemplate()
    {
        if (projectilePrefab != null)
            projectileRbTemplate = projectilePrefab.GetComponent<Rigidbody2D>();
    }

    private void SetupSlider()
    {
        if (powerSlider == null) return;

        powerSlider.minValue = minPower;
        powerSlider.maxValue = maxForce;
        powerSlider.value = minPower;
    }

    private void CreateTrajectoryDots()
    {
        if (trajectoryDotPrefab == null)
            return;

        EnsureTrajectoryDotCount(maxDotCount);
    }

    private void EnsureTrajectoryDotCount(int neededDotCount)
    {
        if (trajectoryDotPrefab == null)
            return;

        while (trajectoryDots.Count < neededDotCount)
        {
            GameObject dot = Instantiate(trajectoryDotPrefab, transform.position, Quaternion.identity);

            if (trajectoryDotParent != null)
                dot.transform.SetParent(trajectoryDotParent, true);

            dot.SetActive(false);
            trajectoryDots.Add(dot);
        }
    }

    private void RotateLauncher()
    {
        float angleInput = Input.GetAxisRaw("Vertical");
        if (Mathf.Approximately(angleInput, 0f)) return;

        currentAngle += angleInput * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    private bool IsCameraMoving()
    {
        if (camBrain == null || vcamLauncher == null) return false;

        var activeCam = camBrain.ActiveVirtualCamera;
        return camBrain.IsBlending || activeCam != (ICinemachineCamera)vcamLauncher;
    }

    private void HandleCharging()
    {
        if (Input.GetKeyDown(fireKey))
            StartCharging();

        if (isCharging)
        {
            ChargePower();
            DrawTrajectory(currentPower);
        }

        if (Input.GetKeyUp(fireKey) && isCharging)
            FireChargedProjectile();
    }

    private void StartCharging()
    {
        isCharging = true;
        currentPower = minPower;
        SetLineVisible(true);
        UpdatePowerSlider();
        PlayChargeSound();
    }

    private void ChargePower()
{
    float powerRatio = Mathf.InverseLerp(minPower, maxForce, currentPower);

    float curveValue = chargeSpeedCurve.Evaluate(powerRatio);

    float currentChargeSpeed = Mathf.Lerp(minChargeSpeed, maxChargeSpeed, curveValue);

    currentPower = Mathf.Clamp(
        currentPower + currentChargeSpeed * Time.deltaTime,
        minPower,
        maxForce
    );

    UpdatePowerSlider();
    DrawTrajectory(currentPower);
}

    private void FireChargedProjectile()
    {
        StopChargeSound();
        Launch(currentPower);
        isCharging = false;
        SetLineVisible(false);
    }

    private void UpdatePowerSlider()
    {
        if (powerSlider == null) return;

        powerSlider.maxValue = maxForce;
        powerSlider.value = currentPower;
    }

    private void SetLineVisible(bool visible)
    {
        if (!visible)
            HideAllTrajectoryDots();
    }

    private void HideAllTrajectoryDots()
    {
        for (int i = 0; i < trajectoryDots.Count; i++)
        {
            if (trajectoryDots[i] != null)
                trajectoryDots[i].SetActive(false);
        }
    }

    private float GetEffectiveGravityScale()
    {
        float upgradeGravityScale = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.ProjectileGravityScale
            : 1f;

        return Mathf.Max(0.05f, projectileGravityScale * upgradeGravityScale);
    }

    private void DrawTrajectory(float power)
    {
        if (firePoint == null) return;

        int maxCount = Mathf.Min(trajectoryStepCount, trajectoryPoints.Length);
        Vector2 startPos = firePoint.position;
        Vector2 startVelocity = firePoint.right * power;
        float gravity = Physics2D.gravity.y * GetEffectiveGravityScale();

        Vector2 previousPosition = startPos;
        trajectoryPoints[0] = startPos;
        int pointCount = 1;

        for (int i = 1; i < maxCount; i++)
        {
            float t = i * timeStep;
            Vector2 currentPosition = startPos + startVelocity * t + 0.5f * Vector2.up * gravity * t * t;

            RaycastHit2D hit = Physics2D.Linecast(previousPosition, currentPosition, groundLayer);
            if (hit.collider != null)
            {
                trajectoryPoints[pointCount++] = hit.point;
                break;
            }

            trajectoryPoints[pointCount++] = currentPosition;
            previousPosition = currentPosition;
        }

        UpdateTrajectoryDots(pointCount);
    }

    private void UpdateTrajectoryDots(int pointCount)
    {
        if (trajectoryDotPrefab == null)
            return;

        int safeSpacing = Mathf.Max(1, dotSpacing);
        int neededDotCount = Mathf.CeilToInt(pointCount / (float)safeSpacing);
        EnsureTrajectoryDotCount(neededDotCount);

        int dotIndex = 0;
        for (int i = 0; i < pointCount; i += safeSpacing)
        {
            if (dotIndex >= trajectoryDots.Count)
                break;

            GameObject dot = trajectoryDots[dotIndex];
            if (dot != null)
            {
                dot.transform.position = trajectoryPoints[i];
                dot.SetActive(true);
            }

            dotIndex++;
        }

        for (int i = dotIndex; i < trajectoryDots.Count; i++)
        {
            if (trajectoryDots[i] != null)
                trajectoryDots[i].SetActive(false);
        }
    }

    private void Launch(float power)
    {
        PlayLaunchSound();

        if (ShouldGoToMoon(power))
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryConsumeMoonLaunchCost())
            {
                GameLogUI.Log("달 발사 성공! MoonStage로 이동합니다.");
                StartCoroutine(LoadMoonStageAfterSound());
            }
            return;
        }

        if (projectilePrefab == null || firePoint == null)
        {
            GameLogUI.Warning("발사 실패: 포탄 프리팹 또는 FirePoint가 연결되지 않았습니다.");
            return;
        }

        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (bulletObj.TryGetComponent(out Bullet bulletScript))
            bulletScript.launcher = this;

        if (bulletObj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.mass = UpgradeManager.ProjectileMass;
            rb.gravityScale = GetEffectiveGravityScale();
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.AddForce(firePoint.right * power, ForceMode2D.Impulse);
        }

        if (vcamProjectile != null)
        {
            vcamProjectile.Follow = bulletObj.transform;
            vcamProjectile.Priority = 20;
        }

        GameLogUI.Log($"포탄 발사! 파워 {power:F1}, 각도 {currentAngle:F0}°");
    }

    private bool ShouldGoToMoon(float power)
    {
        if (UpgradeManager.Instance == null)
            return false;

        bool canLaunchToMoon = UpgradeManager.Instance.CanLaunchToMoon();
        bool isStraightUp = currentAngle >= 89f;
        bool isMaxPower = power >= maxForce - 0.5f;

        return canLaunchToMoon && isStraightUp && isMaxPower;
    }

    private void PlayChargeSound()
    {
        if (hasPlayedChargeSound)
            return;

        if (sfxSource == null || chargeSound == null)
            return;

        sfxSource.PlayOneShot(chargeSound);
        hasPlayedChargeSound = true;
    }

    private void StopChargeSound()
    {
        hasPlayedChargeSound = false;
    }

    private void PlayLaunchSound()
    {
        if (sfxSource == null || launchSound == null)
            return;

        sfxSource.PlayOneShot(launchSound);
    }

    private IEnumerator LoadMoonStageAfterSound()
    {
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(moonSceneName);
    }

    public void ResetCamera()
    {
        if (vcamProjectile == null) return;

        vcamProjectile.Priority = 5;
        vcamProjectile.Follow = null;
    }
}
