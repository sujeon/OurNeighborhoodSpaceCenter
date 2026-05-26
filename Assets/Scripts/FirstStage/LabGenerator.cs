using UnityEngine;
using System.Collections;

public class LabGenerator : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Physics;
    public float baseProductionInterval = 5f;
    public int productionAmount = 1;

    private bool isActivated;
    private Coroutine produceCoroutine;

    public bool ActivateLab()
    {
        if (isActivated)
        {
            Debug.Log($"{resourceType} 연구소는 이미 가동 중입니다!");
            return false;
        }

        isActivated = true;
        produceCoroutine = StartCoroutine(ProduceRoutine());
        Debug.Log($"{resourceType} 연구소 가동 시작!");
        return true;
    }

    private IEnumerator ProduceRoutine()
    {
        while (isActivated)
        {
            float multiplier = UpgradeManager.Instance != null 
                ? UpgradeManager.Instance.BioSpeedMultiplier 
                : 1f;

            float interval = Mathf.Max(0.25f, baseProductionInterval * multiplier);

            yield return new WaitForSeconds(interval);

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(resourceType, productionAmount);
            }
        }
    }

    private void OnDisable()
    {
        if (produceCoroutine != null)
        {
            StopCoroutine(produceCoroutine);
            produceCoroutine = null;
        }
    }
}
