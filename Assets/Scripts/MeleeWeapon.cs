using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeDamage = 50f;
    [SerializeField] private float backstabDamage = 150f;
    [SerializeField] private float attackRate = 1.5f; // Attacks per second
    [SerializeField] private float attackDuration = 0.3f;

    [Header("Melee References")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    // State
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    // Store original values
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (isAttacking) return;

        // Left click to attack
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + (1f / attackRate);

        // Play swing sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayKnifeSwing();
        else if (swingSound != null)
            AudioSource.PlayClipAtPoint(swingSound, transform.position);

        // Animate swing
        yield return StartCoroutine(SwingAnimation());

        // Check for hit at the peak of swing
        CheckHit();

        // Return to original position
        yield return StartCoroutine(ReturnAnimation());

        isAttacking = false;
    }

    IEnumerator SwingAnimation()
    {
        float elapsed = 0f;
        float halfDuration = attackDuration * 0.5f;

        Vector3 startPos = originalPosition;
        Quaternion startRot = originalRotation;

        // Target position (swing forward and rotate)
        Vector3 swingPos = originalPosition + new Vector3(0, 0, 0.3f);
        Quaternion swingRot = originalRotation * Quaternion.Euler(-30f, 0, 0);

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            // Ease out
            t = 1f - Mathf.Pow(1f - t, 2f);

            transform.localPosition = Vector3.Lerp(startPos, swingPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, swingRot, t);

            yield return null;
        }
    }

    IEnumerator ReturnAnimation()
    {
        float elapsed = 0f;
        float halfDuration = attackDuration * 0.5f;

        Vector3 currentPos = transform.localPosition;
        Quaternion currentRot = transform.localRotation;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localPosition = Vector3.Lerp(currentPos, originalPosition, t);
            transform.localRotation = Quaternion.Slerp(currentRot, originalRotation, t);

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    void CheckHit()
    {
        Camera cam = Camera.main;
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, meleeRange))
        {
            Health targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                // Check for backstab (hitting from behind)
                float angle = Vector3.Angle(-hit.transform.forward, cam.transform.forward);
                bool isBackstab = angle < 60f;

                float finalDamage = isBackstab ? backstabDamage : meleeDamage;

                if (isBackstab)
                {
                    Debug.Log("BACKSTAB! Damage: " + finalDamage);
                }
                else
                {
                    Debug.Log("Knife hit! Damage: " + finalDamage);
                }

                targetHealth.TakeDamage(finalDamage);

                // Play hit sound
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayKnifeHit();
                else if (hitSound != null)
                    AudioSource.PlayClipAtPoint(hitSound, hit.point);

                // Show hit marker
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowHitMarker();
                }
            }

            Debug.DrawLine(cam.transform.position, hit.point, Color.yellow, 0.5f);
        }
    }

    // Melee weapons don't use ammo
    public new int GetCurrentAmmo() => 0;
    public new int GetReserveAmmo() => 0;
    public new int GetMagazineSize() => 0;
}