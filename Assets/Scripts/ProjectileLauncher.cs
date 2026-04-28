using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject projectilePrefab; // 포탄 프리팹 
    public Transform firePoint;         // 발사 위치 (대포 끝 자식 객체) 
    public float launchForce = 15f;     // 발사 힘 (Power) 
    
    [Header("궤적 시각화")]
    public LineRenderer lineRenderer;   // 경로를 그릴 컴포넌트 
    public int stepCount = 30;          // 예측 점의 개수 
    public float timeStep = 0.1f;       // 점 사이의 시간 간격 

    private float angle = 45f;          // 현재 발사 각도 

    void Update()
    {
        // 1. 각도 조절 (화살표 키) 
        float angleInput = Input.GetAxis("Vertical");
        angle += angleInput * 50f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. 궤적 실시간 업데이트 
        DrawTrajectory();

        // 3. 발사 (스페이스바) 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Launch();
        }
    }

    void Launch()
    {
        // 포탄 생성 및 대포의 회전값 전달 
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 대포가 바라보는 방향(transform.right)으로 즉각적인 힘 부여 [cite: 1733, 2543]
        rb.AddForce(firePoint.right * launchForce, ForceMode2D.Impulse);
    }

    void DrawTrajectory()
    {
        List<Vector3> points = new List<Vector3>();
        Vector2 startingPosition = firePoint.position;
        Vector2 startingVelocity = firePoint.right * launchForce;

        for (float t = 0; t < stepCount * timeStep; t += timeStep)
        {
            // 물리 공식: P = P0 + V0t + 0.5at^2 적용
            Vector2 newPoint = startingPosition + startingVelocity * t + 0.5f * Physics2D.gravity * t * t;
            points.Add(newPoint);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
