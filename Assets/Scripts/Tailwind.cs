using UnityEngine;
using System.Collections;

public class Tailwind : Ability
{
    [Header("Tailwind Settings")]
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private bool dashInMoveDirection = true; // true = WASD direction, false = look direction

    [Header("Visual Effects")]
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private Color trailColor = new Color(0.8f, 0.9f, 1f, 0.6f);
    [SerializeField] private int trailCount = 5;

    [Header("Cooldown on Kill")]
    [SerializeField] private bool resetOnKill = true;

    // State
    private bool isDashing = false;
    private Vector3 dashDirection;

    void Start()
    {
        abilityName = "Tailwind";
        maxCharges = 1;
        currentCharges = maxCharges;

        // Subscribe to kill events for reset
        if (resetOnKill)
        {
            EnemyAI.OnKill += OnPlayerKill;
        }
    }

    void OnDestroy()
    {
        if (resetOnKill)
        {
            EnemyAI.OnKill -= OnPlayerKill;
        }
    }

    void OnPlayerKill(string killer, string victim, bool headshot)
    {
        if (killer == "Player" && resetOnKill)
        {
            // Reset dash on kill (like Jett in Valorant)
            AddCharge(1);
            Debug.Log("Tailwind reset on kill!");
        }
    }

    protected override void Execute()
    {
        StartCoroutine(PerformDash());
    }

    IEnumerator PerformDash()
    {
        isDashing = true;

        // Play dash sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDash();

        // Determine dash direction
        dashDirection = GetDashDirection();

        // Create trail effect
        StartCoroutine(CreateTrailEffect());

        // Perform dash
        float elapsed = 0f;
        float dashSpeed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            // Move in dash direction
            Vector3 movement = dashDirection * dashSpeed * Time.deltaTime;
            characterController.Move(movement);

            yield return null;
        }

        isDashing = false;
        Debug.Log("Tailwind dash complete!");
    }

    Vector3 GetDashDirection()
    {
        Camera cam = Camera.main;

        if (dashInMoveDirection)
        {
            // Get WASD input direction
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // If no movement input, dash forward
            if (Mathf.Abs(horizontal) < 0.1f && Mathf.Abs(vertical) < 0.1f)
            {
                return cam.transform.forward;
            }

            // Calculate direction based on camera orientation
            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;

            // Flatten to horizontal plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * vertical + right * horizontal).normalized;
            return direction;
        }
        else
        {
            // Dash in look direction
            return cam.transform.forward;
        }
    }

    IEnumerator CreateTrailEffect()
    {
        if (dashEffectPrefab != null)
        {
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            yield break;
        }

        // Create afterimage trail
        for (int i = 0; i < trailCount; i++)
        {
            CreateAfterimage();
            yield return new WaitForSeconds(dashDuration / trailCount);
        }
    }

    void CreateAfterimage()
    {
        // Create a ghost copy at current position
        GameObject afterimage = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        afterimage.name = "DashAfterimage";
        afterimage.transform.position = transform.position + Vector3.up;
        afterimage.transform.rotation = transform.rotation;
        afterimage.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

        // Remove collider
        Collider col = afterimage.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Set transparent material
        Renderer renderer = afterimage.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = trailColor;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;
        }

        // Fade out and destroy
        StartCoroutine(FadeOutAfterimage(afterimage));
    }

    IEnumerator FadeOutAfterimage(GameObject afterimage)
    {
        Renderer renderer = afterimage.GetComponent<Renderer>();
        if (renderer == null)
        {
            Destroy(afterimage);
            yield break;
        }

        float fadeTime = 0.3f;
        float elapsed = 0f;
        Color startColor = renderer.material.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            // Fade alpha
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            renderer.material.color = c;

            yield return null;
        }

        Destroy(afterimage);
    }

    public bool IsDashing() => isDashing;
}