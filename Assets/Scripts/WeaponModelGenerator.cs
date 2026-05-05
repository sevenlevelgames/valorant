using UnityEngine;

public class WeaponModelGenerator : MonoBehaviour
{
    [Header("Weapon Type")]
    public WeaponModelType modelType = WeaponModelType.Vandal;

    [Header("Colors")]
    public Color primaryColor = new Color(0.12f, 0.12f, 0.12f);
    public Color secondaryColor = new Color(0.2f, 0.2f, 0.2f);
    public Color accentColor = new Color(0.9f, 0.3f, 0.1f);
    public Color metalColor = new Color(0.4f, 0.4f, 0.45f);
    public Color gripColor = new Color(0.15f, 0.12f, 0.1f);

    [Header("Quality")]
    [SerializeField] private bool highDetail = true;
    [SerializeField] private bool addAccessories = true;

    public enum WeaponModelType
    {
        Vandal,
        Phantom,
        Spectre,
        Sheriff,
        Classic,
        Operator,
        Knife
    }

    private GameObject modelRoot;
    private Material primaryMat, secondaryMat, accentMat, metalMat, gripMat, glassMat;

    void Start()
    {
        GenerateModel();
    }

    [ContextMenu("Generate Model")]
    public void GenerateModel()
    {
        if (modelRoot != null)
            DestroyImmediate(modelRoot);

        CreateMaterials();

        modelRoot = new GameObject("WeaponModel");
        modelRoot.transform.SetParent(transform);
        modelRoot.transform.localPosition = Vector3.zero;
        modelRoot.transform.localRotation = Quaternion.identity;
        modelRoot.transform.localScale = Vector3.one;

        switch (modelType)
        {
            case WeaponModelType.Vandal: CreateVandal(); break;
            case WeaponModelType.Phantom: CreatePhantom(); break;
            case WeaponModelType.Spectre: CreateSpectre(); break;
            case WeaponModelType.Sheriff: CreateSheriff(); break;
            case WeaponModelType.Classic: CreateClassic(); break;
            case WeaponModelType.Operator: CreateOperator(); break;
            case WeaponModelType.Knife: CreateKnife(); break;
        }
    }

    void CreateMaterials()
    {
        primaryMat = MakeMat(primaryColor, 0.3f, 0.2f);
        secondaryMat = MakeMat(secondaryColor, 0.4f, 0.1f);
        accentMat = MakeMat(accentColor, 0.5f, 0.3f);
        metalMat = MakeMat(metalColor, 0.7f, 0.6f);
        gripMat = MakeMat(gripColor, 0.1f, 0f);

        glassMat = new Material(Shader.Find("Standard"));
        glassMat.color = new Color(0.3f, 0.5f, 0.7f, 0.4f);
        glassMat.SetFloat("_Mode", 3);
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.SetInt("_ZWrite", 0);
        glassMat.EnableKeyword("_ALPHABLEND_ON");
        glassMat.renderQueue = 3000;
        glassMat.SetFloat("_Smoothness", 0.95f);
    }

    Material MakeMat(Color c, float smooth, float metal)
    {
        Material m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", metal);
        return m;
    }

