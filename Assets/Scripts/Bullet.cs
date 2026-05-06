using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 startPosition; // 발사 위치 저장용
    private bool isLanded = false; // 중복 계산 방지용

    public GameObject distancePopupPrefab;
    void Start()
    {
        // 1. 발사된 순간의 위치를 기억합니다.
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return; // 이미 안착했다면 무시

        string hitTag = collision.gameObject.tag;

        // 일반 바닥("Ground")에 닿았을 때의 로직
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("PhysicsSpot"))
    {
        isLanded = true;
        StopProjectile();

        // 1. 비거리 계산
        float distance = Vector2.Distance(startPosition, transform.position);

        // 2. 팝업 생성 (포탄보다 약간 위쪽 좌표에 생성)
        Vector3 spawnPos = transform.position + new Vector3(0, 1.0f, 0);
        GameObject popup = Instantiate(distancePopupPrefab, spawnPos, Quaternion.identity);
        
        // 3. 거리 데이터 전달
        popup.GetComponent<DistancePopup>().Setup(distance);

        // 4. 거리에 따른 자원 계산
        int bonusResource = 1 + (int)(distance / 10f);

        ResourceManager.Instance.AddResource("Physics", bonusResource);
        Debug.Log($"일반 안착! 거리: {distance:F1} / 획득 자원: {bonusResource}");
        
        }
        // 기존 연구소 지점("Spot") 로직도 유지 가능
        else if (hitTag.Contains("Spot"))
        {
            isLanded = true;
            StopProjectile();
            string subject = hitTag.Replace("Spot", "");
            ResourceManager.Instance.AddResource(subject, 5); // 연구소는 고정 대박 점수(예: 5점)
        }
    }

    void StopProjectile()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }
}



