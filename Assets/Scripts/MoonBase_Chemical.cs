using UnityEngine;
using System.Collections;
public class MoonBase_Chemical : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Chemistry;
    public int productionAmount = 2;
    public float productionInterval = 5f;

    private void Start()
    {
        StartCoroutine(ProduceRoutine());
    }

    private IEnumerator ProduceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(productionInterval);

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(resourceType, productionAmount);
            }
        }
    }
}
