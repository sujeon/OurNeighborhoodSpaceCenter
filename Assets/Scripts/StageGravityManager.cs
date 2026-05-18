using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬마다 중력값을 다르게 적용하는 매니저입니다.
/// Stage1: 지구 중력, MoonStage: 달 중력처럼 설정해서 포물선 감각을 바꿀 수 있습니다.
/// </summary>
public class StageGravityManager : MonoBehaviour
{
    public static StageGravityManager Instance { get; private set; }
    public static float CurrentGravityY { get; private set; } = -9.8f;

    public enum GravityPreset
    {
        Earth,
        Moon,
        Mars,
        LowGravity,
        Space,
        Custom
    }

    [Header("현재 스테이지 중력 프리셋")]
    public GravityPreset preset = GravityPreset.Earth;

    [Header("Custom 선택 시 사용")]
    public float customGravityY = -9.8f;

    [Header("재미용 보정값")]
    [Range(0.1f, 2f)] public float gravityMultiplier = 1f;

    [Header("씬 시작 시 자동 적용")]
    public bool applyOnStart = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyGravity();
        }
    }

    public void ApplyGravity()
    {
        float baseGravity = GetPresetGravity(preset);
        SetGravity(baseGravity * gravityMultiplier);
    }

    public void SetGravity(float gravityY)
    {
        CurrentGravityY = gravityY;
        Physics2D.gravity = new Vector2(0f, gravityY);
        Debug.Log($"[{SceneManager.GetActiveScene().name}] Gravity set to {gravityY}");
    }

    private float GetPresetGravity(GravityPreset selectedPreset)
    {
        switch (selectedPreset)
        {
            case GravityPreset.Earth: return -9.8f;
            case GravityPreset.Moon: return -1.62f;
            case GravityPreset.Mars: return -3.71f;
            case GravityPreset.LowGravity: return -4.9f;
            case GravityPreset.Space: return -0.7f;
            case GravityPreset.Custom: return customGravityY;
            default: return -9.8f;
        }
    }
}
