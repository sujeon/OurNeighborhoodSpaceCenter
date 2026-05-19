using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("참조 객체")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public Slider powerSlider;
    public CinemachineBrain camBrain;
    public CinemachineCamera vcamProjectile;
    public CinemachineCamera vcamLauncher;

    [Header("발사 설정")]
    public float minPower = 5f;
    public float maxForce = 30f;
    public float chargeSpeed = 15f;
    public float rotationSpeed = 50f;
    public KeyCode fireKey = KeyCode.Space;

    [Header("중력 / 탄도 설정")]
    [Tooltip("발사체가 받는 기본 중력 배율입니다. 스테이지 중력값 * 이 값 * 업그레이드 중력 배율로 계산됩니다.")]
    public float projectileGravityScale = 1f;
    [Tooltip("달 해금 후, 거의 수직 + 최대 파워 발사 시 MoonStage로 이동")]
    public string moonSceneName = "MoonStage";

    [Header("궤적 설정")]
    [Range(10, 200)] public int trajectoryStepCount = 50;
    [Range(0.02f, 0.2f)] public float timeStep = 0.05f;
    public LayerMask groundLayer;

    [Header("각도 설정")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    [SerializeField] private float currentAngle = 0f;

    private float currentPower;
    private bool isCharging;
    private Rigidbody2D projectileRbTemplate;
    private readonly Vector3[] trajectoryPoints = new Vector3[200];

    private void Awake()
    {
        CacheProjectileTemplate();
    }

    private void Start()
    {
        SetupSlider();
        SetLineVisible(false);
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
        {
            projectileRbTemplate = projectilePrefab.GetComponent<Rigidbody2D>();
        }
    }

    private void SetupSlider()
    {
        if (powerSlider == null) return;

        powerSlider.minValue = minPower;
        powerSlider.maxValue = maxForce;
        powerSlider.value = minPower;
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
        {
            StartCharging();
        }

        if (isCharging)
        {
            ChargePower();
            DrawTrajectory(currentPower);
        }

        if (Input.GetKeyUp(fireKey) && isCharging)
        {
            FireChargedProjectile();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentPower = minPower;
        SetLineVisible(true);
        UpdatePowerSlider();
    }

    private void ChargePower()
    {
        currentPower = Mathf.Clamp(currentPower + chargeSpeed * Time.deltaTime, minPower, maxForce);
        UpdatePowerSlider();
    }

    private void FireChargedProjectile()
    {
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
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
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
        if (lineRenderer == null || firePoint == null) return;

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

        lineRenderer.positionCount = pointCount;
        for (int i = 0; i < pointCount; i++)
        {
            lineRenderer.SetPosition(i, trajectoryPoints[i]);
        }
    }

    private void Launch(float power)
    {
        if (ShouldGoToMoon(power))
        {
            Debug.Log("달 발사 성공! MoonStage로 이동합니다.");
            SceneManager.LoadScene(moonSceneName);
            return;
        }

        if (projectilePrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (bulletObj.TryGetComponent(out Bullet bulletScript))
        {
            bulletScript.launcher = this;
        }

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
    public void ResetCamera()
    {
        if (vcamProjectile == null) return;

        vcamProjectile.Priority = 5;
        vcamProjectile.Follow = null;
    }
}
