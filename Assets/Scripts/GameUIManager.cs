using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private WeaponUI weaponUI;
    [SerializeField] private PlayerHealthUI playerHealthUI;

    [Header("Crosshair Settings")]
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float crosshairSize = 20f;
    [SerializeField] private float crosshairThickness = 2f;
    [SerializeField] private bool showCenterDot = true;

    public static GameUIManager Instance { get; private set; }

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

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // Auto-find UI components if not assigned
        if (weaponUI == null)
            weaponUI = FindObjectOfType<WeaponUI>();

        if (playerHealthUI == null)
            playerHealthUI = FindObjectOfType<PlayerHealthUI>();
    }

    public void ShowHitMarker()
    {
        if (weaponUI != null)
            weaponUI.ShowHitMarker();
    }

    public void UpdateWeapon(Weapon weapon)
    {
        if (weaponUI != null)
            weaponUI.SetWeapon(weapon);
    }

    public void UpdatePlayerHealth(Health health)
    {
        if (playerHealthUI != null)
            playerHealthUI.SetPlayerHealth(health);
    }
}