    #region VANDAL
    void CreateVandal()
    {
        // RECEIVER
        Box("Receiver", V(0, 0, 0.03f), V(0.048f, 0.058f, 0.2f), primaryMat);
        Box("ReceiverTop", V(0, 0.032f, 0.05f), V(0.042f, 0.012f, 0.16f), secondaryMat);
        Box("ReceiverCurve", V(0, 0.02f, -0.06f), V(0.044f, 0.03f, 0.04f), primaryMat, V(-15, 0, 0));
        Box("EjectionPort", V(0.025f, 0.02f, 0.02f), V(0.004f, 0.025f, 0.05f), metalMat);

        // BARREL
        Cyl("BarrelBase", V(0, 0.008f, 0.18f), V(0.022f, 0.04f, 0.022f), primaryMat);
        Cyl("Barrel", V(0, 0.008f, 0.32f), V(0.014f, 0.12f, 0.014f), metalMat);
        Cyl("BarrelBore", V(0, 0.008f, 0.4f), V(0.006f, 0.01f, 0.006f), MakeMat(Color.black, 0, 0));

        // Muzzle brake
        Cyl("MuzzleBrake", V(0, 0.008f, 0.42f), V(0.02f, 0.03f, 0.02f), metalMat);
        Box("MuzzleVent1", V(0.018f, 0.008f, 0.42f), V(0.008f, 0.014f, 0.025f), metalMat);
        Box("MuzzleVent2", V(-0.018f, 0.008f, 0.42f), V(0.008f, 0.014f, 0.025f), metalMat);
        Box("MuzzleVentTop", V(0, 0.022f, 0.42f), V(0.014f, 0.008f, 0.025f), metalMat);

        // Gas system
        Box("GasBlock", V(0, 0.028f, 0.24f), V(0.028f, 0.022f, 0.03f), primaryMat);
        Cyl("GasTube", V(0, 0.042f, 0.15f), V(0.008f, 0.09f, 0.008f), metalMat);

        // HANDGUARD
        Box("HandguardLower", V(0, -0.005f, 0.16f), V(0.042f, 0.028f, 0.11f), secondaryMat);
        Box("HandguardUpper", V(0, 0.028f, 0.16f), V(0.038f, 0.018f, 0.1f), secondaryMat);

        if (highDetail)
        {
            for (int i = 0; i < 7; i++)
            {
                Box($"VentR{i}", V(0.022f, 0.008f, 0.11f + i * 0.013f), V(0.004f, 0.022f, 0.007f), primaryMat);
                Box($"VentL{i}", V(-0.022f, 0.008f, 0.11f + i * 0.013f), V(0.004f, 0.022f, 0.007f), primaryMat);
            }
        }

        // SIGHTS
        Box("FrontSightBase", V(0, 0.028f, 0.36f), V(0.028f, 0.018f, 0.022f), primaryMat);
        Box("FrontSightPost", V(0, 0.052f, 0.36f), V(0.004f, 0.03f, 0.004f), metalMat);
        Box("FrontSightHood", V(0, 0.058f, 0.36f), V(0.018f, 0.01f, 0.018f), primaryMat);

        Box("RearSightBase", V(0, 0.045f, 0f), V(0.032f, 0.015f, 0.028f), metalMat);
        Box("RearSightL", V(-0.012f, 0.062f, 0f), V(0.004f, 0.022f, 0.014f), metalMat);
        Box("RearSightR", V(0.012f, 0.062f, 0f), V(0.004f, 0.022f, 0.014f), metalMat);

        // MAGAZINE
        Box("MagWell", V(0, -0.025f, 0.02f), V(0.034f, 0.022f, 0.058f), primaryMat);
        Box("Magazine", V(0, -0.085f, 0.015f), V(0.028f, 0.1f, 0.05f), accentMat, V(-10, 0, 0));
        Box("MagRib1", V(0.016f, -0.085f, 0.015f), V(0.003f, 0.09f, 0.042f), primaryMat, V(-10, 0, 0));
        Box("MagRib2", V(-0.016f, -0.085f, 0.015f), V(0.003f, 0.09f, 0.042f), primaryMat, V(-10, 0, 0));
        Box("MagBase", V(0, -0.14f, 0.008f), V(0.032f, 0.014f, 0.055f), metalMat, V(-10, 0, 0));

        // GRIP
        Box("GripMain", V(0, -0.058f, -0.048f), V(0.03f, 0.075f, 0.04f), gripMat, V(14, 0, 0));
        Box("GripFront", V(0, -0.042f, -0.025f), V(0.026f, 0.045f, 0.018f), gripMat, V(14, 0, 0));

        if (highDetail)
        {
            for (int i = 0; i < 7; i++)
            {
                Box($"GripTexR{i}", V(0.016f, -0.028f - i * 0.012f, -0.048f), V(0.004f, 0.007f, 0.032f), secondaryMat, V(14, 0, 0));
                Box($"GripTexL{i}", V(-0.016f, -0.028f - i * 0.012f, -0.048f), V(0.004f, 0.007f, 0.032f), secondaryMat, V(14, 0, 0));
            }
        }

        // TRIGGER
        TriggerGroup(V(0, -0.032f, -0.01f));

        // STOCK
        Box("StockAdapter", V(0, 0f, -0.085f), V(0.042f, 0.048f, 0.035f), primaryMat);
        Box("StockBody", V(0, -0.008f, -0.15f), V(0.034f, 0.038f, 0.11f), secondaryMat);
        Box("StockEnd", V(0, -0.018f, -0.22f), V(0.026f, 0.08f, 0.03f), gripMat);
        Box("StockCheek", V(0, 0.018f, -0.17f), V(0.03f, 0.018f, 0.07f), secondaryMat);

        // CONTROLS
        Box("ChargingHandle", V(0.03f, 0.028f, 0f), V(0.018f, 0.018f, 0.035f), metalMat);
        Box("FireSelector", V(0.028f, 0.005f, -0.032f), V(0.007f, 0.01f, 0.028f), metalMat, V(0, 0, -35));

        // FOREGRIP
        if (addAccessories)
        {
            Box("ForegripMount", V(0, -0.022f, 0.14f), V(0.022f, 0.012f, 0.028f), metalMat);
            Box("Foregrip", V(0, -0.055f, 0.14f), V(0.02f, 0.05f, 0.025f), gripMat);
            Box("ForegripBase", V(0, -0.082f, 0.14f), V(0.022f, 0.008f, 0.028f), gripMat);
        }
    }
    #endregion

