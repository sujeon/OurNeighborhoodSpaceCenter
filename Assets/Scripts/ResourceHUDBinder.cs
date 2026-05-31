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
            Bind();
    }

    public void Bind()
    {
        if (ResourceManager.Instance == null)
        {
            GameLogUI.Warning("ResourceManager.Instance가 없습니다. 자원 UI를 연결할 수 없습니다.");
            return;
        }

        ResourceManager.Instance.physicsText = physicsText;
        ResourceManager.Instance.chemistryText = chemistryText;
        ResourceManager.Instance.biologyText = biologyText;
        ResourceManager.Instance.earthText = earthText;

        if (updateImmediately)
            ResourceManager.Instance.UpdateUI();
    }
}
