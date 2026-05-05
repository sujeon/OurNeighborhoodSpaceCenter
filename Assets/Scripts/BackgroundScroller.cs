using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // 배경이 흐르는 속도 (0.1 ~ 0.5 사이에서 조절해 보세요)
    public float scrollSpeed = 0.2f; 
    
    private MeshRenderer meshRenderer;
    private Transform camTransform;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        camTransform = Camera.main.transform; // 메인 카메라 참조
    }

    void Update()
    {
        // 1. 카메라의 현재 X 위치를 가져옵니다.
        float cameraX = camTransform.position.x;

        // 2. 카메라 위치에 따라 텍스처를 얼마나 밀어줄지 계산합니다.
        // 카메라가 이동하는 만큼 배경 텍스처를 반대 방향으로 밀어 무한한 느낌을 줍니다.
        float offset = cameraX * scrollSpeed;

        // 3. 머티리얼의 Main Texture Offset을 업데이트합니다.
        meshRenderer.material.mainTextureOffset = new Vector2(offset, 0);

        // 4. [중요] 쿼드 자체가 카메라를 따라오게 만듭니다. (화면 밖으로 나가지 않도록)
        // 텍스처는 안에서 돌고, 판(Quad) 자체는 카메라와 같이 이동합니다.
        Vector3 newPos = transform.position;
        newPos.x = cameraX;
        transform.position = newPos;
    }
}
