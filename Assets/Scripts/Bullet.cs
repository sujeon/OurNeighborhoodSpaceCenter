using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isAnchored = false;

    void Awake()
    {
<<<<<<< HEAD
        rb = GetComponent<Rigidbody2D>();
    }

    // 유니티 물리 엔진이 충돌 시 자동으로 호출함
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 안착했다면 중복 실행 방지
        if (collision.gameObject.CompareTag("Ground"))
=======
        rb = rb.GetComponent<Rigidbody2D>();
    }

    // 유니티 물리 엔진이 충돌 시 자동으로 호출함
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 안착했다면 중복 실행 방지
        if (isAnchored) return;

        if (collision.gameObject.CompareTag("ground"))
>>>>>>> 65be77f67e6fe848173b4d345d5ed5997ed21dca
        {
            isAnchored = true;
            StopProjectile(collision);
        }
<<<<<<< HEAD
        if (isAnchored) return;
=======
>>>>>>> 65be77f67e6fe848173b4d345d5ed5997ed21dca
    }

    void StopProjectile(Collision2D collision)
    {
        // 1. 물리 정지 (최신 표준 방식)
        rb.bodyType = RigidbodyType2D.Static; 
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 2. 바닥에 박힌 각도 조절 (디테일 연출)
        transform.up = -collision.contacts[0].normal;

        Debug.Log("우리 동네 우주센터 장비가 안전하게 안착했습니다!");
        
        // 여기에 9주차 과제인 '자원 수집 시작' 함수를 호출하면 됩니다.
    }
<<<<<<< HEAD
}

=======
}
>>>>>>> 65be77f67e6fe848173b4d345d5ed5997ed21dca