    #region PHANTOM
    void CreatePhantom()
    {
        // RECEIVERS
        Box("LowerReceiver", V(0, 0, 0), V(0.044f, 0.05f, 0.16f), primaryMat);
        Box("UpperReceiver", V(0, 0.03f, 0.02f), V(0.04f, 0.022f, 0.14f), primaryMat);

        // Rail
        PicatinnyRail(V(0, 0.045f, 0.04f), 0.11f, 14);

        // BARREL & SUPPRESSOR
        Cyl("Barrel", V(0, 0.018f, 0.38f), V(0.012f, 0.2f, 0.012f), metalMat);
        Cyl("SuppressorBase", V(0, 0.018f, 0.4f), V(0.024f, 0.06f, 0.024f), secondaryMat);
        Cyl("SuppressorBody", V(0, 0.018f, 0.5f), V(0.028f, 0.12f, 0.028f), primaryMat);
        Cyl("SuppressorTip", V(0, 0.018f, 0.57f), V(0.022f, 0.025f, 0.022f), metalMat);

        if (highDetail)
        {
            for (int i = 0; i < 3; i++)
            {
                Cyl($"SuppRing{i}", V(0, 0.018f, 0.42f + i * 0.05f), V(0.03f, 0.006f, 0.03f), secondaryMat);
            }
        }

        // HANDGUARD
        Box("Handguard", V(0, 0.012f, 0.17f), V(0.048f, 0.048f, 0.15f), secondaryMat);

        if (highDetail)
        {
            for (int i = 0; i < 5; i++)
            {
                Box($"MLokR{i}", V(0.025f, 0.012f, 0.1f + i * 0.028f), V(0.004f, 0.028f, 0.016f), primaryMat);
                Box($"MLokL{i}", V(-0.025f, 0.012f, 0.1f + i * 0.028f), V(0.004f, 0.028f, 0.016f), primaryMat);
                Box($"MLokB{i}", V(0, -0.015f, 0.1f + i * 0.028f), V(0.028f, 0.004f, 0.016f), primaryMat);
            }
        }

        // SIGHTS
        if (addAccessories)
        {
            Box("SightMount", V(0, 0.052f, 0.02f), V(0.028f, 0.01f, 0.045f), metalMat);
            Box("SightBody", V(0, 0.072f, 0.02f), V(0.035f, 0.028f, 0.055f), primaryMat);
            Box("SightWindow", V(0, 0.072f, 0.052f), V(0.028f, 0.02f, 0.004f), glassMat);
            Box("SightHood", V(0, 0.082f, 0.02f), V(0.038f, 0.01f, 0.06f), primaryMat);
            Box("SightReticle", V(0, 0.072f, 0.025f), V(0.002f, 0.012f, 0.002f), accentMat);
        }

        // MAGAZINE
        Box("MagWell", V(0, -0.022f, 0.01f), V(0.03f, 0.018f, 0.045f), primaryMat);
        Box("Magazine", V(0, -0.075f, 0.01f), V(0.026f, 0.088f, 0.038f), accentMat);
        Box("MagFloor", V(0, -0.122f, 0.01f), V(0.028f, 0.012f, 0.042f), metalMat);

        // GRIP
        Box("Grip", V(0, -0.052f, -0.042f), V(0.026f, 0.07f, 0.035f), gripMat, V(10, 0, 0));
        Box("GripBottom", V(0, -0.09f, -0.048f), V(0.028f, 0.014f, 0.038f), gripMat);

        if (highDetail)
        {
            for (int i = 0; i < 9; i++)
            {
                Box($"GripTex{i}", V(0, -0.03f - i * 0.009f, -0.06f), V(0.022f, 0.005f, 0.004f), secondaryMat, V(10, 0, 0));
            }
        }

        // TRIGGER
        TriggerGroup(V(0, -0.028f, -0.012f), 0.85f);

        // STOCK
        Cyl("BufferTube", V(0, 0.012f, -0.11f), V(0.02f, 0.065f, 0.02f), primaryMat);
        Box("StockBody", V(0, 0.006f, -0.19f), V(0.024f, 0.05f, 0.095f), secondaryMat);
        Box("StockPad", V(0, 0f, -0.24f), V(0.022f, 0.065f, 0.015f), gripMat);
        Box("StockCheek", V(0, 0.035f, -0.18f), V(0.02f, 0.018f, 0.06f), secondaryMat);

        // CONTROLS
        Box("ChargingHandle", V(0, 0.045f, -0.045f), V(0.05f, 0.01f, 0.022f), metalMat);
        Box("ChargingLatch", V(0.028f, 0.045f, -0.04f), V(0.014f, 0.014f, 0.018f), metalMat);
        Cyl("ForwardAssist", V(0.024f, 0.028f, -0.022f), V(0.012f, 0.01f, 0.012f), metalMat);
        Box("BoltCatch", V(0.024f, 0f, 0.02f), V(0.007f, 0.018f, 0.022f), metalMat);
        Box("MagRelease", V(0.024f, -0.012f, 0.022f), V(0.01f, 0.014f, 0.018f), metalMat);
        Box("Selector", V(0.024f, 0.005f, -0.02f), V(0.006f, 0.008f, 0.02f), metalMat);

        // FOREGRIP
        if (addAccessories)
        {
            Box("AFGMount", V(0, -0.022f, 0.16f), V(0.02f, 0.01f, 0.028f), metalMat);
            Box("AFGBody", V(0, -0.045f, 0.17f), V(0.018f, 0.04f, 0.045f), gripMat, V(-28, 0, 0));
        }
    }
    #endregion

