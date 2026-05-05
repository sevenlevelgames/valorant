using UnityEngine;
using System.Collections;

public class Cloudburst : Ability
{
    [Header("Cloudburst Settings")]
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float smokeDuration = 4.5f;
    [SerializeField] private float smokeRadius = 3f;
    [SerializeField] private float expandTime = 0.3f;

    [Header("Prefab")]
    [SerializeField] private GameObject smokeProjectilePrefab;
    [SerializeField] private GameObject smokeCloudPrefab;

    [Header("References")]
    [SerializeField] private Transform throwPoint;

    void Start()
    {
        abilityName = "Cloudburst";
        maxCharges = 2;
        currentCharges = maxCharges;

        // Create throw point if not assigned
        if (throwPoint == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                throwPoint = cam.transform;
            }
        }

        // Get player controller if not set
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }

        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }
    }

    protected override void Execute()
    {
        ThrowSmoke();
    }

    void ThrowSmoke()
    {
        // Create smoke projectile
        GameObject projectile;

        if (smokeProjectilePrefab != null)
        {
            projectile = Instantiate(smokeProjectilePrefab, throwPoint.position + throwPoint.forward * 0.5f, throwPoint.rotation);
        }
        else
        {
            // Create default projectile
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = throwPoint.position + throwPoint.forward * 0.5f;
            projectile.transform.localScale = Vector3.one * 0.2f;
            projectile.GetComponent<Renderer>().material.color = new Color(0.8f, 0.9f, 1f);
        }

        projectile.name = "CloudburstProjectile";

        // Make sure collider is set correctly
        SphereCollider col = projectile.GetComponent<SphereCollider>();
        if (col == null)
            col = projectile.AddComponent<SphereCollider>();
        col.isTrigger = false;
        col.radius = 0.5f;

        // Add rigidbody for physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.mass = 0.3f;
        rb.drag = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Throw forward
        rb.velocity = throwPoint.forward * throwForce + Vector3.up * 2f;

        // Add smoke behavior
        CloudburstProjectile smokeBehavior = projectile.AddComponent<CloudburstProjectile>();
        smokeBehavior.Initialize(smokeDuration, smokeRadius, expandTime, smokeCloudPrefab);

        // Ignore collision with player
        if (playerController != null)
        {
            Collider playerCollider = playerController.GetComponent<Collider>();
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(col, playerCollider, true);
            }

            // Also ignore CharacterController
            CharacterController cc = playerController.GetComponent<CharacterController>();
            if (cc != null)
            {
                Physics.IgnoreCollision(col, cc, true);
            }
        }

        // Play throw sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySmokeThrow();

        Debug.Log("Cloudburst thrown!");
    }
}

// Projectile behavior
public class CloudburstProjectile : MonoBehaviour
{
    private float smokeDuration;
    private float smokeRadius;
    private float expandTime;
    private GameObject smokeCloudPrefab;
    private bool hasExploded = false;

    public void Initialize(float duration, float radius, float expand, GameObject prefab)
    {
        smokeDuration = duration;
        smokeRadius = radius;
        expandTime = expand;
        smokeCloudPrefab = prefab;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    // Also explode after 2 seconds if no collision
    void Start()
    {
        Invoke(nameof(Explode), 2f);
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Play explode sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySmokeExplode();

        // Create smoke cloud
        StartCoroutine(CreateSmokeCloud());
    }

    IEnumerator CreateSmokeCloud()
    {
        GameObject smokeCloud;

        if (smokeCloudPrefab != null)
        {
            smokeCloud = Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Create default smoke cloud
            smokeCloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smokeCloud.transform.position = transform.position;
            smokeCloud.name = "SmokeCloud";

            // Remove collider so players can walk through
            Collider col = smokeCloud.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Set smoke material
            Renderer renderer = smokeCloud.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material smokeMat = new Material(Shader.Find("Standard"));
                smokeMat.color = new Color(0.7f, 0.8f, 0.9f, 0.7f);

                // Make it transparent
                smokeMat.SetFloat("_Mode", 3); // Transparent mode
                smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                smokeMat.SetInt("_ZWrite", 0);
                smokeMat.DisableKeyword("_ALPHATEST_ON");
                smokeMat.EnableKeyword("_ALPHABLEND_ON");
                smokeMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                smokeMat.renderQueue = 3000;

                renderer.material = smokeMat;
            }
        }

        // Expand animation
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 endScale = Vector3.one * smokeRadius * 2f;

        smokeCloud.transform.localScale = startScale;

        while (elapsed < expandTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandTime;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            smokeCloud.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        smokeCloud.transform.localScale = endScale;

        // Hide projectile
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Wait for smoke duration
        yield return new WaitForSeconds(smokeDuration);

        // Fade out and destroy
        Renderer smokeRenderer = smokeCloud.GetComponent<Renderer>();
        if (smokeRenderer != null)
        {
            Material mat = smokeRenderer.material;
            Color startColor = mat.color;
            elapsed = 0f;
            float fadeTime = 0.5f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeTime);
                mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(smokeCloud);
        Destroy(gameObject);
    }
}