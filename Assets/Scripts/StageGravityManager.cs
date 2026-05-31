using UnityEngine;

public class StageGravityManager : MonoBehaviour
{
    [SerializeField] private Vector2 gravity = new Vector2(0f, -9.81f);
    [SerializeField] private bool logGravityOnStart = false;

    private void Start()
    {
        Physics2D.gravity = gravity;

        if (logGravityOnStart)
            GameLogUI.Log($"스테이지 중력 적용: {gravity.y:F2}");
    }
}
