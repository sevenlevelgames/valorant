using UnityEngine;
using System.Collections.Generic;

public class AbilitySystem : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private Ability abilityC; // Cloudburst
    [SerializeField] private Ability abilityQ; // Updraft
    [SerializeField] private Ability abilityE; // Tailwind

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CharacterController characterController;

    // Events for UI
    public System.Action<int, int, int> OnAbilityChargesChanged; // C, Q, E charges

    void Start()
    {
        // Find player controller
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // Initialize abilities
        InitializeAbilities();
    }

    void InitializeAbilities()
    {
        if (abilityC != null)
            abilityC.Initialize(this, playerController, characterController);

        if (abilityQ != null)
            abilityQ.Initialize(this, playerController, characterController);

        if (abilityE != null)
            abilityE.Initialize(this, playerController, characterController);

        UpdateUI();
    }

    void Update()
    {
        // Ability C - Cloudburst
        if (Input.GetKeyDown(KeyCode.C) && abilityC != null)
        {
            if (abilityC.CanUse())
            {
                abilityC.Use();
                UpdateUI();
            }
        }

        // Ability Q - Updraft
        if (Input.GetKeyDown(KeyCode.Q) && abilityQ != null)
        {
            if (abilityQ.CanUse())
            {
                abilityQ.Use();
                UpdateUI();
            }
        }

        // Ability E - Tailwind
        if (Input.GetKeyDown(KeyCode.E) && abilityE != null)
        {
            if (abilityE.CanUse())
            {
                abilityE.Use();
                UpdateUI();
            }
        }
    }

    void UpdateUI()
    {
        int chargesC = abilityC != null ? abilityC.GetCurrentCharges() : 0;
        int chargesQ = abilityQ != null ? abilityQ.GetCurrentCharges() : 0;
        int chargesE = abilityE != null ? abilityE.GetCurrentCharges() : 0;

        OnAbilityChargesChanged?.Invoke(chargesC, chargesQ, chargesE);
    }

    // Public methods for external access
    public Ability GetAbilityC() => abilityC;
    public Ability GetAbilityQ() => abilityQ;
    public Ability GetAbilityE() => abilityE;

    public void ResetAllAbilities()
    {
        if (abilityC != null) abilityC.ResetCharges();
        if (abilityQ != null) abilityQ.ResetCharges();
        if (abilityE != null) abilityE.ResetCharges();
        UpdateUI();
    }
}

// Base Ability class
public abstract class Ability : MonoBehaviour
{
    [Header("Ability Info")]
    public string abilityName;
    public Sprite abilityIcon;

    [Header("Charges")]
    [SerializeField] protected int maxCharges = 2;
    [SerializeField] protected int currentCharges;

    [Header("Cooldown")]
    [SerializeField] protected float cooldown = 0f;
    protected float lastUseTime = -100f;

    [Header("Audio")]
    [SerializeField] protected AudioClip useSound;
    protected AudioSource audioSource;

    // References
    protected AbilitySystem abilitySystem;
    protected PlayerController playerController;
    protected CharacterController characterController;

    public virtual void Initialize(AbilitySystem system, PlayerController player, CharacterController cc)
    {
        abilitySystem = system;
        playerController = player;
        characterController = cc;
        currentCharges = maxCharges;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public virtual bool CanUse()
    {
        bool hasCharges = currentCharges > 0;
        bool offCooldown = Time.time >= lastUseTime + cooldown;
        return hasCharges && offCooldown;
    }

    public virtual void Use()
    {
        if (!CanUse()) return;

        currentCharges--;
        lastUseTime = Time.time;

        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound);
        }

        Execute();
    }

    protected abstract void Execute();

    public int GetCurrentCharges() => currentCharges;
    public int GetMaxCharges() => maxCharges;
    public float GetCooldownRemaining() => Mathf.Max(0, (lastUseTime + cooldown) - Time.time);

    public void ResetCharges()
    {
        currentCharges = maxCharges;
    }

    public void AddCharge(int amount = 1)
    {
        currentCharges = Mathf.Min(currentCharges + amount, maxCharges);
    }
}