using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode scoreboardKey = KeyCode.Tab;
    [SerializeField] private bool holdToShow = true;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    [SerializeField] private Color headerColor = new Color(0.15f, 0.15f, 0.2f, 1f);
    [SerializeField] private Color playerRowColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
    [SerializeField] private Color enemyRowColor = new Color(0.3f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color accentColor = new Color(0.3f, 0.7f, 1f);

    // Player stats
    private int playerKills = 0;
    private int playerDeaths = 0;
    private int playerAssists = 0;
    private int playerHeadshots = 0;
    private int playerScore = 0;

    // Accuracy stats
    private int totalShots = 0;
    private int shotsHit = 0;

    // Enemy stats
    private int totalEnemiesKilled = 0;
    private int totalEnemiesSpawned = 0;

    // UI Elements
    private GameObject scoreboardPanel;
    private TextMeshProUGUI killsText;
    private TextMeshProUGUI deathsText;
    private TextMeshProUGUI assistsText;
    private TextMeshProUGUI headshotsText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI kdRatioText;
    private TextMeshProUGUI enemiesKilledText;
    private TextMeshProUGUI hsPercentText;
    private TextMeshProUGUI accuracyText;

    private bool isShowing = false;

    public static Scoreboard Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        CreateScoreboardUI();

        // Subscribe to events
        EnemyAI.OnKill += OnKillEvent;
        PlayerHealth.OnPlayerDied += OnPlayerDeath;

        // Hide initially
        scoreboardPanel.SetActive(false);
    }

    void OnDestroy()
    {
        EnemyAI.OnKill -= OnKillEvent;
        PlayerHealth.OnPlayerDied -= OnPlayerDeath;
    }

    void CreateScoreboardUI()
    {
        // Find canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        // Main panel
        scoreboardPanel = new GameObject("ScoreboardPanel");
        scoreboardPanel.transform.SetParent(canvas.transform);

        RectTransform panelRect = scoreboardPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400);
        panelRect.localScale = Vector3.one;

        Image panelBg = scoreboardPanel.AddComponent<Image>();
        panelBg.color = backgroundColor;

        // Add vertical layout
        VerticalLayoutGroup layout = scoreboardPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Title
        CreateTitle();

        // Header row
        CreateHeader();

        // Player row
        CreatePlayerRow();

        // Divider
        CreateDivider();

        // Stats section
        CreateStatsSection();
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(scoreboardPanel.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(560, 50);
        titleRect.localScale = Vector3.one;

        Image titleBg = titleObj.AddComponent<Image>();
        titleBg.color = headerColor;

        // Title text
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleObj.transform);

        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.offsetMin = Vector2.zero;
        titleTextRect.offsetMax = Vector2.zero;
        titleTextRect.localScale = Vector3.one;

        TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SCOREBOARD";
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = accentColor;
    }

    void CreateHeader()
    {
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(scoreboardPanel.transform);

        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(560, 35);
        headerRect.localScale = Vector3.one;

        Image headerBg = headerObj.AddComponent<Image>();
        headerBg.color = headerColor;

        // Add horizontal layout
        HorizontalLayoutGroup headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset(10, 10, 5, 5);
        headerLayout.spacing = 5;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = false;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;

        // Header columns
        CreateHeaderColumn(headerObj.transform, "PLAYER", 150);
        CreateHeaderColumn(headerObj.transform, "K", 50);
        CreateHeaderColumn(headerObj.transform, "D", 50);
        CreateHeaderColumn(headerObj.transform, "A", 50);
        CreateHeaderColumn(headerObj.transform, "HS", 50);
        CreateHeaderColumn(headerObj.transform, "SCORE", 80);
    }

    void CreateHeaderColumn(Transform parent, string text, float width)
    {
        GameObject colObj = new GameObject(text);
        colObj.transform.SetParent(parent);

        RectTransform colRect = colObj.AddComponent<RectTransform>();
        colRect.sizeDelta = new Vector2(width, 30);
        colRect.localScale = Vector3.one;

        TextMeshProUGUI colText = colObj.AddComponent<TextMeshProUGUI>();
        colText.text = text;
        colText.fontSize = 14;
        colText.fontStyle = FontStyles.Bold;
        colText.alignment = TextAlignmentOptions.Center;
        colText.color = new Color(0.7f, 0.7f, 0.7f);
    }

    void CreatePlayerRow()
    {
        GameObject rowObj = new GameObject("PlayerRow");
        rowObj.transform.SetParent(scoreboardPanel.transform);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(560, 40);
        rowRect.localScale = Vector3.one;

        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = playerRowColor;

        // Add horizontal layout
        HorizontalLayoutGroup rowLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(10, 10, 5, 5);
        rowLayout.spacing = 5;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        // Player name
        GameObject nameObj = new GameObject("PlayerName");
        nameObj.transform.SetParent(rowObj.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(150, 30);
        nameRect.localScale = Vector3.one;

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "► YOU";
        nameText.fontSize = 16;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = accentColor;

        // Stats columns
        killsText = CreateStatColumn(rowObj.transform, "0", 50);
        deathsText = CreateStatColumn(rowObj.transform, "0", 50);
        assistsText = CreateStatColumn(rowObj.transform, "0", 50);
        headshotsText = CreateStatColumn(rowObj.transform, "0", 50);
        scoreText = CreateStatColumn(rowObj.transform, "0", 80);
    }

    TextMeshProUGUI CreateStatColumn(Transform parent, string text, float width)
    {
        GameObject colObj = new GameObject("StatColumn");
        colObj.transform.SetParent(parent);

        RectTransform colRect = colObj.AddComponent<RectTransform>();
        colRect.sizeDelta = new Vector2(width, 30);
        colRect.localScale = Vector3.one;

        TextMeshProUGUI colText = colObj.AddComponent<TextMeshProUGUI>();
        colText.text = text;
        colText.fontSize = 18;
        colText.fontStyle = FontStyles.Bold;
        colText.alignment = TextAlignmentOptions.Center;
        colText.color = textColor;

        return colText;
    }

    void CreateDivider()
    {
        GameObject dividerObj = new GameObject("Divider");
        dividerObj.transform.SetParent(scoreboardPanel.transform);

        RectTransform dividerRect = dividerObj.AddComponent<RectTransform>();
        dividerRect.sizeDelta = new Vector2(560, 2);
        dividerRect.localScale = Vector3.one;

        Image dividerImg = dividerObj.AddComponent<Image>();
        dividerImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    }

    void CreateStatsSection()
    {
        GameObject statsObj = new GameObject("StatsSection");
        statsObj.transform.SetParent(scoreboardPanel.transform);

        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.sizeDelta = new Vector2(560, 150);
        statsRect.localScale = Vector3.one;

        Image statsBg = statsObj.AddComponent<Image>();
        statsBg.color = new Color(0.12f, 0.12f, 0.17f, 0.9f);

        // Grid layout for stats
        GridLayoutGroup gridLayout = statsObj.AddComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(20, 20, 15, 15);
        gridLayout.cellSize = new Vector2(250, 35);
        gridLayout.spacing = new Vector2(20, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;

        // Create stat items
        kdRatioText = CreateStatItem(statsObj.transform, "K/D Ratio", "0.00");
        hsPercentText = CreateStatItem(statsObj.transform, "HS %", "0%");
        enemiesKilledText = CreateStatItem(statsObj.transform, "Total Kills", "0");
        accuracyText = CreateStatItem(statsObj.transform, "Accuracy", "0%");
    }

    TextMeshProUGUI CreateStatItem(Transform parent, string label, string value)
    {
        GameObject itemObj = new GameObject(label);
        itemObj.transform.SetParent(parent);
        itemObj.transform.localScale = Vector3.one;

        // Horizontal layout
        HorizontalLayoutGroup itemLayout = itemObj.AddComponent<HorizontalLayoutGroup>();
        itemLayout.spacing = 10;
        itemLayout.childAlignment = TextAnchor.MiddleLeft;
        itemLayout.childControlWidth = false;
        itemLayout.childControlHeight = true;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(itemObj.transform);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(120, 30);
        labelRect.localScale = Vector3.one;

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label + ":";
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = new Color(0.7f, 0.7f, 0.7f);

        // Value
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(itemObj.transform);

        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(100, 30);
        valueRect.localScale = Vector3.one;

        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = value;
        valueText.fontSize = 18;
        valueText.fontStyle = FontStyles.Bold;
        valueText.alignment = TextAlignmentOptions.Left;
        valueText.color = accentColor;

        return valueText;
    }

    void Update()
    {
        // Toggle scoreboard
        if (holdToShow)
        {
            // Hold to show
            if (Input.GetKeyDown(scoreboardKey))
            {
                ShowScoreboard();
            }
            else if (Input.GetKeyUp(scoreboardKey))
            {
                HideScoreboard();
            }
        }
        else
        {
            // Toggle on press
            if (Input.GetKeyDown(scoreboardKey))
            {
                if (isShowing)
                    HideScoreboard();
                else
                    ShowScoreboard();
            }
        }
    }

    void ShowScoreboard()
    {
        isShowing = true;
        scoreboardPanel.SetActive(true);
        UpdateUI();
    }

    void HideScoreboard()
    {
        isShowing = false;
        scoreboardPanel.SetActive(false);
    }

    void UpdateUI()
    {
        if (killsText != null) killsText.text = playerKills.ToString();
        if (deathsText != null) deathsText.text = playerDeaths.ToString();
        if (assistsText != null) assistsText.text = playerAssists.ToString();
        if (headshotsText != null) headshotsText.text = playerHeadshots.ToString();
        if (scoreText != null) scoreText.text = playerScore.ToString();

        // K/D Ratio
        if (kdRatioText != null)
        {
            float kd = playerDeaths > 0 ? (float)playerKills / playerDeaths : playerKills;
            kdRatioText.text = kd.ToString("F2");
        }

        // Headshot %
        if (hsPercentText != null)
        {
            float hsPercent = playerKills > 0 ? (float)playerHeadshots / playerKills * 100f : 0;
            hsPercentText.text = hsPercent.ToString("F0") + "%";
        }

        // Total kills
        if (enemiesKilledText != null)
        {
            enemiesKilledText.text = totalEnemiesKilled.ToString();
        }

        // Accuracy
        if (accuracyText != null)
        {
            float accuracy = totalShots > 0 ? (float)shotsHit / totalShots * 100f : 0;
            accuracyText.text = accuracy.ToString("F1") + "%";
        }
    }

    // Event handlers
    void OnKillEvent(string killer, string victim, bool headshot)
    {
        Debug.Log($"Scoreboard: Kill event - {killer} killed {victim}, headshot: {headshot}");

        if (killer == "Player")
        {
            playerKills++;
            totalEnemiesKilled++;
            playerScore += headshot ? 150 : 100;

            if (headshot)
            {
                playerHeadshots++;
                Debug.Log($"Scoreboard: Headshot counted! Total: {playerHeadshots}");
            }

            if (isShowing)
                UpdateUI();
        }
    }

    void OnPlayerDeath()
    {
        playerDeaths++;
        Debug.Log($"Scoreboard: Player death counted! Total: {playerDeaths}");

        if (isShowing)
            UpdateUI();
    }

    // Public methods
    public void AddKill(bool headshot = false)
    {
        playerKills++;
        totalEnemiesKilled++;
        playerScore += headshot ? 150 : 100;

        if (headshot)
            playerHeadshots++;
    }

    public void AddDeath()
    {
        playerDeaths++;
    }

    public void AddAssist()
    {
        playerAssists++;
        playerScore += 25;
    }

    public void AddShot()
    {
        totalShots++;
    }

    public void AddHit()
    {
        shotsHit++;
    }

    public void ResetStats()
    {
        playerKills = 0;
        playerDeaths = 0;
        playerAssists = 0;
        playerHeadshots = 0;
        playerScore = 0;
        totalEnemiesKilled = 0;
        totalShots = 0;
        shotsHit = 0;
    }

    // Getters
    public int GetKills() => playerKills;
    public int GetDeaths() => playerDeaths;
    public int GetScore() => playerScore;
    public float GetAccuracy() => totalShots > 0 ? (float)shotsHit / totalShots * 100f : 0;
}