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
        // 1. 포탄 생성
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // 업그레이드 반영: 포탄 무게 경량화 (질량 감소)
            float upgradedMass = 1.0f - (UpgradeManager.Instance != null ? UpgradeManager.Instance.weightLevel * 0.1f : 0);
            rb.mass = Mathf.Max(upgradedMass, 0.5f); 

            // 질량(Mass)의 영향을 받는 발사 방식 (AddForce + Impulse)
            rb.AddForce(firePoint.right * power, ForceMode2D.Impulse);
        }

        // 2. [카메라 전환] 포탄 추적 시작
        if (vcamProjectile != null)
        {
            vcamProjectile.Follow = bullet.transform; // 생성된 포탄을 타겟으로 설정
            
            // 포탄 카메라의 우선순위를 런처 카메라(10)보다 높게 설정 (20)
            // 이 순간 화면이 포탄으로 부드럽게 넘어갑니다.
            vcamProjectile.Priority = 20; 
        }
    }

    // 포탄이 안착하거나 삭제되었을 때 (Bullet.cs의 코루틴에서 호출됨)
    public void ResetCamera()
    {
        if (vcamProjectile != null)
        {
            // 3. [카메라 복귀] 포탄 카메라의 우선순위를 다시 낮춤 (5)
            // 그러면 우선순위가 더 높은 vcamLauncher(10)가 다시 메인 화면이 됩니다.
            vcamProjectile.Priority = 5;
            
            // 다음 발사를 위해 타겟 해제
            vcamProjectile.Follow = null;
        }
    }
}