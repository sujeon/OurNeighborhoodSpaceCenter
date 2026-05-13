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
    public CinemachineCamera vcamProjectile; 
    public CinemachineCamera vcamLauncher;  
    public CinemachineBrain camBrain; // ★ 메인 카메라의 Cinemachine Brain을 드래그해서 넣으세요.

    [Header("발사 설정")]
    public float minPower = 5f;
    // ★ UpgradeManager에서 직접 수정할 수 있도록 public으로 둡니다.
    public float maxForce = 30f; 
    public float chargeSpeed = 15f;
    public float rotationSpeed = 50f; 
    
    [Header("궤적 설정")]
    // ★ UpgradeManager에서 직접 수정할 수 있도록 public으로 둡니다.
    public int trajectoryStepCount = 50; 
    public float timeStep = 0.05f;

    [Header("각도 설정")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    private float currentAngle = 0f;
    
    private float currentPower;
    private bool isCharging = false;

    void Start()
    {
        // 초기값 설정
        if (powerSlider != null)
        {
            powerSlider.minValue = minPower;
            powerSlider.maxValue = maxForce; 
        }
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // 연구소 UI가 열려있을 때(Time.timeScale == 0) 입력 차단
        if (Time.timeScale == 0) return;
        if (IsCameraMoving()) return;

        // 각도 조절
        float angleInput = Input.GetAxis("Vertical"); 
        currentAngle += angleInput * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

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
            
            // ★ 업그레이드 반영: UpgradeManager에 설정된 실시간 maxForce 사용
            currentPower = Mathf.Clamp(currentPower, minPower, maxForce);
            
            if (powerSlider != null) {
                powerSlider.maxValue = maxForce; // 강화로 늘어난 최대치 반영
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
        
        // ★ 업그레이드 반영: 강화된 궤적 점 개수(trajectoryStepCount) 사용
        float gravity = Physics2D.gravity.y * projectilePrefab.GetComponent<Rigidbody2D>().gravityScale;

        for (float t = 0; t < trajectoryStepCount * timeStep; t += timeStep)
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
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (bulletScript != null)
        {
        bulletScript.launcher = this; 
        }
        
        if (rb != null)
        {
            // ★ 업그레이드 반영: UpgradeManager에 저장된 전역 static 무게값 적용
            rb.mass = UpgradeManager.ProjectileMass; 

            // 발사!
            rb.AddForce(firePoint.right * power, ForceMode2D.Impulse);
        }

        // 2. 카메라 전환
        if (vcamProjectile != null)
        {
            vcamProjectile.Follow = bullet.transform;
            vcamProjectile.Priority = 20; 
        }
    }

    public void ResetCamera()
    {
        if (vcamProjectile != null)
        {
            vcamProjectile.Priority = 5;
            vcamProjectile.Follow = null;
        }
    }
    bool IsCameraMoving()
    {
        if (camBrain == null) return false;

        // 카메라가 현재 섞이고(Blending) 있다면 true 반환
        // 혹은 현재 활성화된 카메라가 대포 카메라(vcamLauncher)가 아니어도 true 반환
        return camBrain.IsBlending || (camBrain.ActiveVirtualCamera as CinemachineCamera != vcamLauncher);
    }
}
