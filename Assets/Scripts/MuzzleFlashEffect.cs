using UnityEngine;
using System.Collections;

public class MuzzleFlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private float flashSize = 0.3f;
    [SerializeField] private float lightIntensity = 3f;
    [SerializeField] private float lightRange = 8f;

    [Header("Colors")]
    [SerializeField] private Color flashColor = new Color(1f, 0.8f, 0.3f);
    [SerializeField] private Color lightColor = new Color(1f, 0.7f, 0.3f);

    [Header("Particles")]
    [SerializeField] private bool useParticles = true;
    [SerializeField] private int particleCount = 5;

    // Components
    private Light flashLight;
    private GameObject flashMesh;
    private GameObject[] flashParticles;

    void Awake()
    {
        CreateFlashComponents();
        // Start hidden
        SetFlashActive(false);
    }

    void CreateFlashComponents()
    {
        // Create point light
        GameObject lightObj = new GameObject("FlashLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        flashLight = lightObj.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.color = lightColor;
        flashLight.intensity = lightIntensity;
        flashLight.range = lightRange;
        flashLight.shadows = LightShadows.None;

        // Create flash mesh (multiple planes for 3D look)
        flashMesh = new GameObject("FlashMesh");
        flashMesh.transform.SetParent(transform);
        flashMesh.transform.localPosition = new Vector3(0, 0, 0.1f);

        // Horizontal flash plane
        CreateFlashPlane(flashMesh.transform, Vector3.zero, new Vector3(flashSize * 1.5f, flashSize * 0.3f, 0.01f));

        // Vertical flash plane
        CreateFlashPlane(flashMesh.transform, Vector3.zero, new Vector3(flashSize * 0.3f, flashSize * 1.5f, 0.01f));

        // Diagonal planes
        CreateFlashPlane(flashMesh.transform, new Vector3(0, 0, 45f), new Vector3(flashSize * 1.2f, flashSize * 0.25f, 0.01f));
        CreateFlashPlane(flashMesh.transform, new Vector3(0, 0, -45f), new Vector3(flashSize * 1.2f, flashSize * 0.25f, 0.01f));

        // Core flash (bright center)
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "FlashCore";
        core.transform.SetParent(flashMesh.transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * flashSize * 0.4f;

        Collider coreCol = core.GetComponent<Collider>();
        if (coreCol != null) Destroy(coreCol);

        Renderer coreRenderer = core.GetComponent<Renderer>();
        if (coreRenderer != null)
        {
            Material coreMat = new Material(Shader.Find("Standard"));
            coreMat.SetFloat("_Mode", 3); // Transparent
            coreMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            coreMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
            coreMat.SetInt("_ZWrite", 0);
            coreMat.DisableKeyword("_ALPHATEST_ON");
            coreMat.EnableKeyword("_ALPHABLEND_ON");
            coreMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            coreMat.renderQueue = 3000;
            coreMat.color = new Color(1f, 0.95f, 0.8f, 0.9f);
            coreMat.SetColor("_EmissionColor", Color.white * 2f);
            coreMat.EnableKeyword("_EMISSION");
            coreRenderer.material = coreMat;
        }

        // Create particles
        if (useParticles)
        {
            flashParticles = new GameObject[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                flashParticles[i] = CreateSparkParticle();
            }
        }
    }

    void CreateFlashPlane(Transform parent, Vector3 rotation, Vector3 scale)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "FlashPlane";
        plane.transform.SetParent(parent);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localEulerAngles = rotation;
        plane.transform.localScale = scale;

        Collider col = plane.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            // Setup for additive blending
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.8f);
            mat.SetColor("_EmissionColor", flashColor * 1.5f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }
    }

    GameObject CreateSparkParticle()
    {
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.name = "Spark";
        spark.transform.SetParent(transform);
        spark.transform.localPosition = Vector3.zero;
        spark.transform.localScale = Vector3.one * 0.03f;

        Collider col = spark.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer renderer = spark.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.color = new Color(1f, 0.6f, 0.2f, 0.9f);
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }

        spark.SetActive(false);
        return spark;
    }

    void SetFlashActive(bool active)
    {
        if (flashLight != null)
            flashLight.enabled = active;
        if (flashMesh != null)
            flashMesh.SetActive(active);
    }

    public void Play()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // Randomize rotation for variety
        if (flashMesh != null)
        {
            flashMesh.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            // Randomize scale slightly
            float randomScale = Random.Range(0.8f, 1.2f);
            flashMesh.transform.localScale = Vector3.one * randomScale;
        }

        // Randomize light intensity
        if (flashLight != null)
        {
            flashLight.intensity = lightIntensity * Random.Range(0.8f, 1.2f);
        }

        // Show flash
        SetFlashActive(true);

        // Spawn spark particles
        if (useParticles && flashParticles != null)
        {
            foreach (GameObject spark in flashParticles)
            {
                if (spark != null)
                {
                    StartCoroutine(AnimateSpark(spark));
                }
            }
        }

        // Flash duration with fade
        float elapsed = 0f;
        float startIntensity = flashLight != null ? flashLight.intensity : lightIntensity;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            // Fade light
            if (flashLight != null)
            {
                flashLight.intensity = Mathf.Lerp(startIntensity, 0, t);
            }

            // Fade and shrink mesh
            if (flashMesh != null)
            {
                float scale = Mathf.Lerp(1f, 0.5f, t);
                flashMesh.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // Hide flash
        SetFlashActive(false);

        // Reset scale
        if (flashMesh != null)
            flashMesh.transform.localScale = Vector3.one;
    }

    IEnumerator AnimateSpark(GameObject spark)
    {
        spark.SetActive(true);
        spark.transform.localPosition = Vector3.zero;

        // Random direction (forward cone)
        Vector3 direction = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.5f, 1f)
        ).normalized;

        float speed = Random.Range(3f, 6f);
        float lifetime = Random.Range(0.03f, 0.08f);
        float elapsed = 0f;

        Vector3 startScale = spark.transform.localScale;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Move forward
            spark.transform.localPosition += direction * speed * Time.deltaTime;

            // Shrink
            spark.transform.localScale = startScale * (1f - t);

            yield return null;
        }

        spark.SetActive(false);
        spark.transform.localScale = startScale;
    }
}