    #region SPECTRE
    void CreateSpectre()
    {
        // RECEIVER
        Box("Receiver", V(0, 0, 0), V(0.04f, 0.045f, 0.11f), primaryMat);
        Box("ReceiverTop", V(0, 0.026f, 0.01f), V(0.036f, 0.012f, 0.09f), secondaryMat);

        // BARREL
        Cyl("Barrel", V(0, 0.01f, 0.2f), V(0.01f, 0.11f, 0.01f), metalMat);
        Cyl("BarrelShroud", V(0, 0.01f, 0.11f), V(0.02f, 0.055f, 0.02f), primaryMat);

        if (highDetail)
        {
            for (int i = 0; i < 4; i++)
            {
                Box($"ShroudVentT{i}", V(0, 0.024f, 0.08f + i * 0.022f), V(0.014f, 0.005f, 0.01f), secondaryMat);
                Box($"ShroudVentR{i}", V(0.018f, 0.01f, 0.08f + i * 0.022f), V(0.005f, 0.014f, 0.01f), secondaryMat);
                Box($"ShroudVentL{i}", V(-0.018f, 0.01f, 0.08f + i * 0.022f), V(0.005f, 0.014f, 0.01f), secondaryMat);
            }
        }

        // SIGHTS
        Box("FrontSight", V(0, 0.04f, 0.14f), V(0.006f, 0.022f, 0.006f), metalMat);
        Box("FrontSightBase", V(0, 0.032f, 0.14f), V(0.018f, 0.01f, 0.015f), primaryMat);
        Box("RearSight", V(0, 0.04f, -0.02f), V(0.025f, 0.018f, 0.018f), metalMat);
        Box("RearAperture", V(0, 0.042f, -0.015f), V(0.018f, 0.01f, 0.005f), primaryMat);

        // MAGAZINE
        Box("MagWell", V(0, -0.02f, 0.02f), V(0.028f, 0.014f, 0.038f), primaryMat);
        Box("Magazine", V(0, -0.082f, 0.02f), V(0.022f, 0.11f, 0.03f), accentMat);
        Box("MagBase", V(0, -0.14f, 0.02f), V(0.024f, 0.012f, 0.034f), metalMat);

        // GRIP
        Box("Grip", V(0, -0.045f, -0.028f), V(0.024f, 0.06f, 0.03f), gripMat, V(12, 0, 0));

        if (highDetail)
        {
            for (int i = 0; i < 6; i++)
            {
                Box($"GripTex{i}", V(0, -0.022f - i * 0.01f, -0.044f), V(0.02f, 0.005f, 0.004f), secondaryMat, V(12, 0, 0));
            }
        }

        // TRIGGER
        TriggerGroup(V(0, -0.022f, 0f), 0.75f);

        // STOCK
        Box("StockHinge", V(0, 0.012f, -0.06f), V(0.032f, 0.028f, 0.018f), primaryMat);
        Box("StockArm", V(0, 0.012f, -0.11f), V(0.01f, 0.012f, 0.09f), metalMat);
        Box("StockPad", V(0, 0.012f, -0.16f), V(0.02f, 0.05f, 0.025f), gripMat);

        // CONTROLS
        Box("ChargingHandle", V(0.024f, 0.018f, 0.035f), V(0.014f, 0.014f, 0.028f), metalMat);
        Box("Selector", V(0.022f, 0f, -0.012f), V(0.007f, 0.008f, 0.018f), metalMat);

        // FOREGRIP
        if (addAccessories)
        {
            Box("Foregrip", V(0, -0.04f, 0.075f), V(0.018f, 0.035f, 0.022f), gripMat);
        }
    }
    #endregion

