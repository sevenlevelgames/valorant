using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinIndicatorUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeSpeed = 2f;

    // UI Elements
    private GameObject indicatorPanel;
    private TextMeshProUGUI skinNameText;
    private Image[] colorPreviews;

    private float hideTimer;
    private CanvasGroup canvasGroup;
    private WeaponManager weaponManager;

    public static SkinIndicatorUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
        CreateUI();

        // Start hidden
        if (canvasGroup != null)
            canvasGroup.alpha = 0;
    }

    void CreateUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        // Main panel
        indicatorPanel = new GameObject("SkinIndicator");
        indicatorPanel.transform.SetParent(canvas.transform);

        RectTransform panelRect = indicatorPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.85f);
        panelRect.anchorMax = new Vector2(0.5f, 0.85f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(300, 60);
        panelRect.localScale = Vector3.one;

        canvasGroup = indicatorPanel.AddComponent<CanvasGroup>();

        Image panelBg = indicatorPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // Horizontal layout
        HorizontalLayoutGroup layout = indicatorPanel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        // Skin name
        GameObject textObj = new GameObject("SkinName");
        textObj.transform.SetParent(indicatorPanel.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150, 40);
        textRect.localScale = Vector3.one;

        skinNameText = textObj.AddComponent<TextMeshProUGUI>();
        skinNameText.text = "Default";
        skinNameText.fontSize = 22;
        skinNameText.fontStyle = FontStyles.Bold;
        skinNameText.alignment = TextAlignmentOptions.Center;
        skinNameText.color = Color.white;

        // Color previews
        colorPreviews = new Image[3];
        string[] colorNames = { "Primary", "Secondary", "Accent" };

        for (int i = 0; i < 3; i++)
        {
            GameObject colorObj = new GameObject(colorNames[i]);
            colorObj.transform.SetParent(indicatorPanel.transform);

            RectTransform colorRect = colorObj.AddComponent<RectTransform>();
            colorRect.sizeDelta = new Vector2(30, 30);
            colorRect.localScale = Vector3.one;

            colorPreviews[i] = colorObj.AddComponent<Image>();
            colorPreviews[i].color = Color.gray;

            // Add outline
            Outline outline = colorObj.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(1, 1);
        }

        // Controls hint
        GameObject hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(canvas.transform);

        RectTransform hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0.8f);
        hintRect.anchorMax = new Vector2(0.5f, 0.8f);
        hintRect.pivot = new Vector2(0.5f, 0.5f);
        hintRect.anchoredPosition = Vector2.zero;
        hintRect.sizeDelta = new Vector2(200, 25);
        hintRect.localScale = Vector3.one;

        TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "[O] ← Skin → [P]";
        hintText.fontSize = 14;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);

        hintObj.transform.SetParent(indicatorPanel.transform);
    }

    void Update()
    {
        // Check for skin change
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.O))
        {
            ShowIndicator();
        }

        // Fade out
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
        }
        else if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
        }
    }

    public void ShowIndicator()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();

        if (weaponManager == null) return;

        // Get current skin
        var skinField = typeof(WeaponManager).GetField("currentSkin",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (skinField != null)
        {
            var skin = skinField.GetValue(weaponManager);
            skinNameText.text = skin.ToString().ToUpper();

            // Update color previews
            Color primary, secondary, accent;
            GetSkinColors(skin.ToString(), out primary, out secondary, out accent);

            colorPreviews[0].color = primary;
            colorPreviews[1].color = secondary;
            colorPreviews[2].color = accent;
        }

        // Show and reset timer
        if (canvasGroup != null)
            canvasGroup.alpha = 1;
        hideTimer = displayDuration;
    }

    void GetSkinColors(string skinName, out Color primary, out Color secondary, out Color accent)
    {
        switch (skinName)
        {
            case "Gold":
                primary = new Color(0.7f, 0.55f, 0.1f);
                secondary = new Color(0.85f, 0.7f, 0.2f);
                accent = new Color(1f, 0.85f, 0.3f);
                break;

            case "RedTiger":
                primary = new Color(0.15f, 0.1f, 0.1f);
                secondary = new Color(0.6f, 0.1f, 0.1f);
                accent = new Color(0.9f, 0.2f, 0.1f);
                break;

            case "Arctic":
                primary = new Color(0.85f, 0.9f, 0.95f);
                secondary = new Color(0.7f, 0.8f, 0.85f);
                accent = new Color(0.4f, 0.7f, 0.9f);
                break;

            case "Carbon":
                primary = new Color(0.1f, 0.1f, 0.1f);
                secondary = new Color(0.15f, 0.15f, 0.15f);
                accent = new Color(0.3f, 0.3f, 0.3f);
                break;

            case "Neon":
                primary = new Color(0.1f, 0.1f, 0.15f);
                secondary = new Color(0.15f, 0.15f, 0.2f);
                accent = new Color(0f, 1f, 0.8f);
                break;

            case "Dragon":
                primary = new Color(0.1f, 0.05f, 0.05f);
                secondary = new Color(0.5f, 0.1f, 0.05f);
                accent = new Color(1f, 0.4f, 0f);
                break;

            case "Galaxy":
                primary = new Color(0.1f, 0.05f, 0.2f);
                secondary = new Color(0.2f, 0.1f, 0.4f);
                accent = new Color(0.8f, 0.3f, 1f);
                break;

            default:
                primary = new Color(0.15f, 0.15f, 0.15f);
                secondary = new Color(0.25f, 0.25f, 0.25f);
                accent = new Color(0.8f, 0.4f, 0.1f);
                break;
        }
    }
}