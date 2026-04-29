using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
<<<<<<< HEAD
using Unity.Cinemachine;
=======
using Unity.Cinemachine; // 최신 버전 기준 (에러 시 using Cinemachine; 사용)
>>>>>>> 65be77f67e6fe848173b4d345d5ed5997ed21dca

public class ProjectileLauncher : MonoBehaviour
{
    [Header("참조 객체")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public Slider powerSlider;
    public CinemachineCamera vcam; // 포탄 추적용 카메라

    [Header("발사 설정")]
    public float minPower = 5f;
    public float maxPower = 30f;
    public float chargeSpeed = 15f;
    public int stepCount = 50;
    public float timeStep = 0.05f;

    private float angle = 45f;
    private float currentPower;
    private bool isCharging = false;

    void Start()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = minPower;
            powerSlider.maxValue = maxPower;
        }
        lineRenderer.enabled = false; // 시작할 땐 선 숨기기
    }

    void Update()
    {
        // 1. 각도 조절 (위/아래 화살표)
        float angleInput = Input.GetAxis("Vertical");
        angle += angleInput * 50f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, angle);

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
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower);
            if (powerSlider != null) powerSlider.value = currentPower;
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

        for (float t = 0; t < stepCount * timeStep; t += timeStep)
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
            rb.linearVelocity = firePoint.right * power; // 최신 표준 linearVelocity 사용
        }

        // 카메라가 포탄을 따라가도록 설정
<<<<<<< HEAD
        if (vcam != null)
        {
             vcam.Follow = bullet.transform;
        }
=======
        if (vcam != null) vcam.Follow = bullet.transform;
>>>>>>> 65be77f67e6fe848173b4d345d5ed5997ed21dca
    }
}