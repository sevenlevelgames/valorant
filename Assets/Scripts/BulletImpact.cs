using UnityEngine;
using System.Collections;

public class BulletImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float sparkCount = 8;
    [SerializeField] private float sparkSpeed = 5f;
    [SerializeField] private float decalSize = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color sparkColor = new Color(1f, 0.8f, 0.4f);
    [SerializeField] private Color decalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    private GameObject[] sparks;
    private GameObject decal;

    public static void Spawn(Vector3 position, Vector3 normal)
    {
        GameObject impactObj = new GameObject("BulletImpact");
        impactObj.transform.position = position;
        impactObj.transform.rotation = Quaternion.LookRotation(normal);

        BulletImpact impact = impactObj.AddComponent<BulletImpact>();
        impact.CreateImpact();
    }

    public void CreateImpact()
    {
        StartCoroutine(ImpactRoutine());
    }

    IEnumerator ImpactRoutine()
    {
        // Create impact flash
        GameObject flash = CreateFlash();

        // Create sparks
        sparks = new GameObject[(int)sparkCount];
        for (int i = 0; i < sparkCount; i++)
        {
            sparks[i] = CreateSpark();
            StartCoroutine(AnimateSpark(sparks[i]));
        }

        // Create decal (bullet hole)
        decal = CreateDecal();

        // Flash duration
        yield return new WaitForSeconds(0.05f);
        if (flash != null) Destroy(flash);

        // Wait for sparks
        yield return new WaitForSeconds(0.3f);

        // Fade decal
        if (decal != null)
        {
            Renderer renderer = decal.GetComponent<Renderer>();
            if (renderer != null)
            {
                float fadeTime = 0.5f;
                float elapsed = 0f;
                Color startColor = renderer.material.color;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeTime;
                    Color c = startColor;
                    c.a = Mathf.Lerp(startColor.a, 0, t);
                    renderer.material.color = c;
                    yield return null;
                }
            }
        }

        Destroy(gameObject);
    }

    GameObject CreateFlash()
    {
        GameObject flash = new GameObject("ImpactFlash");
        flash.transform.SetParent(transform);
        flash.transform.localPosition = Vector3.zero;

        // Light
        Light light = flash.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = sparkColor;
        light.intensity = 2f;
        light.range = 3f;

        return flash;
    }

    GameObject CreateSpark()
    {
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.name = "Spark";
        spark.transform.SetParent(transform);
        spark.transform.localPosition = Vector3.zero;
        spark.transform.localScale = Vector3.one * 0.02f;

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
            mat.color = sparkColor;
            mat.SetColor("_EmissionColor", sparkColor * 2f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }

        return spark;
    }

    IEnumerator AnimateSpark(GameObject spark)
    {
        // Random direction in hemisphere
        Vector3 localDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(0.2f, 1f)
        ).normalized;

        Vector3 worldDir = transform.TransformDirection(localDir);
        float speed = sparkSpeed * Random.Range(0.5f, 1.5f);
        float lifetime = Random.Range(0.1f, 0.25f);
        float elapsed = 0f;

        Vector3 velocity = worldDir * speed;
        Vector3 startScale = spark.transform.localScale;

        while (elapsed < lifetime && spark != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Move with gravity
            velocity += Physics.gravity * Time.deltaTime * 0.5f;
            spark.transform.position += velocity * Time.deltaTime;

            // Shrink
            spark.transform.localScale = startScale * (1f - t);

            yield return null;
        }

        if (spark != null)
            Destroy(spark);
    }

    GameObject CreateDecal()
    {
        GameObject decalObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decalObj.name = "BulletHole";
        decalObj.transform.SetParent(transform);
        decalObj.transform.localPosition = new Vector3(0, 0, 0.01f); // Slight offset from surface
        decalObj.transform.localRotation = Quaternion.identity;
        decalObj.transform.localScale = Vector3.one * decalSize;

        Collider col = decalObj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer renderer = decalObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 2999;
            mat.color = decalColor;
            renderer.material = mat;
        }

        return decalObj;
    }
}