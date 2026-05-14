using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
    
    [Header("궤적 설정")]
    public int trajectoryStepCount = 50; 
    public float timeStep = 0.05f;
    public LayerMask groundLayer; 

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
            powerSlider.maxValue = maxForce; 
        }
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (IsCameraMoving()) return;

        float angleInput = Input.GetAxis("Vertical"); 
        currentAngle += angleInput * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        HandleCharging();
    }

   bool IsCameraMoving()
    {
    if (camBrain == null) return false;
    var activeCam = camBrain.ActiveVirtualCamera;
    return camBrain.IsBlending || (activeCam != (ICinemachineCamera)vcamLauncher);
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
            currentPower = Mathf.Clamp(currentPower, minPower, maxForce);
            
            if (powerSlider != null) {
                powerSlider.maxValue = maxForce;
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
        float gravity = Physics2D.gravity.y * projectilePrefab.GetComponent<Rigidbody2D>().gravityScale;

        Vector2 previousPosition = startingPosition;
        points.Add(startingPosition);

        for (int i = 1; i < trajectoryStepCount; i++) 
        {
            float t = i * timeStep;
            Vector2 currentPosition = startingPosition + startingVelocity * t + 0.5f * Vector2.up * gravity * t * t;
            
            RaycastHit2D hit = Physics2D.Linecast(previousPosition, currentPosition, groundLayer);
            if (hit.collider != null) {
                points.Add(hit.point);
                break; 
            }
            points.Add(currentPosition);
            previousPosition = currentPosition;
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    void Launch(float power)
    {
        // 달 해금 미션 체크
        if (UpgradeManager.IsMoonUnlocked && currentAngle >= 89f && power >= maxForce - 0.5f)
        {
            SceneManager.LoadScene("MoonStage");
            return;
        }

        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null) bulletScript.launcher = this; // ★ 주인 연결

        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.mass = UpgradeManager.ProjectileMass; 
            rb.AddForce(firePoint.right * power, ForceMode2D.Impulse);
        }

        if (vcamProjectile != null) {
            vcamProjectile.Follow = bulletObj.transform;
            vcamProjectile.Priority = 20; 
        }
    }

    public void ResetCamera()
    {
        if (vcamProjectile != null) {
            vcamProjectile.Priority = 5;
            vcamProjectile.Follow = null;
        }
    }
}