using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class Bullet : MonoBehaviour
{
    private Vector2 startPosition; // 발사 위치 저장용
    private bool isLanded = false; // 중복 계산 방지용

    [Header("UI 및 효과")]
    public GameObject distancePopupPrefab;

    [Header("건설 설정")]
    public GameObject labBuildingPrefab; // 생성할 연구소 건물 프리팹
    public float labYOffset = 0.5f;

    void Start()
    {
        // 1. 발사된 순간의 위치를 기억합니다.
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLanded) return; // 이미 안착했다면 무시

        string hitTag = collision.gameObject.tag;

        // --- 1. 연구소(Spot) 태그에 닿았을 경우 (성공) ---
        if (hitTag == "MountainTop")
        {
            isLanded = true;
            StopProjectile();
            ResourceManager.Instance.AddAllResources(5); // 보너스!
            StartCoroutine(FinishTurn(false)); 
        }
        else if (hitTag == "Mountain")
        {
            isLanded = true;
            StopProjectile();

            Debug.Log("산 중턱에 부딪혔습니다. 점수 없음!");

            // 실패했으므로 2초 뒤 포탄 삭제 및 카메라 복구 (FinishTurn의 인자를 true로)
            StartCoroutine(FinishTurn(true));
        }
        else if (hitTag.Contains("Spot"))
        {
            isLanded = true;
            StopProjectile();
        
            LabGenerator lab = collision.gameObject.GetComponent<LabGenerator>();
            if (lab != null)
            {
                bool isNew = lab.ActivateLab();
                string subject = hitTag.Replace("Spot", "");
                ResourceManager.Instance.AddResource(subject, 3);
                
                if (isNew)
                {
                    // ★ 수정: 높이 보정값을 적용한 생성 위치 계산
                    Vector3 spawnPos = collision.transform.position + new Vector3(0, labYOffset, 0);
                    
                    // 계산된 spawnPos 위치에 생성
                    GameObject builtLab = Instantiate(labBuildingPrefab, spawnPos, Quaternion.identity);
                    
                    // 생성된 건물을 해당 스팟의 자식으로 넣어 관리
                    builtLab.transform.SetParent(collision.transform);
                    
                    Debug.Log($"새로운 {subject} 연구소 점령! 3점 획득.");
                    StartCoroutine(FinishTurn(false)); 
                }
                else
                {
                    Debug.Log($"{subject} 연구소는 이미 점령된 상태입니다. 추가 점수 획득!");
                    StartCoroutine(FinishTurn(true)); 
                }
            }
        }
        // --- 2. 일반 바닥(Ground) 혹은 다른 곳에 닿았을 경우 (실패) ---
        else if (hitTag == "Ground")
        {
            isLanded = true;
            StopProjectile();

            // 일반 바닥일 때만 물리(Physics) 점수와 팝업 생성
            float distance = Vector2.Distance(startPosition, transform.position);

            // 팝업 생성
            if (distancePopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(0, 1.0f, 0);
                GameObject popup = Instantiate(distancePopupPrefab, spawnPos, Quaternion.identity);
                popup.GetComponent<DistancePopup>().Setup(distance);
            }

            int bonusResource = 1 + (int)(distance / 10f);
            ResourceManager.Instance.AddResource("Physics", bonusResource);
            Debug.Log($"일반 안착! 거리 보상 획득.");
            

            // 스팟이 아니므로 2초 뒤 카메라를 돌리고 포탄을 삭제합니다.
            StartCoroutine(FinishTurn(true));
        }
    }

    void StopProjectile()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
    }

    // 턴을 종료하고 카메라를 대포로 복구시키는 코루틴
    IEnumerator FinishTurn(bool shouldDestroy)
    {
        // 결과 확인을 위해 2초 대기
        yield return new WaitForSeconds(2.0f);

        // 1. 대포에 있는 ResetCamera 함수 호출
        // 씬 내의 ProjectileLauncher 오브젝트를 찾아 카메라 우선순위를 초기화합니다.
        ProjectileLauncher launcher = FindObjectOfType<ProjectileLauncher>();
        if (launcher != null)
        {
            launcher.ResetCamera();
        }

        // 2. 포탄 삭제 처리 (연구소가 아닐 때만 삭제)
        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }
}


