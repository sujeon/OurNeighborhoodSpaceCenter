using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Cinemachine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("참조 객체")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public Slider powerSlider;
    public CinemachineCamera vcamProjectile; // 포탄 추적용 카메라 (Priority가 낮은 쪽)
    public CinemachineCamera vcamLauncher;   // 대포 비추는 카메라 (Priority가 높은 쪽)

    [Header("발사 설정")]
    public float minPower = 5f;
    public float baseMaxPower = 30f; // 기본 최대 파워
    public float chargeSpeed = 15f;
    public float rotationSpeed = 50f; // ★ 누락되었던 회전 속도 변수 추가
    
    [Header("궤적 설정")]
    public int baseStepCount = 50; // 기본 궤적 길이
    public float timeStep = 0.05f;

    [Header("각도 설정")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    private float currentAngle = 0f;
    
    private float currentPower;
    private bool isCharging = false;

    void Start()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = minPower;
            // 업그레이드 매니저가 있다면 시작 시 최대값 설정 가능
            powerSlider.maxValue = baseMaxPower; 
        }
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // 1. 각도 조절 로직
        float angleInput = Input.GetAxis("Vertical"); 
        currentAngle += angleInput * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        // 2. 파워 충전 로직
        HandleCharging();
    }

    void HandleCharging()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentPower = minPower;
            lineRenderer.enabled = true;
        }

        if (isCharging)
        {
            currentPower += chargeSpeed * Time.deltaTime;
            
            // ★ 업그레이드 반영: 대포 사거리(최대 파워) 증가
            float upgradedMaxPower = baseMaxPower + (UpgradeManager.Instance != null ? UpgradeManager.Instance.rangeLevel * 5f : 0);
            currentPower = Mathf.Clamp(currentPower, minPower, upgradedMaxPower);
            
            if (powerSlider != null) {
                powerSlider.maxValue = upgradedMaxPower;
                powerSlider.value = currentPower;
            }
            DrawTrajectory(currentPower);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Launch(currentPower);
            isCharging = false;
            lineRenderer.enabled = false;
        }
    }

    void DrawTrajectory(float power)
    {
        List<Vector3> points = new List<Vector3>();
        Vector2 startingPosition = firePoint.position;
        Vector2 startingVelocity = firePoint.right * power;
        
        // ★ 업그레이드 반영: 궤적 예측 장비 (길이 증가)
        int upgradedStepCount = baseStepCount + (UpgradeManager.Instance != null ? UpgradeManager.Instance.trajectoryLevel * 10 : 0);
        
        float gravity = Physics2D.gravity.y * projectilePrefab.GetComponent<Rigidbody2D>().gravityScale;

        for (float t = 0; t < upgradedStepCount * timeStep; t += timeStep)
        {
            float x = startingVelocity.x * t;
            float y = startingVelocity.y * t + 0.5f * gravity * t * t;
            points.Add(new Vector3(startingPosition.x + x, startingPosition.y + y, 0));
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    void Launch(float power)
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // ★ 업그레이드 반영: 포탄 무게 경량화 (질량 감소)
            float upgradedMass = 1.0f - (UpgradeManager.Instance != null ? UpgradeManager.Instance.weightLevel * 0.1f : 0);
            rb.mass = Mathf.Max(upgradedMass, 0.5f); // 최소 무게 제한

            rb.AddForce(firePoint.right * power, ForceMode2D.Impulse);
        }

        // ★ 카메라 시스템 연결
        if (vcamProjectile != null)
        {
            vcamProjectile.Follow = bullet.transform;
            // 포탄 카메라의 우선순위를 높여 화면 전환
            vcamProjectile.Priority = 20; 
        }
    }

    // 포탄이 안착했을 때 다시 대포를 비추기 위한 함수
    public void ResetCamera()
    {
        if (vcamProjectile != null)
        {
            vcamProjectile.Priority = 5; // 점수를 낮춰서 vcamLauncher가 다시 보이게 함
            vcamProjectile.Follow = null;
        }
    }
}