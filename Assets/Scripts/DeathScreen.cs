using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    [Header("Death Screen Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private bool autoRespawn = true;

    [Header("UI Elements")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Image vignetteOverlay;
    [SerializeField] private TextMeshProUGUI eliminatedText;
    [SerializeField] private TextMeshProUGUI killerText;
    [SerializeField] private TextMeshProUGUI respawnTimerText;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Image respawnButtonFill;

    [Header("Colors")]
    [SerializeField] private Color overlayColor = new Color(0.1f, 0f, 0f, 0.8f);
    [SerializeField] private Color vignetteColor = new Color(0.5f, 0f, 0f, 0.6f);
    [SerializeField] private Color textColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public Transform respawnPoint;

    // State
    private bool isDead = false;
    private string lastKiller = "Enemy";
    private Canvas canvas;

    public static DeathScreen Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Find player health
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        // Subscribe to death event
        if (playerHealth != null)
            playerHealth.OnDeath += OnPlayerDeath;

        // Find canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        // Create UI
        CreateDeathScreenUI();

        // Hide initially
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);

        // Create default respawn point
        if (respawnPoint == null)
        {
            GameObject rp = new GameObject("RespawnPoint");
            rp.transform.position = new Vector3(0, 2, 0);
            respawnPoint = rp.transform;
        }
    }

    void CreateDeathScreenUI()
    {
        // Main panel
        deathScreenPanel = new GameObject("DeathScreenPanel");
        deathScreenPanel.transform.SetParent(canvas.transform);

        RectTransform panelRect = deathScreenPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.localScale = Vector3.one;

        // Background overlay
        GameObject bgObj = new GameObject("BackgroundOverlay");
        bgObj.transform.SetParent(deathScreenPanel.transform);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localScale = Vector3.one;

        backgroundOverlay = bgObj.AddComponent<Image>();
        backgroundOverlay.color = overlayColor;
        backgroundOverlay.raycastTarget = true;

        // Vignette overlay
        GameObject vignetteObj = new GameObject("VignetteOverlay");
        vignetteObj.transform.SetParent(deathScreenPanel.transform);

        RectTransform vignetteRect = vignetteObj.AddComponent<RectTransform>();
        vignetteRect.anchorMin = Vector2.zero;
        vignetteRect.anchorMax = Vector2.one;
        vignetteRect.offsetMin = Vector2.zero;
        vignetteRect.offsetMax = Vector2.zero;
        vignetteRect.localScale = Vector3.one;

        vignetteOverlay = vignetteObj.AddComponent<Image>();
        vignetteOverlay.color = vignetteColor;
        vignetteOverlay.raycastTarget = false;

        // Content container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(deathScreenPanel.transform);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600, 400);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.localScale = Vector3.one;

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 20;
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.childControlWidth = false;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = false;

        // Skull icon (using text)
        GameObject skullObj = new GameObject("SkullIcon");
        skullObj.transform.SetParent(contentObj.transform);

        RectTransform skullRect = skullObj.AddComponent<RectTransform>();
        skullRect.sizeDelta = new Vector2(100, 100);
        skullRect.localScale = Vector3.one;

        TextMeshProUGUI skullText = skullObj.AddComponent<TextMeshProUGUI>();
        skullText.text = "☠";
        skullText.fontSize = 80;
        skullText.alignment = TextAlignmentOptions.Center;
        skullText.color = textColor;

        // "ELIMINATED" text
        GameObject elimObj = new GameObject("EliminatedText");
        elimObj.transform.SetParent(contentObj.transform);

        RectTransform elimRect = elimObj.AddComponent<RectTransform>();
        elimRect.sizeDelta = new Vector2(500, 80);
        elimRect.localScale = Vector3.one;

        eliminatedText = elimObj.AddComponent<TextMeshProUGUI>();
        eliminatedText.text = "ELIMINATED";
        eliminatedText.fontSize = 64;
        eliminatedText.fontStyle = FontStyles.Bold;
        eliminatedText.alignment = TextAlignmentOptions.Center;
        eliminatedText.color = textColor;

        // Killer text
        GameObject killerObj = new GameObject("KillerText");
        killerObj.transform.SetParent(contentObj.transform);

        RectTransform killerRect = killerObj.AddComponent<RectTransform>();
        killerRect.sizeDelta = new Vector2(400, 40);
        killerRect.localScale = Vector3.one;

        killerText = killerObj.AddComponent<TextMeshProUGUI>();
        killerText.text = "Killed by: Enemy";
        killerText.fontSize = 24;
        killerText.alignment = TextAlignmentOptions.Center;
        killerText.color = Color.white;

        // Spacer
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(contentObj.transform);
        RectTransform spacerRect = spacer.AddComponent<RectTransform>();
        spacerRect.sizeDelta = new Vector2(100, 30);
        spacerRect.localScale = Vector3.one;

        // Respawn timer text
        GameObject timerObj = new GameObject("RespawnTimerText");
        timerObj.transform.SetParent(contentObj.transform);

        RectTransform timerRect = timerObj.AddComponent<RectTransform>();
        timerRect.sizeDelta = new Vector2(300, 40);
        timerRect.localScale = Vector3.one;

        respawnTimerText = timerObj.AddComponent<TextMeshProUGUI>();
        respawnTimerText.text = "Respawning in 3...";
        respawnTimerText.fontSize = 28;
        respawnTimerText.alignment = TextAlignmentOptions.Center;
        respawnTimerText.color = Color.white;

        // Respawn button
        GameObject buttonObj = new GameObject("RespawnButton");
        buttonObj.transform.SetParent(contentObj.transform);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.localScale = Vector3.one;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        respawnButton = buttonObj.AddComponent<Button>();
        respawnButton.targetGraphic = buttonImage;

        ColorBlock colors = respawnButton.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        respawnButton.colors = colors;

        respawnButton.onClick.AddListener(Respawn);

        // Button fill (for countdown visual)
        GameObject fillObj = new GameObject("ButtonFill");
        fillObj.transform.SetParent(buttonObj.transform);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillRect.localScale = Vector3.one;

        respawnButtonFill = fillObj.AddComponent<Image>();
        respawnButtonFill.color = new Color(0.9f, 0.2f, 0.2f, 0.5f);
        respawnButtonFill.type = Image.Type.Filled;
        respawnButtonFill.fillMethod = Image.FillMethod.Horizontal;
        respawnButtonFill.fillAmount = 0;
        respawnButtonFill.raycastTarget = false;

        // Button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        buttonTextRect.localScale = Vector3.one;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "RESPAWN";
        buttonText.fontSize = 24;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        // Initially hide button if auto respawn
        if (autoRespawn)
            respawnButton.gameObject.SetActive(false);
    }

    void OnPlayerDeath()
    {
        if (isDead) return;
        isDead = true;

        // Show death screen
        StartCoroutine(ShowDeathScreen());
    }

    public void ShowDeath(string killerName = "Enemy")
    {
        if (isDead) return;
        isDead = true;
        lastKiller = killerName;

        StartCoroutine(ShowDeathScreen());
    }

    IEnumerator ShowDeathScreen()
    {
        deathScreenPanel.SetActive(true);

        // Play death sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDeath();

        // Disable player controls
        DisablePlayerControls();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Update killer text
        if (killerText != null)
            killerText.text = $"Killed by: {lastKiller}";

        // Fade in
        float elapsed = 0f;
        Color bgStart = overlayColor;
        bgStart.a = 0;
        Color bgEnd = overlayColor;

        Color vigStart = vignetteColor;
        vigStart.a = 0;
        Color vigEnd = vignetteColor;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            if (backgroundOverlay != null)
                backgroundOverlay.color = Color.Lerp(bgStart, bgEnd, t);

            if (vignetteOverlay != null)
                vignetteOverlay.color = Color.Lerp(vigStart, vigEnd, t);

            // Scale up eliminated text
            if (eliminatedText != null)
            {
                float scale = Mathf.Lerp(0.5f, 1f, t);
                eliminatedText.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // Countdown or wait for button
        if (autoRespawn)
        {
            yield return StartCoroutine(RespawnCountdown());
        }
        else
        {
            respawnButton.gameObject.SetActive(true);
            respawnTimerText.text = "Click to respawn";
        }
    }

    IEnumerator RespawnCountdown()
    {
        float countdown = respawnDelay;

        while (countdown > 0)
        {
            countdown -= Time.deltaTime;

            if (respawnTimerText != null)
                respawnTimerText.text = $"Respawning in {Mathf.CeilToInt(countdown)}...";

            if (respawnButtonFill != null)
                respawnButtonFill.fillAmount = 1f - (countdown / respawnDelay);

            yield return null;
        }

        Respawn();
    }

    public void Respawn()
    {
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        // Fade out
        float elapsed = 0f;
        float fadeOutDuration = 0.3f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            if (backgroundOverlay != null)
            {
                Color c = backgroundOverlay.color;
                c.a = Mathf.Lerp(overlayColor.a, 1f, t);
                backgroundOverlay.color = c;
            }

            yield return null;
        }

        // Reset player
        ResetPlayer();

        yield return new WaitForSeconds(0.2f);

        // Fade back in (to game)
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            if (backgroundOverlay != null)
            {
                Color c = backgroundOverlay.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                backgroundOverlay.color = c;
            }

            yield return null;
        }

        // Hide death screen
        deathScreenPanel.SetActive(false);
        isDead = false;

        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ResetPlayer()
    {
        // Find player
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            // Reset health
            playerHealth.Heal(playerHealth.GetMaxHealth());

            // Reset position
            CharacterController cc = playerHealth.GetComponent<CharacterController>();
            if (cc != null && respawnPoint != null)
            {
                cc.enabled = false;
                playerHealth.transform.position = respawnPoint.position;
                cc.enabled = true;
            }
        }

        // Reset abilities
        AbilitySystem abilitySystem = FindObjectOfType<AbilitySystem>();
        if (abilitySystem != null)
            abilitySystem.ResetAllAbilities();

        // Enable player controls
        EnablePlayerControls();
    }

    void DisablePlayerControls()
    {
        // Disable PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        // Disable WeaponManager
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
            weaponManager.enabled = false;

        // Disable AbilitySystem
        AbilitySystem abilitySystem = FindObjectOfType<AbilitySystem>();
        if (abilitySystem != null)
            abilitySystem.enabled = false;

        // Disable BladeStorm
        BladeStorm bladeStorm = FindObjectOfType<BladeStorm>();
        if (bladeStorm != null)
            bladeStorm.enabled = false;

        // Disable all weapons
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            weapon.enabled = false;
        }
    }

    void EnablePlayerControls()
    {
        // Enable PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.enabled = true;

        // Enable WeaponManager
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
            weaponManager.enabled = true;

        // Enable AbilitySystem
        AbilitySystem abilitySystem = FindObjectOfType<AbilitySystem>();
        if (abilitySystem != null)
            abilitySystem.enabled = true;

        // Enable BladeStorm
        BladeStorm bladeStorm = FindObjectOfType<BladeStorm>();
        if (bladeStorm != null)
            bladeStorm.enabled = true;

        // Enable all weapons
        Weapon[] weapons = FindObjectsOfType<Weapon>(true);
        foreach (Weapon weapon in weapons)
        {
            weapon.enabled = true;
        }
    }

    // Track who killed the player
    public void SetKiller(string killerName)
    {
        lastKiller = killerName;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= OnPlayerDeath;
    }
}