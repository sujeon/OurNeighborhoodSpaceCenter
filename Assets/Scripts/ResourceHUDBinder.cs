using UnityEngine;
using TMPro;

public class ResourceHUDBinder : MonoBehaviour
{
    [Header("현재 씬의 자원 UI 텍스트")]
    [SerializeField] private TextMeshProUGUI physicsText;
    [SerializeField] private TextMeshProUGUI chemistryText;
    [SerializeField] private TextMeshProUGUI biologyText;
    [SerializeField] private TextMeshProUGUI earthText;

    [Header("자동 재연결 설정")]
    [SerializeField] private bool bindOnStart = true;
    [SerializeField] private bool updateImmediately = true;

    private void Start()
    {
        if (bindOnStart)
        {
            Bind();
        }
    }

    public void Bind()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("ResourceManager.Instance가 없습니다. 자원 UI를 연결할 수 없습니다.");
            return;
        }

        ResourceManager resourceManager = ResourceManager.Instance;

        resourceManager.physicsText = physicsText;
        resourceManager.chemistryText = chemistryText;
        resourceManager.biologyText = biologyText;
        resourceManager.earthText = earthText;

        if (updateImmediately)
        {
            resourceManager.UpdateUI();
        }

        Debug.Log("현재 씬의 자원 HUD가 ResourceManager에 연결되었습니다.");
    }
}
