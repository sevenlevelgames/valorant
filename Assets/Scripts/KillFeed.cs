using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class KillFeed : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private int maxEntries = 5;

    [Header("Prefab")]
    [SerializeField] private GameObject killEntryPrefab;

    [Header("Container")]
    [SerializeField] private Transform killFeedContainer;

    [Header("Colors")]
    [SerializeField] private Color playerKillColor = new Color(1f, 0.8f, 0.2f); // Gold
    [SerializeField] private Color enemyKillColor = new Color(0.8f, 0.2f, 0.2f); // Red
    [SerializeField] private Color headshotColor = new Color(1f, 0.4f, 0.4f);    // Bright red

    // Active entries
    private List<KillFeedEntry> activeEntries = new List<KillFeedEntry>();

    public static KillFeed Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        EnemyAI.OnKill += AddKillEntry;
    }

    void OnDisable()
    {
        EnemyAI.OnKill -= AddKillEntry;
    }

    void Start()
    {
        // Create container if not assigned
        if (killFeedContainer == null)
        {
            killFeedContainer = transform;
        }

        // Create prefab if not assigned
        if (killEntryPrefab == null)
        {
            CreateDefaultPrefab();
        }
    }

    void CreateDefaultPrefab()
    {
        // Create a default kill entry prefab
        killEntryPrefab = new GameObject("KillEntryPrefab");
        killEntryPrefab.SetActive(false);

        RectTransform rect = killEntryPrefab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 30);

        HorizontalLayoutGroup layout = killEntryPrefab.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        CanvasGroup canvasGroup = killEntryPrefab.AddComponent<CanvasGroup>();

        // Killer name
        GameObject killerObj = new GameObject("KillerName");
        killerObj.transform.SetParent(killEntryPrefab.transform);
        TextMeshProUGUI killerText = killerObj.AddComponent<TextMeshProUGUI>();
        killerText.fontSize = 16;
        killerText.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform killerRect = killerObj.GetComponent<RectTransform>();
        killerRect.sizeDelta = new Vector2(100, 30);

        // Kill icon (text for now)
        GameObject iconObj = new GameObject("KillIcon");
        iconObj.transform.SetParent(killEntryPrefab.transform);
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "▶";
        iconText.fontSize = 14;
        iconText.alignment = TextAlignmentOptions.Midline;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(30, 30);

        // Victim name
        GameObject victimObj = new GameObject("VictimName");
        victimObj.transform.SetParent(killEntryPrefab.transform);
        TextMeshProUGUI victimText = victimObj.AddComponent<TextMeshProUGUI>();
        victimText.fontSize = 16;
        victimText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform victimRect = victimObj.GetComponent<RectTransform>();
        victimRect.sizeDelta = new Vector2(100, 30);

        killEntryPrefab.transform.SetParent(transform);
    }

    public void AddKillEntry(string killer, string victim, bool headshot)
    {
        // Remove oldest entry if at max
        if (activeEntries.Count >= maxEntries)
        {
            RemoveEntry(activeEntries[0]);
        }

        // Create new entry
        GameObject entryObj = Instantiate(killEntryPrefab, killFeedContainer);
        entryObj.SetActive(true);

        KillFeedEntry entry = entryObj.AddComponent<KillFeedEntry>();
        entry.Initialize(killer, victim, headshot,
            killer == "Player" ? playerKillColor : enemyKillColor,
            headshotColor);

        activeEntries.Add(entry);

        // Start fade out timer
        StartCoroutine(RemoveEntryAfterDelay(entry));
    }

    System.Collections.IEnumerator RemoveEntryAfterDelay(KillFeedEntry entry)
    {
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        CanvasGroup canvasGroup = entry.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = entry.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }

        RemoveEntry(entry);
    }

    void RemoveEntry(KillFeedEntry entry)
    {
        if (entry == null) return;

        activeEntries.Remove(entry);
        Destroy(entry.gameObject);
    }
}

// Helper component for kill feed entries
public class KillFeedEntry : MonoBehaviour
{
    private TextMeshProUGUI killerText;
    private TextMeshProUGUI victimText;
    private TextMeshProUGUI iconText;

    public void Initialize(string killer, string victim, bool headshot, Color killColor, Color headshotColor)
    {
        // Find text components
        Transform killerTransform = transform.Find("KillerName");
        Transform victimTransform = transform.Find("VictimName");
        Transform iconTransform = transform.Find("KillIcon");

        if (killerTransform != null)
        {
            killerText = killerTransform.GetComponent<TextMeshProUGUI>();
            if (killerText != null)
            {
                killerText.text = killer;
                killerText.color = killColor;
            }
        }

        if (victimTransform != null)
        {
            victimText = victimTransform.GetComponent<TextMeshProUGUI>();
            if (victimText != null)
            {
                victimText.text = victim;
                victimText.color = Color.white;
            }
        }

        if (iconTransform != null)
        {
            iconText = iconTransform.GetComponent<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = headshot ? "☠" : "▶";
                iconText.color = headshot ? headshotColor : Color.white;
            }
        }
    }
}