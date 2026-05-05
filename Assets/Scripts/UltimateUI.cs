using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UltimateUI : MonoBehaviour
{
    [Header("Ultimate Slot")]
    [SerializeField] private Image ultimateBackground;
    [SerializeField] private Image ultimateChargeFill;
    [SerializeField] private TextMeshProUGUI ultimateKeyText;
    [SerializeField] private TextMeshProUGUI ultimateNameText;

    [Header("Knife Counter (when active)")]
    [SerializeField] private GameObject knifeCounterContainer;
    [SerializeField] private Image[] knifeIcons;

    [Header("Colors")]
    [SerializeField] private Color chargingColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color readyColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color knifeActiveColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color knifeUsedColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("References")]
    [SerializeField] private BladeStorm bladeStorm;

    // UI elements created at runtime
    private GameObject ultimateContainer;

    void Start()
    {
        // Find BladeStorm
        if (bladeStorm == null)
            bladeStorm = FindObjectOfType<BladeStorm>();

        CreateUI();

        if (bladeStorm != null)
        {
            bladeStorm.OnChargeChanged += UpdateChargeDisplay;
            bladeStorm.OnKnivesChanged += UpdateKnivesDisplay;
            bladeStorm.OnUltimateStateChanged += OnUltimateStateChanged;
        }

        // Initial state
        UpdateChargeDisplay(0, 6);
        if (knifeCounterContainer != null)
            knifeCounterContainer.SetActive(false);
    }

    void CreateUI()
    {
        // Create container
        ultimateContainer = new GameObject("UltimateContainer");
        ultimateContainer.transform.SetParent(transform);

        RectTransform containerRect = ultimateContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(120, 120);
        containerRect.sizeDelta = new Vector2(60, 80);
        containerRect.localScale = Vector3.one;

        // Ultimate background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(ultimateContainer.transform);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1);
        bgRect.anchorMax = new Vector2(0.5f, 1);
        bgRect.pivot = new Vector2(0.5f, 1);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(50, 50);
        bgRect.localScale = Vector3.one;

        ultimateBackground = bgObj.AddComponent<Image>();
        ultimateBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Charge fill
        GameObject fillObj = new GameObject("ChargeFill");
        fillObj.transform.SetParent(bgObj.transform);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        fillRect.localScale = Vector3.one;

        ultimateChargeFill = fillObj.AddComponent<Image>();
        ultimateChargeFill.color = chargingColor;
        ultimateChargeFill.type = Image.Type.Filled;
        ultimateChargeFill.fillMethod = Image.FillMethod.Vertical;
        ultimateChargeFill.fillOrigin = 0;
        ultimateChargeFill.fillAmount = 0;

        // Key text
        GameObject keyObj = new GameObject("KeyText");
        keyObj.transform.SetParent(bgObj.transform);

        RectTransform keyRect = keyObj.AddComponent<RectTransform>();
        keyRect.anchorMin = Vector2.zero;
        keyRect.anchorMax = Vector2.one;
        keyRect.offsetMin = Vector2.zero;
        keyRect.offsetMax = Vector2.zero;
        keyRect.localScale = Vector3.one;

        ultimateKeyText = keyObj.AddComponent<TextMeshProUGUI>();
        ultimateKeyText.text = "X";
        ultimateKeyText.fontSize = 24;
        ultimateKeyText.fontStyle = FontStyles.Bold;
        ultimateKeyText.alignment = TextAlignmentOptions.Center;
        ultimateKeyText.color = Color.white;

        // Name text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(ultimateContainer.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1);
        nameRect.anchorMax = new Vector2(0.5f, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -52);
        nameRect.sizeDelta = new Vector2(80, 15);
        nameRect.localScale = Vector3.one;

        ultimateNameText = nameObj.AddComponent<TextMeshProUGUI>();
        ultimateNameText.text = "BLADE STORM";
        ultimateNameText.fontSize = 7;
        ultimateNameText.alignment = TextAlignmentOptions.Center;
        ultimateNameText.color = Color.white;

        // Knife counter container
        knifeCounterContainer = new GameObject("KnifeCounter");
        knifeCounterContainer.transform.SetParent(ultimateContainer.transform);

        RectTransform knifeContainerRect = knifeCounterContainer.AddComponent<RectTransform>();
        knifeContainerRect.anchorMin = new Vector2(0.5f, 1);
        knifeContainerRect.anchorMax = new Vector2(0.5f, 1);
        knifeContainerRect.pivot = new Vector2(0.5f, 1);
        knifeContainerRect.anchoredPosition = new Vector2(0, -70);
        knifeContainerRect.sizeDelta = new Vector2(80, 15);
        knifeContainerRect.localScale = Vector3.one;

        HorizontalLayoutGroup knifeLayout = knifeCounterContainer.AddComponent<HorizontalLayoutGroup>();
        knifeLayout.spacing = 4;
        knifeLayout.childAlignment = TextAnchor.MiddleCenter;
        knifeLayout.childControlWidth = false;
        knifeLayout.childControlHeight = false;

        // Create knife icons
        knifeIcons = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject knifeIcon = new GameObject($"Knife_{i}");
            knifeIcon.transform.SetParent(knifeCounterContainer.transform);

            RectTransform knifeRect = knifeIcon.AddComponent<RectTransform>();
            knifeRect.sizeDelta = new Vector2(8, 20);
            knifeRect.localScale = Vector3.one;

            knifeIcons[i] = knifeIcon.AddComponent<Image>();
            knifeIcons[i].color = knifeActiveColor;
        }

        knifeCounterContainer.SetActive(false);
    }

    void UpdateChargeDisplay(int current, int max)
    {
        if (ultimateChargeFill == null) return;

        float percent = (float)current / max;
        ultimateChargeFill.fillAmount = percent;

        if (percent >= 1f)
        {
            ultimateChargeFill.color = readyColor;
            ultimateKeyText.color = readyColor;

            // Pulse effect when ready
            StartCoroutine(PulseEffect());
        }
        else
        {
            ultimateChargeFill.color = chargingColor;
            ultimateKeyText.color = Color.white;
        }
    }

    void UpdateKnivesDisplay(int current, int max)
    {
        if (knifeIcons == null) return;

        for (int i = 0; i < knifeIcons.Length; i++)
        {
            if (knifeIcons[i] != null)
            {
                knifeIcons[i].color = i < current ? knifeActiveColor : knifeUsedColor;
            }
        }
    }

    void OnUltimateStateChanged(bool active)
    {
        if (knifeCounterContainer != null)
            knifeCounterContainer.SetActive(active);

        if (ultimateChargeFill != null)
            ultimateChargeFill.color = active ? activeColor : chargingColor;

        if (ultimateKeyText != null)
            ultimateKeyText.color = active ? activeColor : Color.white;
    }

    IEnumerator PulseEffect()
    {
        while (bladeStorm != null && bladeStorm.IsUltimateReady() && !bladeStorm.IsUltimateActive())
        {
            // Pulse the border/background
            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;

            if (ultimateBackground != null)
            {
                Color c = Color.Lerp(new Color(0.1f, 0.1f, 0.1f, 0.8f), readyColor * 0.5f, pulse * 0.3f);
                ultimateBackground.color = c;
            }

            yield return null;
        }

        // Reset background
        if (ultimateBackground != null)
            ultimateBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    }

    void OnDestroy()
    {
        if (bladeStorm != null)
        {
            bladeStorm.OnChargeChanged -= UpdateChargeDisplay;
            bladeStorm.OnKnivesChanged -= UpdateKnivesDisplay;
            bladeStorm.OnUltimateStateChanged -= OnUltimateStateChanged;
        }
    }
}