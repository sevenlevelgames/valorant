using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Vandal";
    public WeaponType weaponType = WeaponType.Rifle;

    [Header("Shooting Settings")]
    [SerializeField] private float damage = 40f;
    [SerializeField] private float headshotMultiplier = 4f;
    [SerializeField] private float fireRate = 10f; // Rounds per second
    [SerializeField] private float range = 100f;
    [SerializeField] private bool isAutomatic = true;

    [Header("Ammo Settings")]
    [SerializeField] private int magazineSize = 25;
    [SerializeField] private int reserveAmmo = 75;
    [SerializeField] private float reloadTime = 2.5f;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilX = 0.5f;  // Horizontal recoil
    [SerializeField] private float recoilY = 2f;    // Vertical recoil
    [SerializeField] private float recoilRecoverySpeed = 5f;
    [SerializeField] private float maxRecoilX = 5f;
    [SerializeField] private float maxRecoilY = 15f;

    [Header("Accuracy Settings")]
    [SerializeField] private float baseSpread = 0.5f;
    [SerializeField] private float maxSpread = 5f;
    [SerializeField] private float spreadIncreasePerShot = 0.3f;
    [SerializeField] private float spreadRecoverySpeed = 3f;
    [SerializeField] private float movingSpreadMultiplier = 1.5f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private MuzzleFlashEffect muzzleFlashEffect;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;

    [Header("Aim Down Sights (ADS)")]
    [SerializeField] private Vector3 hipPosition = new Vector3(0.2f, -0.2f, 0.5f);
    [SerializeField] private Vector3 adsPosition = new Vector3(0f, -0.15f, 0.3f);
    [SerializeField] private float adsSpeed = 10f;
    [SerializeField] private float adsFOV = 50f;
    [SerializeField] private float normalFOV = 60f;

    // Current state
    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;
    private float currentSpread;
    private Vector2 currentRecoil;
    private Vector2 targetRecoil;
    private bool isAiming;

    // References
    private Camera playerCamera;
    private PlayerController playerController;

    // Events for UI
    public System.Action<int, int, int> OnAmmoChanged; // current, magazine, reserve

    public enum WeaponType
    {
        Pistol,
        SMG,
        Rifle,
        Sniper,
        Shotgun
    }

    void Start()
    {
        currentAmmo = magazineSize;
        playerCamera = Camera.main;
        playerController = GetComponentInParent<PlayerController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Find fire point if not assigned (check children first)
        if (firePoint == null)
        {
            Transform existingFP = transform.Find("FirePoint");
            if (existingFP != null)
            {
                firePoint = existingFP;
            }
            else
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0, 0, 0.5f);
                firePoint = fp.transform;
            }
        }

        // Find muzzle flash effect if not assigned (check firePoint children)
        if (muzzleFlashEffect == null && muzzleFlash == null)
        {
            // Check if MuzzleFlashEffect already exists on firePoint
            muzzleFlashEffect = firePoint.GetComponentInChildren<MuzzleFlashEffect>();

            if (muzzleFlashEffect == null)
            {
                GameObject mfx = new GameObject("MuzzleFlash");
                mfx.transform.SetParent(firePoint);
                mfx.transform.localPosition = Vector3.zero;
                mfx.transform.localRotation = Quaternion.identity;
                muzzleFlashEffect = mfx.AddComponent<MuzzleFlashEffect>();
            }
        }

        OnAmmoChanged?.Invoke(currentAmmo, magazineSize, reserveAmmo);
    }

    void Update()
    {
        if (isReloading) return;

        HandleShooting();
        HandleADS();
        HandleReload();
        HandleRecoilRecovery();
        HandleSpreadRecovery();
    }

    void HandleShooting()
    {
        bool shootInput = isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (shootInput && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                // Empty magazine click
                PlaySound(emptySound);
                nextFireTime = Time.time + 0.2f;
            }
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;

        // Track shot for accuracy
        if (Scoreboard.Instance != null)
            Scoreboard.Instance.AddShot();

        // Muzzle flash
        if (muzzleFlashEffect != null)
            muzzleFlashEffect.Play();
        else if (muzzleFlash != null)
            muzzleFlash.Play();

        // Sound - use SoundManager if available
        if (SoundManager.Instance != null)
        {
            if (weaponType == WeaponType.Pistol)
                SoundManager.Instance.PlayPistolShot();
            else
                SoundManager.Instance.PlayGunshot();
        }
        else
        {
            PlaySound(shootSound);
        }

        // Calculate spread
        float totalSpread = currentSpread;
        if (playerController != null && playerController.IsSprinting())
            totalSpread *= movingSpreadMultiplier;
        if (isAiming)
            totalSpread *= 0.5f; // Reduced spread when ADS

        // Calculate shot direction with spread
        Vector3 shootDirection = playerCamera.transform.forward;
        shootDirection += playerCamera.transform.right * Random.Range(-totalSpread, totalSpread) * 0.01f;
        shootDirection += playerCamera.transform.up * Random.Range(-totalSpread, totalSpread) * 0.01f;

        // Raycast - ignore player layer
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); // Ignore player layer

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, shootDirection, out hit, range, layerMask))
        {
            // Check for headshot (by tag or name)
            float finalDamage = damage;
            bool isHeadshot = false;

            // Check by tag first
            try
            {
                isHeadshot = hit.collider.CompareTag("Head");
            }
            catch
            {
                // Tag doesn't exist, check by name
                isHeadshot = hit.collider.gameObject.name.ToLower().Contains("head");
            }

            // Also check by name as backup
            if (!isHeadshot)
            {
                isHeadshot = hit.collider.gameObject.name.ToLower().Contains("head");
            }

            if (isHeadshot)
            {
                finalDamage *= headshotMultiplier;
                Debug.Log("HEADSHOT! Damage: " + finalDamage);
            }

            // Try to damage enemy
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                // Track hit for accuracy
                if (Scoreboard.Instance != null)
                    Scoreboard.Instance.AddHit();

                enemy.TakeDamage(finalDamage, isHeadshot, "Player");

                // Play hit sound
                if (SoundManager.Instance != null)
                {
                    if (isHeadshot)
                        SoundManager.Instance.PlayHeadshot();
                    else
                        SoundManager.Instance.PlayHitMarker();
                }

                // Show hit marker
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowHitMarker();
                }
            }
            else
            {
                // Try generic health component (but not player)
                Health targetHealth = hit.collider.GetComponentInParent<Health>();
                PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();

                // Only damage if it's not the player
                if (targetHealth != null && playerHealth == null)
                {
                    targetHealth.TakeDamage(finalDamage);
                }
            }

            // Impact effect
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
            else
            {
                // Use procedural impact
                BulletImpact.Spawn(hit.point, hit.normal);
            }

            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red, 0.1f);
        }

        // Apply recoil
        ApplyRecoil();

        // Increase spread
        currentSpread = Mathf.Min(currentSpread + spreadIncreasePerShot, maxSpread);

        // Update UI
        OnAmmoChanged?.Invoke(currentAmmo, magazineSize, reserveAmmo);
    }

    void ApplyRecoil()
    {
        // Add random horizontal recoil
        float horizontalRecoil = Random.Range(-recoilX, recoilX);

        targetRecoil.x += horizontalRecoil;
        targetRecoil.y += recoilY;

        // Clamp recoil
        targetRecoil.x = Mathf.Clamp(targetRecoil.x, -maxRecoilX, maxRecoilX);
        targetRecoil.y = Mathf.Clamp(targetRecoil.y, 0, maxRecoilY);
    }

    void HandleRecoilRecovery()
    {
        // Smoothly apply recoil to camera
        if (targetRecoil.magnitude > 0.01f)
        {
            Vector2 recoilStep = targetRecoil * Time.deltaTime * 20f;

            // Apply to camera (you'll need to handle this in PlayerController)
            playerCamera.transform.localRotation *= Quaternion.Euler(-recoilStep.y, recoilStep.x, 0);

            targetRecoil -= recoilStep;
        }

        // Recover recoil over time
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, recoilRecoverySpeed * Time.deltaTime);
    }

    void HandleSpreadRecovery()
    {
        if (!Input.GetMouseButton(0))
        {
            currentSpread = Mathf.Lerp(currentSpread, baseSpread, spreadRecoverySpeed * Time.deltaTime);
        }
    }

    void HandleADS()
    {
        isAiming = Input.GetMouseButton(1);

        // Lerp weapon position
        Vector3 targetPosition = isAiming ? adsPosition : hipPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, adsSpeed * Time.deltaTime);

        // Lerp FOV
        float targetFOV = isAiming ? adsFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, adsSpeed * Time.deltaTime);
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        // Sound - use SoundManager if available
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayReload();
        else
            PlaySound(reloadSound);

        Debug.Log("Reloading...");

        // Reload animation
        yield return StartCoroutine(ReloadAnimation());

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        isReloading = false;

        OnAmmoChanged?.Invoke(currentAmmo, magazineSize, reserveAmmo);
        Debug.Log("Reload complete!");
    }

    System.Collections.IEnumerator ReloadAnimation()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        // Phase 1: Move gun down and rotate
        float phase1Duration = reloadTime * 0.3f;
        float elapsed = 0f;

        Vector3 downPos = startPos + new Vector3(0, -0.15f, -0.1f);
        Quaternion downRot = startRot * Quaternion.Euler(30f, 0, 0);

        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            t = t * t; // Ease in

            transform.localPosition = Vector3.Lerp(startPos, downPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, downRot, t);

            yield return null;
        }

        // Phase 2: Hold and slight movement (mag swap)
        float phase2Duration = reloadTime * 0.4f;
        elapsed = 0f;

        Vector3 magOutPos = downPos + new Vector3(0.02f, -0.02f, 0);
        Vector3 magInPos = downPos + new Vector3(-0.02f, 0.02f, 0);

        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;

            // Slight wiggle for mag swap feel
            if (t < 0.5f)
            {
                float t2 = t * 2f;
                transform.localPosition = Vector3.Lerp(downPos, magOutPos, t2);
            }
            else
            {
                float t2 = (t - 0.5f) * 2f;
                transform.localPosition = Vector3.Lerp(magOutPos, magInPos, t2);
            }

            yield return null;
        }

        // Phase 3: Bring gun back up
        float phase3Duration = reloadTime * 0.3f;
        elapsed = 0f;

        Vector3 currentPos = transform.localPosition;
        Quaternion currentRot = transform.localRotation;

        while (elapsed < phase3Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase3Duration;
            t = 1f - Mathf.Pow(1f - t, 2f); // Ease out

            transform.localPosition = Vector3.Lerp(currentPos, startPos, t);
            transform.localRotation = Quaternion.Slerp(currentRot, startRot, t);

            yield return null;
        }

        transform.localPosition = startPos;
        transform.localRotation = startRot;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public getters
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => reserveAmmo;
    public int GetMagazineSize() => magazineSize;
    public bool IsReloading() => isReloading;
    public bool IsAiming() => isAiming;
    public float GetCurrentSpread() => currentSpread;
}