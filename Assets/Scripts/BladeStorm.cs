using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BladeStorm : MonoBehaviour
{
    [Header("Ultimate Settings")]
    [SerializeField] private int maxKnives = 5;
    [SerializeField] private float knifeDamage = 50f;
    [SerializeField] private float headshotDamage = 150f;
    [SerializeField] private float knifeSpeed = 80f;
    [SerializeField] private float knifeRange = 100f;
    [SerializeField] private float fireRate = 5f; // Single throw rate
    [SerializeField] private float burstDelay = 0.05f; // Delay between burst knives

    [Header("Ultimate Charge")]
    [SerializeField] private int killsToCharge = 6;
    [SerializeField] private bool resetKnivesOnKill = true;

    [Header("Visual Settings")]
    [SerializeField] private Color knifeColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color glowColor = new Color(0.5f, 0.8f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip refreshSound;

    // State
    private int currentKnives;
    private int currentChargeKills;
    private bool isUltimateActive = false;
    private bool canThrow = true;
    private float nextThrowTime = 0f;

    // References
    private Camera playerCamera;
    private AudioSource audioSource;
    private WeaponManager weaponManager;
    private Weapon previousWeapon;

    // UI Event
    public System.Action<int, int> OnKnivesChanged; // current, max
    public System.Action<int, int> OnChargeChanged; // current kills, needed kills
    public System.Action<bool> OnUltimateStateChanged; // active or not

    // Knife visuals in hand
    private List<GameObject> handKnives = new List<GameObject>();
    private Transform knifeHolder;

    void Start()
    {
        playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        weaponManager = FindObjectOfType<WeaponManager>();

        // Subscribe to kill events for charging
        EnemyAI.OnKill += OnEnemyKilled;

        // Create knife holder
        CreateKnifeHolder();

        // Initialize
        currentKnives = maxKnives;
        currentChargeKills = 0;

        OnChargeChanged?.Invoke(currentChargeKills, killsToCharge);
    }

    void OnDestroy()
    {
        EnemyAI.OnKill -= OnEnemyKilled;
    }

    void CreateKnifeHolder()
    {
        knifeHolder = new GameObject("KnifeHolder").transform;
        knifeHolder.SetParent(playerCamera.transform);
        knifeHolder.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
        knifeHolder.localRotation = Quaternion.identity;
    }

    void Update()
    {
        // Activate ultimate with X key
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!isUltimateActive && currentChargeKills >= killsToCharge)
            {
                ActivateUltimate();
            }
            else if (isUltimateActive)
            {
                DeactivateUltimate();
            }
        }

        // Handle throwing when ultimate is active
        if (isUltimateActive && currentKnives > 0)
        {
            // Left click - single throw
            if (Input.GetMouseButton(0) && Time.time >= nextThrowTime)
            {
                ThrowSingleKnife();
                nextThrowTime = Time.time + (1f / fireRate);
            }

            // Right click - burst throw all knives
            if (Input.GetMouseButtonDown(1) && currentKnives > 0)
            {
                StartCoroutine(ThrowBurst());
            }
        }

        // Auto deactivate when out of knives
        if (isUltimateActive && currentKnives <= 0)
        {
            StartCoroutine(DeactivateAfterDelay(0.5f));
        }
    }

    void ActivateUltimate()
    {
        isUltimateActive = true;
        currentKnives = maxKnives;
        currentChargeKills = 0;

        // Play sound
        if (activateSound != null)
            audioSource.PlayOneShot(activateSound);

        // Hide current weapon
        if (weaponManager != null)
        {
            previousWeapon = weaponManager.GetCurrentWeapon();
            if (previousWeapon != null)
                previousWeapon.gameObject.SetActive(false);
        }

        // Show knives in hand
        CreateHandKnives();

        OnUltimateStateChanged?.Invoke(true);
        OnKnivesChanged?.Invoke(currentKnives, maxKnives);
        OnChargeChanged?.Invoke(0, killsToCharge);

        Debug.Log("BLADE STORM ACTIVATED!");
    }

    void DeactivateUltimate()
    {
        isUltimateActive = false;

        // Show previous weapon
        if (previousWeapon != null)
            previousWeapon.gameObject.SetActive(true);

        // Remove hand knives
        ClearHandKnives();

        OnUltimateStateChanged?.Invoke(false);

        Debug.Log("Blade Storm deactivated");
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentKnives <= 0)
            DeactivateUltimate();
    }

    void CreateHandKnives()
    {
        ClearHandKnives();

        for (int i = 0; i < currentKnives; i++)
        {
            GameObject knife = CreateKnifeVisual();
            knife.transform.SetParent(knifeHolder);

            // Fan out knives
            float angle = (i - (currentKnives - 1) / 2f) * 15f;
            knife.transform.localPosition = new Vector3(i * 0.05f - 0.1f, 0, 0);
            knife.transform.localRotation = Quaternion.Euler(0, angle, -90f);

            handKnives.Add(knife);
        }
    }

    void UpdateHandKnives()
    {
        // Remove extra knives
        while (handKnives.Count > currentKnives)
        {
            int lastIndex = handKnives.Count - 1;
            Destroy(handKnives[lastIndex]);
            handKnives.RemoveAt(lastIndex);
        }
    }

    void ClearHandKnives()
    {
        foreach (GameObject knife in handKnives)
        {
            if (knife != null)
                Destroy(knife);
        }
        handKnives.Clear();
    }

    GameObject CreateKnifeVisual()
    {
        GameObject knife = new GameObject("Knife");

        // Blade
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(knife.transform);
        blade.transform.localPosition = new Vector3(0, 0, 0.1f);
        blade.transform.localScale = new Vector3(0.02f, 0.15f, 0.01f);

        Collider bladeCol = blade.GetComponent<Collider>();
        if (bladeCol != null) Destroy(bladeCol);

        Renderer bladeRenderer = blade.GetComponent<Renderer>();
        if (bladeRenderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = knifeColor;
            mat.SetColor("_EmissionColor", glowColor * 0.5f);
            mat.EnableKeyword("_EMISSION");
            bladeRenderer.material = mat;
        }

        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(knife.transform);
        handle.transform.localPosition = new Vector3(0, 0, -0.02f);
        handle.transform.localScale = new Vector3(0.015f, 0.04f, 0.015f);

        Collider handleCol = handle.GetComponent<Collider>();
        if (handleCol != null) Destroy(handleCol);

        Renderer handleRenderer = handle.GetComponent<Renderer>();
        if (handleRenderer != null)
        {
            handleRenderer.material.color = new Color(0.2f, 0.2f, 0.25f);
        }

        return knife;
    }

    void ThrowSingleKnife()
    {
        if (currentKnives <= 0) return;

        currentKnives--;

        // Play sound
        if (throwSound != null)
            audioSource.PlayOneShot(throwSound);

        // Spawn and throw knife
        SpawnThrownKnife(playerCamera.transform.forward);

        // Update hand visuals
        UpdateHandKnives();

        OnKnivesChanged?.Invoke(currentKnives, maxKnives);
    }

    IEnumerator ThrowBurst()
    {
        canThrow = false;

        int knivesToThrow = currentKnives;
        float spreadAngle = 3f;

        for (int i = 0; i < knivesToThrow; i++)
        {
            if (currentKnives <= 0) break;

            currentKnives--;

            // Calculate spread direction
            float horizontalSpread = (i - (knivesToThrow - 1) / 2f) * spreadAngle;
            Vector3 direction = Quaternion.Euler(0, horizontalSpread, 0) * playerCamera.transform.forward;

            // Play sound
            if (throwSound != null)
                audioSource.PlayOneShot(throwSound, 0.7f);

            SpawnThrownKnife(direction);

            OnKnivesChanged?.Invoke(currentKnives, maxKnives);

            yield return new WaitForSeconds(burstDelay);
        }

        UpdateHandKnives();
        canThrow = true;
    }

    void SpawnThrownKnife(Vector3 direction)
    {
        GameObject knife = CreateKnifeVisual();
        knife.name = "ThrownKnife";
        knife.transform.position = playerCamera.transform.position + direction * 0.5f;
        knife.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0, 0);

        // Add projectile component
        BladeStormKnife knifeScript = knife.AddComponent<BladeStormKnife>();
        knifeScript.Initialize(direction, knifeSpeed, knifeRange, knifeDamage, headshotDamage, this);

        // Add collider
        BoxCollider col = knife.AddComponent<BoxCollider>();
        col.size = new Vector3(0.05f, 0.2f, 0.05f);
        col.isTrigger = true;

        // Add rigidbody
        Rigidbody rb = knife.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnEnemyKilled(string killer, string victim, bool headshot)
    {
        if (killer != "Player") return;

        if (isUltimateActive && resetKnivesOnKill)
        {
            // Refresh knives on kill
            currentKnives = maxKnives;
            CreateHandKnives();
            OnKnivesChanged?.Invoke(currentKnives, maxKnives);

            if (refreshSound != null)
                audioSource.PlayOneShot(refreshSound);

            Debug.Log("Knives refreshed!");
        }
        else if (!isUltimateActive)
        {
            // Charge ultimate
            currentChargeKills++;
            OnChargeChanged?.Invoke(currentChargeKills, killsToCharge);

            if (currentChargeKills >= killsToCharge)
            {
                Debug.Log("BLADE STORM READY! Press X to activate.");
            }
        }
    }

    public void OnKnifeHit(bool headshot)
    {
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }

    // Public getters
    public bool IsUltimateActive() => isUltimateActive;
    public bool IsUltimateReady() => currentChargeKills >= killsToCharge;
    public int GetCurrentKnives() => currentKnives;
    public int GetMaxKnives() => maxKnives;
    public float GetChargePercent() => (float)currentChargeKills / killsToCharge;
}

