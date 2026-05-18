using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class DistancePopup : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float destroyTime = 2.0f;

    private TextMeshPro textMesh;
    private float timer;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float distance)
    {
        textMesh.text = $"{distance:F1}m!";
        timer = destroyTime;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * (moveSpeed * Time.deltaTime), Space.World);
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
