using UnityEngine;
using TMPro;

public class DistancePopup : MonoBehaviour
{
    public float moveSpeed = 1.5f; // 위로 올라가는 속도
    public float destroyTime = 2.0f; // 사라지기까지의 시간
    private TextMeshPro textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // 외부(Bullet.cs)에서 비거리 값을 넘겨줄 함수
    public void Setup(float distance)
    {
        textMesh.text = $"{distance:F1}m!"; // 소수점 한자리까지 표시
        Destroy(gameObject, destroyTime); // 2초 뒤에 자동 삭제
    }

    void Update()
    {
        // 매 프레임마다 위쪽(Y축)으로 이동
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
    }
}