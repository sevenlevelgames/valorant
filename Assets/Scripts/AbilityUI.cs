using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float slotSize = 50f;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private float dotSize = 8f;

    [Header("Colors")]
    [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color unavailableColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    [Header("References")]
    [SerializeField] private AbilitySystem abilitySystem;

    // UI Elements
    private Image[] slotBackgrounds = new Image[3];
    private TextMeshProUGUI[] keyTexts = new TextMeshProUGUI[3];
    private TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[3];
    private Image[][] chargeDots = new Image[3][];

    // Ability names
    private string[] abilityNames = { "CLOUDBURST", "UPDRAFT", "TAILWIND" };
    private string[] keyNames = { "C", "Q", "E" };
    private int[] maxCharges = { 2, 2, 1 };

    void Start()
    {
        // Find ability system
        if (abilitySystem == null)
            abilitySystem = FindObjectOfType<AbilitySystem>();

        CreateUI();

        if (abilitySystem != null)
        {
            abilitySystem.OnAbilityChargesChanged += UpdateAbilityDisplay;
        }

        // Initial update
        UpdateAbilityDisplay(2, 2, 1);
    }

    void CreateUI()
    {
        // Main container setup
        RectTransform container = GetComponent<RectTransform>();

        // Add horizontal layout
        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Create 3 ability slots (C, Q, E)
        for (int i = 0; i < 3; i++)
        {
            CreateAbilitySlot(i);
        }
    }

    void CreateAbilitySlot(int index)
    {
        // Slot container
        GameObject slot = new GameObject($"AbilitySlot_{keyNames[index]}");
        slot.transform.SetParent(transform);

        RectTransform slotRect = slot.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(slotSize, slotSize + 25f);
        slotRect.localScale = Vector3.one;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(slot.transform);

        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1f);
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 1f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(slotSize, slotSize);
        bgRect.localScale = Vector3.one;

        slotBackgrounds[index] = bg.AddComponent<Image>();
        slotBackgrounds[index].color = backgroundColor;

        // Key text (C, Q, E)
        GameObject keyObj = new GameObject("KeyText");
        keyObj.transform.SetParent(bg.transform);

        RectTransform keyRect = keyObj.AddComponent<RectTransform>();
        keyRect.anchorMin = Vector2.zero;
        keyRect.anchorMax = Vector2.one;
        keyRect.offsetMin = Vector2.zero;
        keyRect.offsetMax = Vector2.zero;
        keyRect.localScale = Vector3.one;

        keyTexts[index] = keyObj.AddComponent<TextMeshProUGUI>();
        keyTexts[index].text = keyNames[index];
        keyTexts[index].fontSize = 24;
        keyTexts[index].fontStyle = FontStyles.Bold;
        keyTexts[index].alignment = TextAlignmentOptions.Center;
        keyTexts[index].color = availableColor;

        // Ability name text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(slot.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0f);
        nameRect.anchorMax = new Vector2(0.5f, 0f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, -slotSize - 2f);
        nameRect.sizeDelta = new Vector2(slotSize + 20f, 15f);
        nameRect.localScale = Vector3.one;

        nameTexts[index] = nameObj.AddComponent<TextMeshProUGUI>();
        nameTexts[index].text = abilityNames[index];
        nameTexts[index].fontSize = 8;
        nameTexts[index].alignment = TextAlignmentOptions.Center;
        nameTexts[index].color = Color.white;

        // Charge dots container
        GameObject dotsContainer = new GameObject("ChargeDots");
        dotsContainer.transform.SetParent(slot.transform);

        RectTransform dotsRect = dotsContainer.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 1f);
        dotsRect.anchorMax = new Vector2(0.5f, 1f);
        dotsRect.pivot = new Vector2(0.5f, 0f);
        dotsRect.anchoredPosition = new Vector2(0, -slotSize - 18f);
        dotsRect.sizeDelta = new Vector2(slotSize, dotSize);
        dotsRect.localScale = Vector3.one;

        HorizontalLayoutGroup dotsLayout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
        dotsLayout.spacing = 4f;
        dotsLayout.childAlignment = TextAnchor.MiddleCenter;
        dotsLayout.childControlWidth = false;
        dotsLayout.childControlHeight = false;

        // Create charge dots
        chargeDots[index] = new Image[maxCharges[index]];
        for (int d = 0; d < maxCharges[index]; d++)
        {
            GameObject dot = new GameObject($"Dot_{d}");
            dot.transform.SetParent(dotsContainer.transform);

            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(dotSize, dotSize);
            dotRect.localScale = Vector3.one;

            chargeDots[index][d] = dot.AddComponent<Image>();
            chargeDots[index][d].color = availableColor;

            // Make it round (use default UI sprite or leave as square)
        }
    }

    void UpdateAbilityDisplay(int chargesC, int chargesQ, int chargesE)
    {
        int[] charges = { chargesC, chargesQ, chargesE };

        for (int i = 0; i < 3; i++)
        {
            bool hasCharges = charges[i] > 0;

            // Update key text color
            if (keyTexts[i] != null)
            {
                keyTexts[i].color = hasCharges ? availableColor : unavailableColor;
            }

            // Update charge dots
            if (chargeDots[i] != null)
            {
                for (int d = 0; d < chargeDots[i].Length; d++)
                {
                    if (chargeDots[i][d] != null)
                    {
                        chargeDots[i][d].color = d < charges[i] ? availableColor : unavailableColor;
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (abilitySystem != null)
        {
            abilitySystem.OnAbilityChargesChanged -= UpdateAbilityDisplay;
        }
    }
}