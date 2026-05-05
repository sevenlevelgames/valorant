using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] private Weapon primaryWeapon;    // Slot 1 - Rifle/SMG
    [SerializeField] private Weapon secondaryWeapon;  // Slot 2 - Pistol
    [SerializeField] private Weapon meleeWeapon;      // Slot 3 - Knife

    [Header("Switch Settings")]
    [SerializeField] private float switchTime = 0.5f;
    [SerializeField] private float drawTime = 0.3f;

    [Header("References")]
    [SerializeField] private Transform weaponHolder;

    [Header("Weapon Skin")]
    [SerializeField] private WeaponSkin currentSkin = WeaponSkin.Default;
    [SerializeField] private KeyCode nextSkinKey = KeyCode.P;
    [SerializeField] private KeyCode prevSkinKey = KeyCode.O;

    public enum WeaponSkin
    {
        Default,
        Gold,
        RedTiger,
        Arctic,
        Carbon,
        Neon,
        Dragon,
        Galaxy
    }

    // Current state
    private Weapon currentWeapon;
    private int currentSlot = 1;
    private bool isSwitching = false;

    // Events
    public System.Action<Weapon> OnWeaponSwitched;
    public System.Action<int> OnSlotChanged;

    void Start()
    {
        // Get weapon holder (parent of weapons)
        if (weaponHolder == null)
            weaponHolder = transform;

        // Find weapons in children if not assigned
        FindWeaponsInChildren();

        // Create default weapons if missing
        CreateMissingWeapons();

        // Generate weapon models
        GenerateWeaponModels();

        // Initialize - equip primary weapon
        InitializeWeapons();
        EquipWeapon(1);
    }

    void CreateMissingWeapons()
    {
        // Create Vandal (Primary) if missing
        if (primaryWeapon == null)
        {
            GameObject vandalObj = new GameObject("Vandal");
            vandalObj.transform.SetParent(weaponHolder);
            vandalObj.transform.localPosition = new Vector3(0.2f, -0.2f, 0.4f);
            vandalObj.transform.localRotation = Quaternion.identity;

            primaryWeapon = vandalObj.AddComponent<Weapon>();
            primaryWeapon.weaponName = "Vandal";
            primaryWeapon.weaponType = Weapon.WeaponType.Rifle;
            Debug.Log("Created default primary weapon: Vandal");
        }

        // Create Classic (Secondary) if missing
        if (secondaryWeapon == null)
        {
            GameObject classicObj = new GameObject("Classic");
            classicObj.transform.SetParent(weaponHolder);
            classicObj.transform.localPosition = new Vector3(0.15f, -0.15f, 0.3f);
            classicObj.transform.localRotation = Quaternion.identity;

            secondaryWeapon = classicObj.AddComponent<Weapon>();
            secondaryWeapon.weaponName = "Classic";
            secondaryWeapon.weaponType = Weapon.WeaponType.Pistol;
            Debug.Log("Created default secondary weapon: Classic");
        }

        // Create Knife (Melee) if missing
        MeleeWeapon existingMelee = weaponHolder.GetComponentInChildren<MeleeWeapon>(true);
        if (existingMelee == null)
        {
            GameObject knifeObj = new GameObject("Knife");
            knifeObj.transform.SetParent(weaponHolder);
            knifeObj.transform.localPosition = new Vector3(0.15f, -0.15f, 0.25f);
            knifeObj.transform.localRotation = Quaternion.identity;

            MeleeWeapon knife = knifeObj.AddComponent<MeleeWeapon>();
            Debug.Log("Created default melee weapon: Knife");
        }
    }

    void GenerateWeaponModels()
    {
        // Generate model for primary weapon
        if (primaryWeapon != null)
        {
            GenerateModelForWeapon(primaryWeapon);
        }

        // Generate model for secondary weapon
        if (secondaryWeapon != null)
        {
            GenerateModelForWeapon(secondaryWeapon);
        }

        // Generate model for melee (find MeleeWeapon)
        MeleeWeapon melee = weaponHolder.GetComponentInChildren<MeleeWeapon>(true);
        if (melee != null)
        {
            GenerateKnifeModel(melee.gameObject);
        }
    }

    void GenerateModelForWeapon(Weapon weapon)
    {
        // Skip if model already exists
        if (weapon.GetComponentInChildren<WeaponModelGenerator>() != null) return;

        GameObject modelObj = new GameObject("Model");
        modelObj.transform.SetParent(weapon.transform);
        modelObj.transform.localPosition = Vector3.zero;
        modelObj.transform.localRotation = Quaternion.identity;

        WeaponModelGenerator modelGen = modelObj.AddComponent<WeaponModelGenerator>();

        // Determine model type and firepoint position
        string weaponName = weapon.weaponName.ToLower();
        Vector3 firePointPos = Vector3.zero;

        if (weaponName.Contains("vandal") || weaponName.Contains("ak"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Vandal;
            firePointPos = new Vector3(0, 0.01f, 0.42f); // Vandal barrel tip
        }
        else if (weaponName.Contains("phantom") || weaponName.Contains("m4"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Phantom;
            firePointPos = new Vector3(0, 0.005f, 0.46f); // Phantom barrel tip
        }
        else if (weaponName.Contains("spectre") || weaponName.Contains("smg"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Spectre;
            firePointPos = new Vector3(0, 0.005f, 0.22f); // Spectre barrel tip
        }
        else if (weaponName.Contains("sheriff") || weaponName.Contains("deagle"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Sheriff;
            firePointPos = new Vector3(0, 0.015f, 0.17f); // Sheriff barrel tip
        }
        else if (weapon.weaponType == Weapon.WeaponType.Pistol)
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Classic;
            firePointPos = new Vector3(0, 0.012f, 0.11f); // Classic barrel tip
        }
        else
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Vandal;
            firePointPos = new Vector3(0, 0.01f, 0.42f);
        }

        // Apply current skin
        ApplySkinToModel(modelGen);

        modelGen.GenerateModel();

        // Create FirePoint at barrel tip
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(weapon.transform);
        firePointObj.transform.localPosition = firePointPos;
        firePointObj.transform.localRotation = Quaternion.identity;

        // Add MuzzleFlashEffect to FirePoint
        MuzzleFlashEffect muzzleFlash = firePointObj.AddComponent<MuzzleFlashEffect>();

        // Use reflection to set firePoint and muzzleFlashEffect on Weapon
        System.Reflection.FieldInfo firePointField = typeof(Weapon).GetField("firePoint",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (firePointField != null)
            firePointField.SetValue(weapon, firePointObj.transform);

        System.Reflection.FieldInfo muzzleField = typeof(Weapon).GetField("muzzleFlashEffect",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (muzzleField != null)
            muzzleField.SetValue(weapon, muzzleFlash);

        Debug.Log($"Generated {modelGen.modelType} model for {weapon.weaponName} with FirePoint at {firePointPos}");
    }

    void GenerateKnifeModel(GameObject knifeObj)
    {
        if (knifeObj.GetComponentInChildren<WeaponModelGenerator>() != null) return;

        GameObject modelObj = new GameObject("Model");
        modelObj.transform.SetParent(knifeObj.transform);
        modelObj.transform.localPosition = Vector3.zero;
        modelObj.transform.localRotation = Quaternion.identity;

        WeaponModelGenerator modelGen = modelObj.AddComponent<WeaponModelGenerator>();
        modelGen.modelType = WeaponModelGenerator.WeaponModelType.Knife;

        // Apply current skin
        ApplySkinToModel(modelGen);

        modelGen.GenerateModel();
    }

    void FindWeaponsInChildren()
    {
        Weapon[] weapons = weaponHolder.GetComponentsInChildren<Weapon>(true);

        foreach (Weapon weapon in weapons)
        {
            switch (weapon.weaponType)
            {
                case Weapon.WeaponType.Rifle:
                case Weapon.WeaponType.SMG:
                case Weapon.WeaponType.Sniper:
                case Weapon.WeaponType.Shotgun:
                    if (primaryWeapon == null)
                        primaryWeapon = weapon;
                    break;
                case Weapon.WeaponType.Pistol:
                    if (secondaryWeapon == null)
                        secondaryWeapon = weapon;
                    break;
            }
        }
    }

    void InitializeWeapons()
    {
        // Disable all weapons initially
        if (primaryWeapon != null)
            primaryWeapon.gameObject.SetActive(false);
        if (secondaryWeapon != null)
            secondaryWeapon.gameObject.SetActive(false);
        if (meleeWeapon != null)
            meleeWeapon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isSwitching) return;

        HandleWeaponInput();
        HandleScrollWheel();
        HandleSkinInput();
    }

    void HandleSkinInput()
    {
        if (Input.GetKeyDown(nextSkinKey))
        {
            NextSkin();
        }
        else if (Input.GetKeyDown(prevSkinKey))
        {
            PrevSkin();
        }
    }

    public void NextSkin()
    {
        int skinCount = System.Enum.GetValues(typeof(WeaponSkin)).Length;
        currentSkin = (WeaponSkin)(((int)currentSkin + 1) % skinCount);
        ApplySkinToAllWeapons();
        Debug.Log($"Skin changed to: {currentSkin}");
    }

    public void PrevSkin()
    {
        int skinCount = System.Enum.GetValues(typeof(WeaponSkin)).Length;
        int newIndex = (int)currentSkin - 1;
        if (newIndex < 0) newIndex = skinCount - 1;
        currentSkin = (WeaponSkin)newIndex;
        ApplySkinToAllWeapons();
        Debug.Log($"Skin changed to: {currentSkin}");
    }

    public void SetSkin(WeaponSkin skin)
    {
        currentSkin = skin;
        ApplySkinToAllWeapons();
    }

    void ApplySkinToAllWeapons()
    {
        WeaponModelGenerator[] models = weaponHolder.GetComponentsInChildren<WeaponModelGenerator>(true);

        foreach (WeaponModelGenerator model in models)
        {
            ApplySkinToModel(model);
            model.GenerateModel();
        }
    }

    void ApplySkinToModel(WeaponModelGenerator model)
    {
        Color primary, secondary, accent;

        switch (currentSkin)
        {
            case WeaponSkin.Gold:
                primary = new Color(0.7f, 0.55f, 0.1f);
                secondary = new Color(0.85f, 0.7f, 0.2f);
                accent = new Color(1f, 0.85f, 0.3f);
                break;

            case WeaponSkin.RedTiger:
                primary = new Color(0.15f, 0.1f, 0.1f);
                secondary = new Color(0.6f, 0.1f, 0.1f);
                accent = new Color(0.9f, 0.2f, 0.1f);
                break;

            case WeaponSkin.Arctic:
                primary = new Color(0.85f, 0.9f, 0.95f);
                secondary = new Color(0.7f, 0.8f, 0.85f);
                accent = new Color(0.4f, 0.7f, 0.9f);
                break;

            case WeaponSkin.Carbon:
                primary = new Color(0.1f, 0.1f, 0.1f);
                secondary = new Color(0.15f, 0.15f, 0.15f);
                accent = new Color(0.3f, 0.3f, 0.3f);
                break;

            case WeaponSkin.Neon:
                primary = new Color(0.1f, 0.1f, 0.15f);
                secondary = new Color(0.15f, 0.15f, 0.2f);
                accent = new Color(0f, 1f, 0.8f);
                break;

            case WeaponSkin.Dragon:
                primary = new Color(0.1f, 0.05f, 0.05f);
                secondary = new Color(0.5f, 0.1f, 0.05f);
                accent = new Color(1f, 0.4f, 0f);
                break;

            case WeaponSkin.Galaxy:
                primary = new Color(0.1f, 0.05f, 0.2f);
                secondary = new Color(0.2f, 0.1f, 0.4f);
                accent = new Color(0.8f, 0.3f, 1f);
                break;

            default: // Default
                primary = new Color(0.15f, 0.15f, 0.15f);
                secondary = new Color(0.25f, 0.25f, 0.25f);
                accent = new Color(0.8f, 0.4f, 0.1f);
                break;
        }

        model.SetColors(primary, secondary, accent);
    }

    void HandleWeaponInput()
    {
        // Number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && primaryWeapon != null)
        {
            EquipWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && secondaryWeapon != null)
        {
            EquipWeapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && meleeWeapon != null)
        {
            EquipWeapon(3);
        }

        // Quick switch (Tab key) - switch to last weapon
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            QuickSwitch();
        }
    }

    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            // Scroll up - previous weapon
            SwitchToPrevious();
        }
        else if (scroll < 0f)
        {
            // Scroll down - next weapon
            SwitchToNext();
        }
    }

    void SwitchToNext()
    {
        int nextSlot = currentSlot + 1;
        if (nextSlot > 3) nextSlot = 1;

        // Find next available weapon
        for (int i = 0; i < 3; i++)
        {
            if (GetWeaponAtSlot(nextSlot) != null)
            {
                EquipWeapon(nextSlot);
                return;
            }
            nextSlot++;
            if (nextSlot > 3) nextSlot = 1;
        }
    }

    void SwitchToPrevious()
    {
        int prevSlot = currentSlot - 1;
        if (prevSlot < 1) prevSlot = 3;

        // Find previous available weapon
        for (int i = 0; i < 3; i++)
        {
            if (GetWeaponAtSlot(prevSlot) != null)
            {
                EquipWeapon(prevSlot);
                return;
            }
            prevSlot--;
            if (prevSlot < 1) prevSlot = 3;
        }
    }

    private int lastSlot = 1;

    void QuickSwitch()
    {
        if (lastSlot != currentSlot && GetWeaponAtSlot(lastSlot) != null)
        {
            EquipWeapon(lastSlot);
        }
    }

    public void EquipWeapon(int slot)
    {
        if (slot == currentSlot && currentWeapon != null) return;

        Weapon newWeapon = GetWeaponAtSlot(slot);
        if (newWeapon == null) return;

        StartCoroutine(SwitchWeaponRoutine(slot, newWeapon));
    }

    IEnumerator SwitchWeaponRoutine(int newSlot, Weapon newWeapon)
    {
        isSwitching = true;

        // Put away current weapon
        if (currentWeapon != null)
        {
            // Play holster animation/effect here
            yield return StartCoroutine(HolsterWeapon(currentWeapon));
        }

        // Update slots
        lastSlot = currentSlot;
        currentSlot = newSlot;

        // Draw new weapon
        currentWeapon = newWeapon;
        yield return StartCoroutine(DrawWeapon(newWeapon));

        // Notify listeners
        OnWeaponSwitched?.Invoke(currentWeapon);
        OnSlotChanged?.Invoke(currentSlot);

        isSwitching = false;
    }

    IEnumerator HolsterWeapon(Weapon weapon)
    {
        float elapsed = 0f;
        Vector3 startPos = weapon.transform.localPosition;
        Vector3 holsterPos = startPos + new Vector3(0, -0.3f, 0);

        // Animate down
        while (elapsed < switchTime * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (switchTime * 0.5f);
            weapon.transform.localPosition = Vector3.Lerp(startPos, holsterPos, t);
            yield return null;
        }

        weapon.gameObject.SetActive(false);
    }

    IEnumerator DrawWeapon(Weapon weapon)
    {
        weapon.gameObject.SetActive(true);

        Vector3 targetPos = weapon.transform.localPosition;
        Vector3 drawStartPos = targetPos + new Vector3(0, -0.3f, 0);
        weapon.transform.localPosition = drawStartPos;

        float elapsed = 0f;

        // Animate up
        while (elapsed < drawTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / drawTime;
            // Ease out
            t = 1f - Mathf.Pow(1f - t, 3f);
            weapon.transform.localPosition = Vector3.Lerp(drawStartPos, targetPos, t);
            yield return null;
        }

        weapon.transform.localPosition = targetPos;
    }

    Weapon GetWeaponAtSlot(int slot)
    {
        switch (slot)
        {
            case 1: return primaryWeapon;
            case 2: return secondaryWeapon;
            case 3: return meleeWeapon;
            default: return null;
        }
    }

    // Public methods
    public Weapon GetCurrentWeapon() => currentWeapon;
    public int GetCurrentSlot() => currentSlot;
    public bool IsSwitching() => isSwitching;

    public void SetPrimaryWeapon(Weapon weapon)
    {
        primaryWeapon = weapon;
    }

    public void SetSecondaryWeapon(Weapon weapon)
    {
        secondaryWeapon = weapon;
    }

    public void SetMeleeWeapon(Weapon weapon)
    {
        meleeWeapon = weapon;
    }

    public bool HasWeaponAtSlot(int slot)
    {
        return GetWeaponAtSlot(slot) != null;
    }
}