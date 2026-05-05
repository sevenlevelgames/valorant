using UnityEngine;
using System.Collections;

public class Updraft : Ability
{
    [Header("Updraft Settings")]
    [SerializeField] private float upwardForce = 12f;
    [SerializeField] private float boostDuration = 0.3f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject windEffectPrefab;
    [SerializeField] private Color windColor = new Color(0.8f, 0.9f, 1f, 0.5f);

    // State
    private bool isBoosting = false;
    private Vector3 boostVelocity;

    void Start()
    {
        abilityName = "Updraft";
        maxCharges = 2;
        currentCharges = maxCharges;
    }

    protected override void Execute()
    {
        StartCoroutine(PerformUpdraft());
    }

    IEnumerator PerformUpdraft()
    {
        isBoosting = true;

        // Create wind effect at feet
        CreateWindEffect();

        // Apply upward velocity
        float elapsed = 0f;

        while (elapsed < boostDuration)
        {
            elapsed += Time.deltaTime;

            // Calculate boost strength (stronger at start)
            float t = elapsed / boostDuration;
            float boostStrength = Mathf.Lerp(upwardForce, upwardForce * 0.3f, t);

            // Move character up
            characterController.Move(Vector3.up * boostStrength * Time.deltaTime);

            yield return null;
        }

        isBoosting = false;
        Debug.Log("Updraft complete!");
    }

    void CreateWindEffect()
    {
        if (windEffectPrefab != null)
        {
            GameObject effect = Instantiate(windEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        else
        {
            // Create simple wind effect
            StartCoroutine(SimpleWindEffect());
        }
    }

    IEnumerator SimpleWindEffect()
    {
        // Create multiple small spheres going upward
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "WindParticle";
            particle.transform.localScale = Vector3.one * 0.15f;

            // Random position around player feet
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                0,
                Random.Range(-0.3f, 0.3f)
            );
            particle.transform.position = transform.position + randomOffset;

            // Remove collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Set color
            Renderer renderer = particle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = windColor;
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }

            // Animate upward
            StartCoroutine(AnimateParticle(particle));

            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator AnimateParticle(GameObject particle)
    {
        float lifetime = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;
        float speed = Random.Range(8f, 12f);

        Renderer renderer = particle.GetComponent<Renderer>();
        Color startColor = renderer != null ? renderer.material.color : Color.white;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Move up
            particle.transform.position = startPos + Vector3.up * speed * elapsed;

            // Fade out
            if (renderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                renderer.material.color = c;
            }

            // Shrink
            particle.transform.localScale = Vector3.one * 0.15f * (1f - t);

            yield return null;
        }

        Destroy(particle);
    }

    public bool IsBoosting() => isBoosting;
}