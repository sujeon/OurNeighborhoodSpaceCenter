using UnityEngine;
using System.Collections;

public class MoonBaseProducer : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Physics;
    public int productionAmount = 2;
    public float productionInterval = 5f;

    [Header("로그 설정")]
    [SerializeField] private bool logProduction = false;

    private Coroutine produceRoutine;
    private WaitForSeconds productionWait;

    private void OnEnable()
    {
        productionWait = new WaitForSeconds(Mathf.Max(0.25f, productionInterval));
        produceRoutine = StartCoroutine(ProduceRoutine());
        GameLogUI.Log($"{ResourceManager.GetResourceName(resourceType)} 생산 기지 가동 시작");
    }

    private void OnDisable()
    {
        if (produceRoutine != null)
        {
            StopCoroutine(produceRoutine);
            produceRoutine = null;
        }
    }

    private IEnumerator ProduceRoutine()
    {
        while (true)
        {
            yield return productionWait;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(resourceType, productionAmount);

                if (logProduction)
                    GameLogUI.Log($"{ResourceManager.GetResourceName(resourceType)} 자동 생산 +{productionAmount}");
            }
        }
    }
}
