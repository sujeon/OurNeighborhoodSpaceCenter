using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLogUI : MonoBehaviour
{
    public static GameLogUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Transform logContent;
    [SerializeField] private TextMeshProUGUI logTextPrefab;

    [Header("표시 설정")]
    [SerializeField] private int maxLogCount = 5;
    [SerializeField] private float visibleTime = 3.0f;
    [SerializeField] private float fadeTime = 0.5f;

    [Header("Debug.Log 자동 표시")]
    [SerializeField] private bool showUnityDebugLog = false;

    private readonly Queue<GameObject> logObjects = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (showUnityDebugLog)
            Application.logMessageReceived += HandleUnityLog;
    }

    private void OnDisable()
    {
        if (showUnityDebugLog)
            Application.logMessageReceived -= HandleUnityLog;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void Log(string message, bool alsoDebugLog = true)
    {
        if (alsoDebugLog)
            Debug.Log(message);

        if (Instance != null)
            Instance.Show(message);
    }

    public static void Warning(string message, bool alsoDebugLog = true)
    {
        if (alsoDebugLog)
            Debug.LogWarning(message);

        if (Instance != null)
            Instance.Show($"경고: {message}");
    }

    private void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Warning)
            Show($"경고: {condition}");
        else if (type == LogType.Error || type == LogType.Exception)
            Show($"에러: {condition}");
        else
            Show(condition);
    }

    public void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (logContent == null || logTextPrefab == null)
            return;

        TextMeshProUGUI textObj = Instantiate(logTextPrefab, logContent);
        textObj.text = message;
        textObj.gameObject.SetActive(true);

        CanvasGroup canvasGroup = textObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = textObj.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        logObjects.Enqueue(textObj.gameObject);

        while (logObjects.Count > maxLogCount)
        {
            GameObject oldLog = logObjects.Dequeue();
            if (oldLog != null)
                Destroy(oldLog);
        }

        StartCoroutine(FadeAndDestroy(textObj.gameObject, canvasGroup));
    }

    private IEnumerator FadeAndDestroy(GameObject logObject, CanvasGroup canvasGroup)
    {
        yield return new WaitForSecondsRealtime(visibleTime);

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            yield return null;
        }

        RemoveFromQueue(logObject);

        if (logObject != null)
            Destroy(logObject);
    }

    private void RemoveFromQueue(GameObject target)
    {
        if (target == null || logObjects.Count == 0)
            return;

        Queue<GameObject> newQueue = new Queue<GameObject>();

        while (logObjects.Count > 0)
        {
            GameObject item = logObjects.Dequeue();
            if (item != target)
                newQueue.Enqueue(item);
        }

        while (newQueue.Count > 0)
            logObjects.Enqueue(newQueue.Dequeue());
    }
}
