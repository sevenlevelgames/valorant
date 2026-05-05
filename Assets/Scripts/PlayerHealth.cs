using UnityEngine;

public class PlayerHealth : Health
{
    [Header("Player Specific")]
    [SerializeField] private float shield = 0f;
    [SerializeField] private float maxShield = 50f;

    [Header("Armor Settings")]
    [SerializeField] private bool hasLightArmor = false;  // 25 shield
    [SerializeField] private bool hasHeavyArmor = false;  // 50 shield
    [SerializeField] private float armorDamageReduction = 0.66f; // Armor absorbs 66% of damage

    // Events
    public System.Action<float, float> OnShieldChanged; // current, max
    public static System.Action OnPlayerDied; // Static event for global access

    protected override void Start()
    {
        destroyOnDeath = false; // Player should not be destroyed
        base.Start();
        UpdateArmor();
        Debug.Log($"PlayerHealth initialized with {currentHealth} HP");
    }

    public void EquipLightArmor()
    {
        hasLightArmor = true;
        hasHeavyArmor = false;
        maxShield = 25f;
        shield = 25f;
        OnShieldChanged?.Invoke(shield, maxShield);
    }

    public void EquipHeavyArmor()
    {
        hasLightArmor = false;
        hasHeavyArmor = true;
        maxShield = 50f;
        shield = 50f;
        OnShieldChanged?.Invoke(shield, maxShield);
    }

    void UpdateArmor()
    {
        if (hasHeavyArmor)
        {
            maxShield = 50f;
            shield = 50f;
        }
        else if (hasLightArmor)
        {
            maxShield = 25f;
            shield = 25f;
        }
        else
        {
            maxShield = 0f;
            shield = 0f;
        }
        OnShieldChanged?.Invoke(shield, maxShield);
    }

    private string lastDamageSource = "Enemy";

    public override void TakeDamage(float damage)
    {
        TakeDamageFrom(damage, "Enemy");
    }

    public void TakeDamageFrom(float damage, string source)
    {
        Debug.Log($"PlayerHealth.TakeDamage called with {damage} damage from {source}!");

        lastDamageSource = source;
        float remainingDamage = damage;

        // Shield absorbs damage first
        if (shield > 0)
        {
            float shieldDamage = damage * armorDamageReduction;

            if (shield >= shieldDamage)
            {
                shield -= shieldDamage;
                remainingDamage = damage * (1f - armorDamageReduction);
            }
            else
            {
                remainingDamage = damage - (shield / armorDamageReduction);
                shield = 0;
            }

            OnShieldChanged?.Invoke(shield, maxShield);
        }

        // Apply remaining damage to health
        currentHealth -= remainingDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player health now: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        Debug.Log("Player died!");

        // Set killer on death screen
        if (DeathScreen.Instance != null)
        {
            DeathScreen.Instance.SetKiller(lastDamageSource);
        }

        // Invoke static event for scoreboard etc.
        OnPlayerDied?.Invoke();

        OnDeath?.Invoke();
    }

    public float GetShield() => shield;
    public float GetMaxShield() => maxShield;
}