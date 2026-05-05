using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float barWidth = 100f;
    [SerializeField] private float barHeight = 10f;
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool faceCamera = true;

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    // References
    private EnemyAI enemy;
    private Camera mainCamera;
    private GameObject healthBarObject;
    private Image backgroundImage;
    private Image fillImage;

    void Start()
    {
        mainCamera = Camera.main;
        enemy = GetComponent<EnemyAI>();

        if (enemy == null)
            enemy = GetComponentInParent<EnemyAI>();

        CreateHealthBar();

        // Initial hide
        if (hideWhenFull && healthBarObject != null)
            healthBarObject.SetActive(false);
    }

    void CreateHealthBar()
    {
        // Create main health bar object
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(transform);
        healthBarObject.transform.localPosition = offset;

        // Create canvas
        Canvas canvas = healthBarObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        RectTransform canvasRect = healthBarObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = healthBarObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObject.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localRotation = Quaternion.identity;
        bgObj.transform.localScale = Vector3.one;

        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(barWidth, barHeight);
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localRotation = Quaternion.identity;
        fillObj.transform.localScale = Vector3.one;

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullHealthColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = new Vector2(barWidth - 4, barHeight - 4);
        fillRect.anchoredPosition = new Vector2(2, 0);
    }

    void Update()
    {
        if (enemy == null) return;

        // Hide when dead
        if (enemy.IsDead())
        {
            if (healthBarObject != null)
                healthBarObject.SetActive(false);
            return;
        }

        float healthPercent = enemy.GetHealthPercent();

        // Show/hide based on health
        if (hideWhenFull)
        {
            bool shouldShow = healthPercent < 0.99f;
            if (healthBarObject != null && healthBarObject.activeSelf != shouldShow)
                healthBarObject.SetActive(shouldShow);
        }

        // Update fill width
        if (fillImage != null)
        {
            RectTransform fillRect = fillImage.GetComponent<RectTransform>();
            float fillWidth = (barWidth - 4) * healthPercent;
            fillRect.sizeDelta = new Vector2(fillWidth, barHeight - 4);

            // Update color based on health
            if (healthPercent > 0.6f)
                fillImage.color = fullHealthColor;
            else if (healthPercent > 0.3f)
                fillImage.color = midHealthColor;
            else
                fillImage.color = lowHealthColor;
        }

        // Face camera
        if (faceCamera && mainCamera != null && healthBarObject != null && healthBarObject.activeSelf)
        {
            healthBarObject.transform.rotation = mainCamera.transform.rotation;
        }
    }

    void OnDestroy()
    {
        if (healthBarObject != null)
            Destroy(healthBarObject);
    }
}