// Thrown knife projectile
public class BladeStormKnife : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float maxDistance;
    private float damage;
    private float headshotDamage;
    private BladeStorm owner;

    private Vector3 startPos;
    private bool hasHit = false;

    public void Initialize(Vector3 dir, float spd, float range, float dmg, float hsDmg, BladeStorm bladeStorm)
    {
        direction = dir.normalized;
        speed = spd;
        maxDistance = range;
        damage = dmg;
        headshotDamage = hsDmg;
        owner = bladeStorm;
        startPos = transform.position;
    }

    void Update()
    {
        if (hasHit) return;

        // Move forward
        transform.position += direction * speed * Time.deltaTime;

        // Rotate for visual effect
        transform.Rotate(Vector3.up * 720f * Time.deltaTime, Space.Self);

        // Check distance
        if (Vector3.Distance(startPos, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Ignore player
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
            return;

        hasHit = true;

        // Check for headshot (by tag or name)
        bool isHeadshot = false;
        try
        {
            isHeadshot = other.CompareTag("Head");
        }
        catch
        {
            isHeadshot = other.gameObject.name.ToLower().Contains("head");
        }

        if (!isHeadshot)
        {
            isHeadshot = other.gameObject.name.ToLower().Contains("head");
        }

        float finalDamage = isHeadshot ? headshotDamage : damage;

        // Try to damage enemy
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(finalDamage, isHeadshot, "Player");

            if (owner != null)
                owner.OnKnifeHit(isHeadshot);

            if (isHeadshot)
                Debug.Log("BLADE STORM HEADSHOT!");

            // Show hit marker
            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowHitMarker();
        }

        // Stick into surface briefly then destroy
        StartCoroutine(StickAndDestroy());
    }

    IEnumerator StickAndDestroy()
    {
        // Stop moving
        speed = 0;

        yield return new WaitForSeconds(0.5f);

        // Fade out
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float fadeTime = 0.3f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);

            foreach (Renderer r in renderers)
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}