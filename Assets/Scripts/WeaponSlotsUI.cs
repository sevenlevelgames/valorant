using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotsUI : MonoBehaviour
{
    [Header("Slot References")]
    [SerializeField] private GameObject slot1;
    [SerializeField] private GameObject slot2;
    [SerializeField] private GameObject slot3;

    [Header("Slot Images")]
    [SerializeField] private Image slot1Icon;
    [SerializeField] private Image slot2Icon;
    [SerializeField] private Image slot3Icon;

    [Header("Slot Borders")]
    [SerializeField] private Image slot1Border;
    [SerializeField] private Image slot2Border;
    [SerializeField] private Image slot3Border;

    [Header("Slot Labels")]
    [SerializeField] private TextMeshProUGUI slot1Label;
    [SerializeField] private TextMeshProUGUI slot2Label;
    [SerializeField] private TextMeshProUGUI slot3Label;

    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.1f);

    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;

    private int currentSlot = 1;

    void Start()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();

        if (weaponManager != null)
        {
            weaponManager.OnSlotChanged += OnSlotChanged;
            weaponManager.OnWeaponSwitched += OnWeaponSwitched;
        }

        // Set default labels
        if (slot1Label != null) slot1Label.text = "1";
        if (slot2Label != null) slot2Label.text = "2";
        if (slot3Label != null) slot3Label.text = "3";

        UpdateSlotVisuals();
    }

    void OnSlotChanged(int newSlot)
    {
        currentSlot = newSlot;
        UpdateSlotVisuals();
    }

    void OnWeaponSwitched(Weapon weapon)
    {
        UpdateSlotVisuals();
    }

    void UpdateSlotVisuals()
    {
        // Update Slot 1
        if (slot1Border != null)
        {
            bool hasWeapon = weaponManager != null && weaponManager.HasWeaponAtSlot(1);
            bool isSelected = currentSlot == 1;

            if (!hasWeapon)
                slot1Border.color = emptySlotColor;
            else if (isSelected)
                slot1Border.color = selectedColor;
            else
                slot1Border.color = unselectedColor;
        }

        // Update Slot 2
        if (slot2Border != null)
        {
            bool hasWeapon = weaponManager != null && weaponManager.HasWeaponAtSlot(2);
            bool isSelected = currentSlot == 2;

            if (!hasWeapon)
                slot2Border.color = emptySlotColor;
            else if (isSelected)
                slot2Border.color = selectedColor;
            else
                slot2Border.color = unselectedColor;
        }

        // Update Slot 3
        if (slot3Border != null)
        {
            bool hasWeapon = weaponManager != null && weaponManager.HasWeaponAtSlot(3);
            bool isSelected = currentSlot == 3;

            if (!hasWeapon)
                slot3Border.color = emptySlotColor;
            else if (isSelected)
                slot3Border.color = selectedColor;
            else
                slot3Border.color = unselectedColor;
        }
    }

    public void SetWeaponManager(WeaponManager manager)
    {
        if (weaponManager != null)
        {
            weaponManager.OnSlotChanged -= OnSlotChanged;
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
        }

        weaponManager = manager;

        if (weaponManager != null)
        {
            weaponManager.OnSlotChanged += OnSlotChanged;
            weaponManager.OnWeaponSwitched += OnWeaponSwitched;
        }

        UpdateSlotVisuals();
    }

    void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnSlotChanged -= OnSlotChanged;
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
        }
    }
}