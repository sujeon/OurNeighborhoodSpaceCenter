using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    public bool followCameraX = true;

    private MeshRenderer meshRenderer;
    private Transform camTransform;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            camTransform = mainCamera.transform;
        }
    }

    private void LateUpdate()
    {
        if (camTransform == null) return;

        float cameraX = camTransform.position.x;
        Vector2 offset = new Vector2(cameraX * scrollSpeed, 0f);

        // meshRenderer.material을 매 프레임 호출하면 머티리얼 인스턴스가 계속 생길 수 있어서 PropertyBlock으로 변경했습니다.
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(MainTexId, new Vector4(1f, 1f, offset.x, offset.y));
        meshRenderer.SetPropertyBlock(propertyBlock);

        if (followCameraX)
        {
            Vector3 newPos = transform.position;
            newPos.x = cameraX;
            transform.position = newPos;
        }
    }
}
