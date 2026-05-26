using UnityEngine;
using System.Collections;
public class MoonBase_Physics : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Physics;
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
