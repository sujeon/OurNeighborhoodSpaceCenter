using UnityEngine;

public class MoonCraterZone : MonoBehaviour
{
    [Header("크레이터 정보")]
    public string craterName = "Crater";
    public Transform buildPoint;

    [Header("착륙 보상")]
    public int landingReward = 5;

    private bool isOccupied = false;
    private bool isBuildable = false;

    public bool IsOccupied => isOccupied;
    public bool IsBuildable => isBuildable;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOccupied) return;

        if (!other.CompareTag("Ball"))
            return;

        isBuildable = true;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddAllResources(landingReward);
        }

        MoonBuildManager.Instance.SetCurrentCrater(this);

        Debug.Log($"{craterName} 도착! 이 위치에 연구소를 지을 수 있습니다.");
    }

    public Vector3 GetBuildPosition()
    {
        if (buildPoint != null)
            return buildPoint.position;

        return transform.position;
    }

    public void MarkOccupied()
    {
        isOccupied = true;
        isBuildable = false;
    }
}