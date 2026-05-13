using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private Vector2 startPosition; 
    private bool isLanded = false; 

    // ★ [수정] 대포에서 직접 전달받을 참조 변수
    [HideInInspector] public ProjectileLauncher launcher; 

    [Header("UI 및 효과")]
    public GameObject distancePopupPrefab;

    [Header("건설 설정")]
    public GameObject labBuildingPrefab; 
    public float labYOffset = 0.5f;

    void Start()
    {
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return; 

        string hitTag = collision.gameObject.tag;

        // --- 1. 산 꼭대기 안착 (모든 자원 보너스) ---
        if (hitTag == "MountainTop")
        {
            isLanded = true;
            StopProjectile();
            
            // ★ [업그레이드 반영] 기본 5점 + 업그레이드 보너스 점수
            int totalBonus = 5 + UpgradeManager.BonusAmount;
            ResourceManager.Instance.AddAllResources(totalBonus); 

            StartCoroutine(FinishTurn(false)); 
        }
        // --- 2. 산 중턱 안착 (실패) ---
        else if (hitTag == "Mountain")
        {
            isLanded = true;
            StopProjectile();
            Debug.Log("산 중턱에 부딪혔습니다. 점수 없음!");
            StartCoroutine(FinishTurn(true));
        }
        // --- 3. 연구소 스팟 안착 (성공) ---
        else if (hitTag.Contains("Spot"))
        {
            isLanded = true;
            StopProjectile();
        
            LabGenerator lab = collision.gameObject.GetComponent<LabGenerator>();
            if (lab != null)
            {
                bool isNew = lab.ActivateLab();
                string subject = hitTag.Replace("Spot", "");
                
                // ★ [업그레이드 반영] 기본 3점 + 업그레이드 보너스 점수
                int score = 3 + UpgradeManager.BonusAmount;
                ResourceManager.Instance.AddResource(subject, score);
                
                if (isNew)
                {
                    Vector3 spawnPos = collision.transform.position + new Vector3(0, labYOffset, 0);
                    GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity);
                    builtLab.transform.SetParent(collision.transform);
                    
                    Debug.Log($"새로운 {subject} 연구소 점령! {score}점 획득.");
                    StartCoroutine(FinishTurn(false)); 
                }
                else
                {
                    Debug.Log($"{subject} 연구소는 이미 점령된 상태입니다. 추가 점수 획득!");
                    StartCoroutine(FinishTurn(true)); 
                }
            }
        }
        // --- 4. 일반 바닥 안착 (거리 보상) ---
        else if (hitTag == "Ground")
        {
            isLanded = true;
            StopProjectile();

            float distance = Vector2.Distance(startPosition, transform.position);

            if (distancePopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(0, 1.0f, 0);
                GameObject popup = Instantiate(distancePopupPrefab, spawnPos, Quaternion.identity);
                popup.GetComponent<DistancePopup>().Setup(distance);
            }

            // 거리 보상에도 보너스 점수 합산
            int bonusResource = 1 + (int)(distance / 10f) + UpgradeManager.BonusAmount;
            ResourceManager.Instance.AddResource("Physics", bonusResource);
            
            StartCoroutine(FinishTurn(true));
        }
    }

    void StopProjectile()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    IEnumerator FinishTurn(bool shouldDestroy)
    {
        yield return new WaitForSeconds(2.0f);

        // ★ [수정] FindObject 대신 넘겨받은 launcher를 직접 사용하여 에러 방지
        if (launcher != null)
        {
            launcher.ResetCamera();
        }

        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }
}