    #region SHERIFF
    void CreateSheriff()
    {
        // FRAME
        Box("Frame", V(0, 0, 0), V(0.032f, 0.042f, 0.075f), primaryMat);
        Box("FrameRail", V(0, -0.016f, 0.022f), V(0.028f, 0.014f, 0.045f), primaryMat);

        // SLIDE
        Box("Slide", V(0, 0.02f, 0.018f), V(0.03f, 0.024f, 0.11f), secondaryMat);
        Box("SlideTop", V(0, 0.035f, 0.022f), V(0.026f, 0.01f, 0.09f), secondaryMat);

        if (highDetail)
        {
            for (int i = 0; i < 10; i++)
            {
                Box($"SerrationR{i}", V(0.016f, 0.02f, -0.025f + i * 0.009f), V(0.004f, 0.018f, 0.005f), primaryMat);
                Box($"SerrationL{i}", V(-0.016f, 0.02f, -0.025f + i * 0.009f), V(0.004f, 0.018f, 0.005f), primaryMat);
            }
        }

        // BARREL
        Cyl("Barrel", V(0, 0.02f, 0.14f), V(0.014f, 0.05f, 0.014f), metalMat);
        Box("BarrelWeight", V(0, 0.008f, 0.1f), V(0.028f, 0.02f, 0.055f), primaryMat);
        Cyl("Muzzle", V(0, 0.02f, 0.17f), V(0.018f, 0.018f, 0.018f), metalMat);
        Cyl("MuzzleBore", V(0, 0.02f, 0.182f), V(0.008f, 0.006f, 0.008f), MakeMat(Color.black, 0, 0));

        // SIGHTS
        Box("FrontSight", V(0, 0.048f, 0.09f), V(0.006f, 0.015f, 0.01f), accentMat);
        Box("FrontSightBase", V(0, 0.042f, 0.09f), V(0.014f, 0.008f, 0.015f), metalMat);
        Box("RearSight", V(0, 0.048f, -0.012f), V(0.022f, 0.015f, 0.018f), metalMat);
        Box("RearNotch", V(0, 0.05f, -0.006f), V(0.01f, 0.008f, 0.005f), primaryMat);

        // GRIP
        Box("Grip", V(0, -0.045f, -0.018f), V(0.03f, 0.065f, 0.038f), gripMat, V(14, 0, 0));
        Box("GripPanelR", V(0.016f, -0.045f, -0.018f), V(0.005f, 0.055f, 0.03f), secondaryMat, V(14, 0, 0));
        Box("GripPanelL", V(-0.016f, -0.045f, -0.018f), V(0.005f, 0.055f, 0.03f), secondaryMat, V(14, 0, 0));

        if (highDetail)
        {
            for (int i = 0; i < 7; i++)
            {
                Box($"GripTexR{i}", V(0.017f, -0.02f - i * 0.01f, -0.018f), V(0.006f, 0.006f, 0.024f), primaryMat, V(14, 0, 0));
            }
        }

        Box("MagBase", V(0, -0.082f, -0.012f), V(0.026f, 0.014f, 0.03f), accentMat);

        // TRIGGER
        TriggerGroup(V(0, -0.016f, 0.018f), 0.75f);

        // CONTROLS
        Box("Hammer", V(0, 0.04f, -0.04f), V(0.01f, 0.024f, 0.015f), metalMat, V(-25, 0, 0));
        Box("Safety", V(0.018f, 0.018f, -0.018f), V(0.008f, 0.012f, 0.02f), metalMat);
        Box("SlideRelease", V(0.018f, 0.006f, 0.022f), V(0.006f, 0.01f, 0.022f), metalMat);
        Box("MagRelease", V(0.018f, -0.006f, 0.012f), V(0.008f, 0.014f, 0.01f), metalMat);

        Box("EjectionPort", V(0.016f, 0.028f, 0.012f), V(0.005f, 0.014f, 0.028f), MakeMat(new Color(0.08f, 0.08f, 0.08f), 0.1f, 0));
    }
    #endregion

