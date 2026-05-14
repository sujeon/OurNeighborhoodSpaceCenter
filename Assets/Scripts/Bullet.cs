using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private Vector2 startPosition; 
    private bool isLanded = false; 
    [HideInInspector] public ProjectileLauncher launcher; 

    public GameObject distancePopupPrefab;
    public GameObject labBuildingPrefab; 
    public float labYOffset = 0.5f;

    void Start() { startPosition = transform.position; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return; 
        string hitTag = collision.gameObject.tag;

        if (hitTag == "MountainTop")
        {
            isLanded = true; StopProjectile();
            ResourceManager.Instance.AddAllResources(5 + UpgradeManager.BonusAmount); // ★ 이름 일치
            StartCoroutine(FinishTurn(false)); 
        }
        else if (hitTag == "Mountain")
        {
            isLanded = true; StopProjectile();
            StartCoroutine(FinishTurn(true));
        }
        else if (hitTag.Contains("Spot"))
        {
            isLanded = true; StopProjectile();
            LabGenerator lab = collision.gameObject.GetComponent<LabGenerator>();
            if (lab != null)
            {
                bool isNew = lab.ActivateLab();
                string subject = hitTag.Replace("Spot", "");
                int score = 3 + UpgradeManager.BonusAmount; // ★ 이름 일치
                ResourceManager.Instance.AddResource(subject, score);
                if (isNew) {
                    Vector3 spawnPos = collision.transform.position + new Vector3(0, labYOffset, 0);
                    GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity);
                    builtLab.transform.SetParent(collision.transform);
                }
                StartCoroutine(FinishTurn(!isNew)); 
            }
        }
        else if (hitTag == "Ground")
        {
            isLanded = true; StopProjectile();
            float distance = Vector2.Distance(startPosition, transform.position);
            if (distancePopupPrefab != null) {
                GameObject popup = Instantiate(distancePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
                popup.GetComponent<DistancePopup>().Setup(distance);
            }
            ResourceManager.Instance.AddResource("Physics", 1 + (int)(distance / 10f) + UpgradeManager.BonusAmount);
            StartCoroutine(FinishTurn(true));
        }
    }

    void StopProjectile()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    IEnumerator FinishTurn(bool shouldDestroy)
    {
        yield return new WaitForSeconds(2.0f);
        if (launcher != null) launcher.ResetCamera(); // ★ 에러 해결 포인트
        if (shouldDestroy) Destroy(gameObject);
    }
}