using UnityEngine;
using System.Collections;

public class LabGenerator : MonoBehaviour
{
    public string resourceType; 
    public float productionInterval = 5f; 
    private bool isActivated = false; // 현재 연구소가 가동 중인지 체크하는 변수

    // 연구소 가동 시도 (성공하면 true, 이미 가동 중이면 false 반환)
    public bool ActivateLab() 
    {
        if (isActivated) 
        {
            Debug.Log($"{resourceType} 연구소는 이미 가동 중입니다!");
            return false; // 이미 가동 중이므로 실패 반환
        }
        
        isActivated = true;
        StartCoroutine(ProduceRoutine());
        Debug.Log($"{resourceType} 연구소 가동 시작!");
        return true; // 새로 가동했으므로 성공 반환
    }

    IEnumerator ProduceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(productionInterval);
            
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(resourceType, 1);
            }
        }
    }
}