    #region CLASSIC
    void CreateClassic()
    {
        // FRAME
        Box("Frame", V(0, 0, 0), V(0.026f, 0.035f, 0.06f), primaryMat);

        // SLIDE
        Box("Slide", V(0, 0.016f, 0.01f), V(0.024f, 0.02f, 0.08f), secondaryMat);

        if (highDetail)
        {
            for (int i = 0; i < 7; i++)
            {
                Box($"Serr{i}", V(0.013f, 0.016f, -0.018f + i * 0.007f), V(0.003f, 0.014f, 0.004f), primaryMat);
            }
        }

        // BARREL
        Cyl("Barrel", V(0, 0.016f, 0.09f), V(0.009f, 0.03f, 0.009f), metalMat);

        // SIGHTS
        Box("FrontSight", V(0, 0.032f, 0.06f), V(0.005f, 0.01f, 0.005f), accentMat);
        Box("RearSight", V(0, 0.032f, -0.018f), V(0.018f, 0.01f, 0.01f), metalMat);

        // GRIP
        Box("Grip", V(0, -0.035f, -0.01f), V(0.024f, 0.052f, 0.028f), gripMat, V(12, 0, 0));
        Box("MagBase", V(0, -0.065f, -0.006f), V(0.02f, 0.01f, 0.022f), accentMat);

        // TRIGGER
        TriggerGroup(V(0, -0.014f, 0.014f), 0.65f);

        // CONTROLS
        Box("SlideRelease", V(0.014f, 0.006f, 0.018f), V(0.005f, 0.008f, 0.018f), metalMat);
        Box("MagRelease", V(0.014f, -0.01f, 0.01f), V(0.006f, 0.012f, 0.008f), metalMat);
    }
    #endregion

