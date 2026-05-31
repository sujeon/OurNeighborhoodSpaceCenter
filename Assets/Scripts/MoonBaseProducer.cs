using UnityEngine;
using System.Collections;

public class MoonBaseProducer : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Physics;
    public int productionAmount = 2;
    public float productionInterval = 5f;

    private WaitForSeconds productionWait;

    private void Start()
    {
        productionWait = new WaitForSeconds(productionInterval);
        StartCoroutine(ProduceRoutine());
    }

    private IEnumerator ProduceRoutine()
    {
        while (true)
        {
            yield return productionWait;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(resourceType, productionAmount);
            }
        }
    }
}
