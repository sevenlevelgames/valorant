using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("Ammo Display")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;

    [Header("Reload Indicator")]
    [SerializeField] private GameObject reloadIndicator;
    [SerializeField] private Image reloadProgressBar;

    [Header("Crosshair")]
    [SerializeField] private RectTransform crosshairContainer;
    [SerializeField] private RectTransform[] crosshairLines; // Top, Bottom, Left, Right
    [SerializeField] private float crosshairBaseGap = 10f;
    [SerializeField] private float crosshairSpreadMultiplier = 5f;

    [Header("Hit Marker")]
    [SerializeField] private GameObject hitMarker;
    [SerializeField] private float hitMarkerDuration = 0.1f;

    [Header("References")]
    [SerializeField] private Weapon currentWeapon;

    [Header("Colors")]
    [SerializeField] private Color normalAmmoColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.red;
    [SerializeField] private int lowAmmoThreshold = 7;

    private Coroutine hitMarkerCoroutine;

    private WeaponManager weaponManager;

    void Start()
    {
        // Find weapon manager first
        weaponManager = FindObjectOfType<WeaponManager>();

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched += OnWeaponSwitched;

            // Wait a frame for WeaponManager to initialize
            StartCoroutine(InitializeAfterFrame());
        }
        else
        {
            // Fallback: Find weapon directly if no manager
            if (currentWeapon == null)
            {
                currentWeapon = FindObjectOfType<Weapon>();
            }
            InitializeWeapon();
        }

        // Hide reload indicator initially
        if (reloadIndicator != null)
            reloadIndicator.SetActive(false);

        // Hide hit marker initially
        if (hitMarker != null)
            hitMarker.SetActive(false);
    }

    System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null;

        Weapon weapon = weaponManager.GetCurrentWeapon();
        if (weapon != null)
        {
            SetWeapon(weapon);
        }
    }

    void InitializeWeapon()
    {
        if (currentWeapon != null)
        {
            // Subscribe to ammo changes
            currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;

            // Initial update
            UpdateAmmoDisplay(currentWeapon.GetCurrentAmmo(), currentWeapon.GetMagazineSize(), currentWeapon.GetReserveAmmo());

            if (weaponNameText != null)
                weaponNameText.text = currentWeapon.weaponName;
        }
    }

    void OnWeaponSwitched(Weapon newWeapon)
    {
        SetWeapon(newWeapon);
    }

    void Update()
    {
        if (currentWeapon == null) return;

        UpdateReloadIndicator();
        UpdateCrosshair();
    }

    void UpdateAmmoDisplay(int current, int magazineSize, int reserve)
    {
        if (ammoText != null)
        {
            ammoText.text = current.ToString();

            // Change color when low on ammo
            ammoText.color = current <= lowAmmoThreshold ? lowAmmoColor : normalAmmoColor;
        }

        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = "/ " + reserve.ToString();
        }
    }

    void UpdateReloadIndicator()
    {
        if (reloadIndicator == null) return;

        bool isReloading = currentWeapon.IsReloading();
        reloadIndicator.SetActive(isReloading);
    }

    void UpdateCrosshair()
    {
        if (crosshairLines == null || crosshairLines.Length < 4) return;

        // Calculate spread-based gap
        float spread = currentWeapon.GetCurrentSpread();
        float gap = crosshairBaseGap + (spread * crosshairSpreadMultiplier);

        // Update crosshair line positions (Top, Bottom, Left, Right)
        if (crosshairLines[0] != null) // Top
            crosshairLines[0].anchoredPosition = new Vector2(0, gap);
        if (crosshairLines[1] != null) // Bottom
            crosshairLines[1].anchoredPosition = new Vector2(0, -gap);
        if (crosshairLines[2] != null) // Left
            crosshairLines[2].anchoredPosition = new Vector2(-gap, 0);
        if (crosshairLines[3] != null) // Right
            crosshairLines[3].anchoredPosition = new Vector2(gap, 0);
    }

    public void ShowHitMarker()
    {
        if (hitMarker == null) return;

        if (hitMarkerCoroutine != null)
            StopCoroutine(hitMarkerCoroutine);

        hitMarkerCoroutine = StartCoroutine(HitMarkerRoutine());
    }

    System.Collections.IEnumerator HitMarkerRoutine()
    {
        hitMarker.SetActive(true);
        yield return new WaitForSeconds(hitMarkerDuration);
        hitMarker.SetActive(false);
    }

    public void SetWeapon(Weapon weapon)
    {
        // Unsubscribe from old weapon
        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;

        currentWeapon = weapon;

        // Subscribe to new weapon
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            UpdateAmmoDisplay(currentWeapon.GetCurrentAmmo(), currentWeapon.GetMagazineSize(), currentWeapon.GetReserveAmmo());

            if (weaponNameText != null)
                weaponNameText.text = currentWeapon.weaponName;
        }
    }

    void OnDestroy()
    {
        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;

        if (weaponManager != null)
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
    }
}