    #region OPERATOR
    void CreateOperator()
    {
        // RECEIVER
        Box("Receiver", V(0, 0, 0), V(0.048f, 0.058f, 0.24f), primaryMat);
        Box("ReceiverTop", V(0, 0.035f, 0.02f), V(0.042f, 0.014f, 0.2f), secondaryMat);

        PicatinnyRail(V(0, 0.046f, 0.02f), 0.16f, 20);

        // BARREL
        Cyl("Barrel", V(0, 0.012f, 0.5f), V(0.016f, 0.28f, 0.016f), metalMat);
        Cyl("BarrelHeavy", V(0, 0.012f, 0.28f), V(0.024f, 0.1f, 0.024f), primaryMat);

        // Muzzle brake
        Cyl("MuzzleBrake", V(0, 0.012f, 0.65f), V(0.028f, 0.045f, 0.028f), metalMat);
        Box("MuzzleVent1", V(0.024f, 0.012f, 0.65f), V(0.015f, 0.018f, 0.035f), metalMat);
        Box("MuzzleVent2", V(-0.024f, 0.012f, 0.65f), V(0.015f, 0.018f, 0.035f), metalMat);
        Box("MuzzleVent3", V(0, 0.032f, 0.65f), V(0.018f, 0.015f, 0.035f), metalMat);

        // SCOPE
        if (addAccessories)
        {
            Box("ScopeMount", V(0, 0.055f, 0.02f), V(0.032f, 0.018f, 0.09f), metalMat);
            Box("ScopeRingF", V(0, 0.072f, 0.055f), V(0.038f, 0.022f, 0.028f), metalMat);
            Box("ScopeRingR", V(0, 0.072f, -0.015f), V(0.038f, 0.022f, 0.028f), metalMat);

            Cyl("ScopeMain", V(0, 0.095f, 0.02f), V(0.03f, 0.14f, 0.03f), primaryMat);
            Cyl("ScopeObj", V(0, 0.095f, 0.12f), V(0.038f, 0.045f, 0.038f), primaryMat);
            Cyl("ScopeObjLens", V(0, 0.095f, 0.148f), V(0.032f, 0.006f, 0.032f), glassMat);
            Cyl("ScopeEye", V(0, 0.095f, -0.07f), V(0.035f, 0.035f, 0.035f), primaryMat);
            Cyl("ScopeEyeLens", V(0, 0.095f, -0.092f), V(0.028f, 0.006f, 0.028f), glassMat);

            Cyl("TurretTop", V(0, 0.13f, 0f), V(0.018f, 0.025f, 0.018f), metalMat);
            Cyl("TurretSide", V(0.04f, 0.095f, 0f), V(0.015f, 0.018f, 0.015f), metalMat);
        }

        // MAGAZINE
        Box("MagWell", V(0, -0.028f, 0.045f), V(0.038f, 0.022f, 0.065f), primaryMat);
        Box("Magazine", V(0, -0.068f, 0.045f), V(0.032f, 0.06f, 0.055f), accentMat);

        // GRIP
        Box("Grip", V(0, -0.055f, -0.045f), V(0.028f, 0.07f, 0.038f), gripMat, V(12, 0, 0));

        // TRIGGER
        TriggerGroup(V(0, -0.032f, -0.012f), 0.95f);

        // STOCK
        Box("StockBase", V(0, 0f, -0.13f), V(0.044f, 0.055f, 0.09f), primaryMat);
        Box("StockBody", V(0, -0.012f, -0.25f), V(0.032f, 0.065f, 0.16f), secondaryMat);
        Box("StockPad", V(0, -0.012f, -0.34f), V(0.028f, 0.09f, 0.022f), gripMat);
        Box("CheekRiser", V(0, 0.04f, -0.2f), V(0.028f, 0.028f, 0.09f), secondaryMat);

        // Bolt
        Box("BoltHandle", V(0.032f, 0.022f, 0.025f), V(0.028f, 0.015f, 0.018f), metalMat);
        Cyl("BoltKnob", V(0.055f, 0.022f, 0.025f), V(0.014f, 0.012f, 0.014f), metalMat);

        // BIPOD
        if (addAccessories)
        {
            Box("BipodMount", V(0, -0.032f, 0.2f), V(0.022f, 0.018f, 0.035f), metalMat);
            Box("BipodLegL", V(-0.018f, -0.095f, 0.2f), V(0.008f, 0.1f, 0.01f), metalMat, V(0, 0, 18));
            Box("BipodLegR", V(0.018f, -0.095f, 0.2f), V(0.008f, 0.1f, 0.01f), metalMat, V(0, 0, -18));
            Box("BipodFootL", V(-0.045f, -0.15f, 0.2f), V(0.018f, 0.01f, 0.018f), gripMat);
            Box("BipodFootR", V(0.045f, -0.15f, 0.2f), V(0.018f, 0.01f, 0.018f), gripMat);
        }
    }
    #endregion

