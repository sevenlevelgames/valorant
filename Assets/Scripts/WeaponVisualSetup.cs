using UnityEngine;

public class WeaponVisualSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool autoSetup = true;

    [Header("Weapon Transforms")]
    [SerializeField] private Transform primaryWeaponSlot;
    [SerializeField] private Transform secondaryWeaponSlot;
    [SerializeField] private Transform meleeWeaponSlot;

    [Header("Skin Colors")]
    [SerializeField] private WeaponSkin currentSkin = WeaponSkin.Default;

    public enum WeaponSkin
    {
        Default,
        Gold,
        RedTiger,
        Arctic,
        Carbon,
        Neon,
        Military,
        BlueSteel,
        Crimson,
        Forest
    }

    void Start()
    {
        if (autoSetup)
        {
            SetupAllWeapons();
        }
    }

    [ContextMenu("Setup All Weapons")]
    public void SetupAllWeapons()
    {
        // Find weapon manager
        WeaponManager weaponManager = GetComponentInParent<WeaponManager>();
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();

        // Setup each weapon slot
        Weapon[] weapons = GetComponentsInChildren<Weapon>(true);

        foreach (Weapon weapon in weapons)
        {
            SetupWeaponModel(weapon);
        }

        // Also setup melee
        MeleeWeapon[] melees = GetComponentsInChildren<MeleeWeapon>(true);
        foreach (MeleeWeapon melee in melees)
        {
            SetupMeleeModel(melee);
        }

        Debug.Log($"Setup {weapons.Length} weapons and {melees.Length} melee weapons with {currentSkin} skin!");
    }

    void SetupWeaponModel(Weapon weapon)
    {
        // Check if model already exists
        WeaponModelGenerator existingModel = weapon.GetComponentInChildren<WeaponModelGenerator>();
        if (existingModel != null)
        {
            ApplySkin(existingModel);
            return;
        }

        // Create model generator
        GameObject modelObj = new GameObject("Model");
        modelObj.transform.SetParent(weapon.transform);
        modelObj.transform.localPosition = Vector3.zero;
        modelObj.transform.localRotation = Quaternion.identity;
        modelObj.transform.localScale = Vector3.one;

        WeaponModelGenerator modelGen = modelObj.AddComponent<WeaponModelGenerator>();

        // Determine model type based on weapon name/type
        string weaponName = weapon.weaponName.ToLower();

        if (weaponName.Contains("vandal") || weaponName.Contains("ak") || weaponName.Contains("rifle"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Vandal;
        }
        else if (weaponName.Contains("phantom") || weaponName.Contains("m4"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Phantom;
        }
        else if (weaponName.Contains("spectre") || weaponName.Contains("smg") || weaponName.Contains("mp"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Spectre;
        }
        else if (weaponName.Contains("operator") || weaponName.Contains("awp") || weaponName.Contains("sniper"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Operator;
        }
        else if (weaponName.Contains("sheriff") || weaponName.Contains("deagle"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Sheriff;
        }
        else if (weaponName.Contains("classic") || weaponName.Contains("pistol") || weaponName.Contains("glock"))
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Classic;
        }
        else if (weapon.weaponType == Weapon.WeaponType.Sniper)
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Operator;
        }
        else if (weapon.weaponType == Weapon.WeaponType.Pistol)
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Classic;
        }
        else
        {
            modelGen.modelType = WeaponModelGenerator.WeaponModelType.Vandal;
        }

        ApplySkin(modelGen);
        modelGen.GenerateModel();
    }

    void SetupMeleeModel(MeleeWeapon melee)
    {
        WeaponModelGenerator existingModel = melee.GetComponentInChildren<WeaponModelGenerator>();
        if (existingModel != null)
        {
            ApplySkin(existingModel);
            return;
        }

        GameObject modelObj = new GameObject("Model");
        modelObj.transform.SetParent(melee.transform);
        modelObj.transform.localPosition = Vector3.zero;
        modelObj.transform.localRotation = Quaternion.identity;
        modelObj.transform.localScale = Vector3.one;

        WeaponModelGenerator modelGen = modelObj.AddComponent<WeaponModelGenerator>();
        modelGen.modelType = WeaponModelGenerator.WeaponModelType.Knife;

        ApplySkin(modelGen);
        modelGen.GenerateModel();
    }

    void ApplySkin(WeaponModelGenerator modelGen)
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

            case WeaponSkin.Military:
                primary = new Color(0.25f, 0.28f, 0.2f);
                secondary = new Color(0.35f, 0.38f, 0.28f);
                accent = new Color(0.5f, 0.45f, 0.3f);
                break;

            case WeaponSkin.BlueSteel:
                primary = new Color(0.2f, 0.22f, 0.28f);
                secondary = new Color(0.3f, 0.35f, 0.45f);
                accent = new Color(0.4f, 0.5f, 0.7f);
                break;

            case WeaponSkin.Crimson:
                primary = new Color(0.12f, 0.08f, 0.08f);
                secondary = new Color(0.4f, 0.05f, 0.1f);
                accent = new Color(0.8f, 0.1f, 0.15f);
                break;

            case WeaponSkin.Forest:
                primary = new Color(0.15f, 0.2f, 0.12f);
                secondary = new Color(0.25f, 0.32f, 0.2f);
                accent = new Color(0.4f, 0.5f, 0.3f);
                break;

            default: // Default
                primary = new Color(0.15f, 0.15f, 0.15f);
                secondary = new Color(0.25f, 0.25f, 0.25f);
                accent = new Color(0.8f, 0.4f, 0.1f);
                break;
        }

        modelGen.SetColors(primary, secondary, accent);
    }

    public void ChangeSkin(WeaponSkin newSkin)
    {
        currentSkin = newSkin;
        SetupAllWeapons();
    }
}