using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Shield Bar (Optional)")]
    [SerializeField] private Image shieldBarFill;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private GameObject shieldContainer;

    [Header("Damage Effect")]
    [SerializeField] private Image damageVignette;
    [SerializeField] private float vignetteIntensity = 0.5f;
    [SerializeField] private float vignetteFadeSpeed = 2f;

    [Header("Low Health Warning")]
    [SerializeField] private Image lowHealthOverlay;
    [SerializeField] private float lowHealthThreshold = 30f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Health Bar Colors")]
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color midHealthColor = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("References")]
    [SerializeField] private Health playerHealth;

    private float currentVignetteAlpha;
    private float previousHealth;

    void Start()
    {
        // Find player health if not assigned
        if (playerHealth == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                playerHealth = player.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthDisplay;
            previousHealth = playerHealth.GetCurrentHealth();
            UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }

        // Initialize vignette
        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0;
            damageVignette.color = c;
        }

        // Initialize low health overlay
        if (lowHealthOverlay != null)
        {
            Color c = lowHealthOverlay.color;
            c.a = 0;
            lowHealthOverlay.color = c;
        }

        // Hide shield if not used
        if (shieldContainer != null)
            shieldContainer.SetActive(false);
    }

    void Update()
    {
        HandleDamageVignette();
        HandleLowHealthPulse();
    }

    void UpdateHealthDisplay(float current, float max)
    {
        float healthPercent = current / max;

        // Update health bar fill
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
            healthBarFill.color = GetHealthColor(healthPercent);
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(current).ToString();
        }

        // Check if damage was taken
        if (current < previousHealth && damageVignette != null)
        {
            currentVignetteAlpha = vignetteIntensity;
        }
        previousHealth = current;
    }

    Color GetHealthColor(float percent)
    {
        if (percent > 0.6f)
            return fullHealthColor;
        else if (percent > 0.3f)
            return Color.Lerp(midHealthColor, fullHealthColor, (percent - 0.3f) / 0.3f);
        else
            return Color.Lerp(lowHealthColor, midHealthColor, percent / 0.3f);
    }

    void HandleDamageVignette()
    {
        if (damageVignette == null) return;

        // Fade out vignette
        if (currentVignetteAlpha > 0)
        {
            currentVignetteAlpha -= vignetteFadeSpeed * Time.deltaTime;
            currentVignetteAlpha = Mathf.Max(0, currentVignetteAlpha);

            Color c = damageVignette.color;
            c.a = currentVignetteAlpha;
            damageVignette.color = c;
        }
    }

    void HandleLowHealthPulse()
    {
        if (lowHealthOverlay == null || playerHealth == null) return;

        float currentHealth = playerHealth.GetCurrentHealth();

        if (currentHealth <= lowHealthThreshold && currentHealth > 0)
        {
            // Pulse effect
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float alpha = pulse * 0.3f;

            Color c = lowHealthOverlay.color;
            c.a = alpha;
            lowHealthOverlay.color = c;
        }
        else
        {
            Color c = lowHealthOverlay.color;
            c.a = 0;
            lowHealthOverlay.color = c;
        }
    }

    public void UpdateShield(float current, float max)
    {
        if (shieldContainer != null)
            shieldContainer.SetActive(current > 0);

        if (shieldBarFill != null)
            shieldBarFill.fillAmount = current / max;

        if (shieldText != null)
            shieldText.text = Mathf.CeilToInt(current).ToString();
    }

    public void SetPlayerHealth(Health health)
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;

        playerHealth = health;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
    }
}