    #region KNIFE
    void CreateKnife()
    {
        // BLADE
        Box("BladeMid", V(0, 0, 0.075f), V(0.005f, 0.03f, 0.11f), metalMat);
        Box("BladeEdge", V(0, -0.014f, 0.075f), V(0.002f, 0.008f, 0.105f), MakeMat(new Color(0.85f, 0.85f, 0.9f), 0.9f, 0.8f));
        Box("BladeSpine", V(0, 0.014f, 0.075f), V(0.006f, 0.007f, 0.1f), secondaryMat);
        Box("BladeTip", V(0, -0.006f, 0.14f), V(0.004f, 0.022f, 0.028f), metalMat, V(-28, 0, 0));
        Box("Fuller", V(0, 0.006f, 0.065f), V(0.002f, 0.01f, 0.07f), MakeMat(new Color(0.25f, 0.25f, 0.28f), 0.5f, 0.3f));

        if (highDetail)
        {
            for (int i = 0; i < 6; i++)
            {
                Box($"Serration{i}", V(0, -0.01f, 0.025f + i * 0.01f), V(0.004f, 0.005f, 0.006f), metalMat);
            }
        }

        // GUARD
        Box("Guard", V(0, 0, 0.015f), V(0.01f, 0.045f, 0.014f), primaryMat);
        Box("GuardFront", V(0, 0, 0.022f), V(0.007f, 0.04f, 0.006f), metalMat);

        // HANDLE
        Box("HandleBase", V(0, 0, -0.035f), V(0.02f, 0.032f, 0.08f), secondaryMat);
        Box("ScaleR", V(0.012f, 0, -0.035f), V(0.005f, 0.03f, 0.075f), gripMat);
        Box("ScaleL", V(-0.012f, 0, -0.035f), V(0.005f, 0.03f, 0.075f), gripMat);

        if (highDetail)
        {
            for (int i = 0; i < 7; i++)
            {
                Box($"WrapR{i}", V(0.014f, 0, -0.005f - i * 0.013f), V(0.004f, 0.028f, 0.007f), accentMat);
                Box($"WrapL{i}", V(-0.014f, 0, -0.005f - i * 0.013f), V(0.004f, 0.028f, 0.007f), accentMat);
            }
        }

        // POMMEL
        Box("Pommel", V(0, 0, -0.082f), V(0.018f, 0.028f, 0.018f), metalMat);
        Cyl("PommelRing", V(0, 0, -0.095f), V(0.014f, 0.01f, 0.014f), metalMat);
        Cyl("LanyardHole", V(0, 0, -0.1f), V(0.008f, 0.008f, 0.008f), primaryMat);
    }
    #endregion

    #region HELPERS
    Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

    void Box(string n, Vector3 p, Vector3 s, Material m, Vector3? r = null)
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name = n;
        o.transform.SetParent(modelRoot.transform);
        o.transform.localPosition = p;
        o.transform.localScale = s;
        o.transform.localRotation = r.HasValue ? Quaternion.Euler(r.Value) : Quaternion.identity;
        DestroyImmediate(o.GetComponent<Collider>());
        o.GetComponent<Renderer>().material = m;
    }

    void Cyl(string n, Vector3 p, Vector3 s, Material m)
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        o.name = n;
        o.transform.SetParent(modelRoot.transform);
        o.transform.localPosition = p;
        o.transform.localScale = s;
        o.transform.localRotation = Quaternion.Euler(90, 0, 0);
        DestroyImmediate(o.GetComponent<Collider>());
        o.GetComponent<Renderer>().material = m;
    }

    void TriggerGroup(Vector3 pos, float scale = 1f)
    {
        Box("TriggerGuardBot", pos + V(0, -0.018f, 0) * scale, V(0.006f, 0.005f, 0.04f) * scale, primaryMat);
        Box("TriggerGuardFront", pos + V(0, -0.006f, 0.018f) * scale, V(0.006f, 0.028f, 0.005f) * scale, primaryMat);
        Box("TriggerGuardBack", pos + V(0, -0.006f, -0.018f) * scale, V(0.006f, 0.028f, 0.005f) * scale, primaryMat);
        Box("Trigger", pos + V(0, -0.01f, 0.006f) * scale, V(0.005f, 0.022f, 0.012f) * scale, metalMat, V(18, 0, 0));
    }

    void PicatinnyRail(Vector3 pos, float len, int segs)
    {
        float w = len / segs;
        for (int i = 0; i < segs; i++)
        {
            Box($"Rail{i}", pos + V(0, 0, -len / 2 + i * w + w / 2), V(0.024f, 0.005f, w * 0.75f), metalMat);
        }
    }
    #endregion

    public void SetColors(Color primary, Color secondary, Color accent)
    {
        primaryColor = primary;
        secondaryColor = secondary;
        accentColor = accent;
    }
}