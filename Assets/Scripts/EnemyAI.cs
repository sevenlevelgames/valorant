using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyName = "Enemy";
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Combat")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float accuracy = 0.7f; // 0-1, higher = more accurate

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject muzzleFlashEffect;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip alertSound;

    // Components
    private NavMeshAgent agent;
    private AudioSource audioSource;

    // State
    private enum State { Idle, Patrol, Chase, Attack, Dead }
    private State currentState = State.Idle;

    // Target
    private Transform player;
    private Vector3 lastKnownPosition;
    private bool playerInSight;

    // Patrol
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;

    // Combat
    private float nextFireTime = 0f;

    // Events
    public System.Action<EnemyAI, string> OnEnemyDeath; // enemy, killerName
    public static System.Action<string, string, bool> OnKill; // killer, victim, headshot

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Find player
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
        }

        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0.3f, 1.2f, 0.5f);
            firePoint = fp.transform;
        }

        // Create simple weapon visual
        CreateWeaponVisual();

        agent.speed = walkSpeed;
    }

    void CreateWeaponVisual()
    {
        GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weapon.name = "EnemyWeapon";
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = new Vector3(0.3f, 1.1f, 0.3f);
        weapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
        weapon.transform.localScale = new Vector3(0.08f, 0.08f, 0.35f);

        // Remove collider
        Collider col = weapon.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Dark color for weapon
        Renderer renderer = weapon.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        CheckPlayerVisibility();

        switch (currentState)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Patrol:
                PatrolBehavior();
                break;
            case State.Chase:
                ChaseBehavior();
                break;
            case State.Attack:
                AttackBehavior();
                break;
        }
    }

    void CheckPlayerVisibility()
    {
        if (player == null) return;

        playerInSight = false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            // Check if player is in field of view
            if (angle < fieldOfView / 2f)
            {
                // Raycast to check for obstacles
                Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
                Vector3 playerCenter = player.position + Vector3.up * 1f;

                if (!Physics.Linecast(eyePosition, playerCenter, obstacleMask))
                {
                    playerInSight = true;
                    lastKnownPosition = player.position;

                    // State transitions based on distance
                    if (distanceToPlayer <= attackRange)
                    {
                        SetState(State.Attack);
                    }
                    else
                    {
                        SetState(State.Chase);
                    }
                }
            }
        }

        // Lost sight of player
        if (!playerInSight && (currentState == State.Attack || currentState == State.Chase))
        {
            // Go to last known position
            agent.SetDestination(lastKnownPosition);

            if (Vector3.Distance(transform.position, lastKnownPosition) < 2f)
            {
                SetState(State.Patrol);
            }
        }
    }

    void IdleBehavior()
    {
        // Just stand and look around, then start patrol
        patrolWaitTimer += Time.deltaTime;
        if (patrolWaitTimer >= patrolWaitTime)
        {
            SetState(State.Patrol);
        }
    }

    void PatrolBehavior()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetState(State.Idle);
            return;
        }

        agent.speed = walkSpeed;

        // Check if reached patrol point
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                // Move to next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                patrolWaitTimer = 0f;
            }
        }
    }

    void ChaseBehavior()
    {
        if (player == null) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        // Look at player
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), 5f * Time.deltaTime);
        }
    }

    void AttackBehavior()
    {
        if (player == null) return;

        // Stop moving
        agent.SetDestination(transform.position);

        // Look at player
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), 10f * Time.deltaTime);
        }

        // Shoot
        if (Time.time >= nextFireTime && playerInSight)
        {
            Shoot();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    void Shoot()
    {
        // Muzzle flash
        if (muzzleFlashEffect != null)
        {
            GameObject flash = Instantiate(muzzleFlashEffect, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
        else
        {
            // Create simple muzzle flash
            StartCoroutine(SimpleMuzzleFlash());
        }

        // Sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Calculate shot direction with accuracy
        Vector3 targetPosition = player.position + Vector3.up * 1f; // Aim at chest
        Vector3 shootDirection = (targetPosition - firePoint.position).normalized;

        // Add inaccuracy
        float inaccuracy = (1f - accuracy) * 0.15f;
        shootDirection += new Vector3(
            Random.Range(-inaccuracy, inaccuracy),
            Random.Range(-inaccuracy, inaccuracy),
            Random.Range(-inaccuracy, inaccuracy)
        );
        shootDirection.Normalize();

        // Show bullet trail
        StartCoroutine(ShowBulletTrail(firePoint.position, firePoint.position + shootDirection * attackRange));

        // Simple distance-based hit detection (more reliable)
        float distanceToPlayer = Vector3.Distance(firePoint.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Check if shot is accurate enough to hit
            Vector3 toPlayer = (player.position + Vector3.up * 1f - firePoint.position).normalized;
            float aimDot = Vector3.Dot(shootDirection, toPlayer);

            // If aim is close enough (within ~10 degrees), it's a hit
            if (aimDot > 0.98f)
            {
                // Find PlayerHealth on player
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                    playerHealth = player.GetComponentInChildren<PlayerHealth>();
                if (playerHealth == null)
                    playerHealth = FindObjectOfType<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamageFrom(damage, enemyName);
                    Debug.Log($"{enemyName} HIT player for {damage} damage! Health: {playerHealth.GetCurrentHealth()}");
                    StartCoroutine(ShowImpactEffect(player.position + Vector3.up * 1f));
                }
                else
                {
                    Debug.LogError("PlayerHealth not found!");
                }
            }
            else
            {
                Debug.Log($"{enemyName} missed! (aim accuracy: {aimDot})");
            }
        }

        Debug.DrawLine(firePoint.position, firePoint.position + shootDirection * attackRange, Color.red, 0.5f);
    }

    System.Collections.IEnumerator SimpleMuzzleFlash()
    {
        // Create flash object
        GameObject flashObj = new GameObject("MuzzleFlash");
        flashObj.transform.position = firePoint.position;
        flashObj.transform.rotation = firePoint.rotation;

        // Create point light
        Light flashLight = flashObj.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.color = new Color(1f, 0.8f, 0.3f);
        flashLight.intensity = 4f;
        flashLight.range = 8f;

        // Create flash core
        GameObject flashCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flashCore.transform.SetParent(flashObj.transform);
        flashCore.transform.localPosition = Vector3.zero;
        flashCore.transform.localScale = Vector3.one * 0.12f;

        Collider coreCol = flashCore.GetComponent<Collider>();
        if (coreCol != null) Destroy(coreCol);

        Renderer coreRenderer = flashCore.GetComponent<Renderer>();
        if (coreRenderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.color = new Color(1f, 0.95f, 0.8f, 0.9f);
            mat.SetColor("_EmissionColor", Color.white * 3f);
            mat.EnableKeyword("_EMISSION");
            coreRenderer.material = mat;
        }

        // Create flash rays
        for (int i = 0; i < 4; i++)
        {
            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ray.transform.SetParent(flashObj.transform);
            ray.transform.localPosition = new Vector3(0, 0, 0.05f);
            ray.transform.localRotation = Quaternion.Euler(0, 0, i * 45f);
            ray.transform.localScale = new Vector3(0.25f, 0.06f, 1f);

            Collider rayCol = ray.GetComponent<Collider>();
            if (rayCol != null) Destroy(rayCol);

            Renderer rayRenderer = ray.GetComponent<Renderer>();
            if (rayRenderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.color = new Color(1f, 0.8f, 0.3f, 0.8f);
                mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.2f) * 2f);
                mat.EnableKeyword("_EMISSION");
                rayRenderer.material = mat;
            }
        }

        // Animate fade out
        float duration = 0.06f;
        float elapsed = 0f;
        float startIntensity = flashLight.intensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            flashLight.intensity = Mathf.Lerp(startIntensity, 0, t);
            flashObj.transform.localScale = Vector3.one * (1f - t * 0.5f);
            yield return null;
        }

        Destroy(flashObj);
    }

    System.Collections.IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        // Create line renderer for bullet trail
        GameObject trailObj = new GameObject("BulletTrail");
        LineRenderer line = trailObj.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = 0.02f;
        line.endWidth = 0.02f;

        // Create material
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1f, 0.8f, 0.3f);
        line.endColor = new Color(1f, 0.6f, 0.2f, 0.5f);

        yield return new WaitForSeconds(0.05f);
        Destroy(trailObj);
    }

    System.Collections.IEnumerator ShowImpactEffect(Vector3 position)
    {
        // Create impact sphere
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.name = "ImpactEffect";
        impact.transform.position = position;
        impact.transform.localScale = Vector3.one * 0.1f;

        Collider col = impact.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer renderer = impact.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        // Expand and fade
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            impact.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.3f, t);

            if (renderer != null)
            {
                Color c = renderer.material.color;
                c.a = 1f - t;
                renderer.material.color = c;
            }

            yield return null;
        }

        Destroy(impact);
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;

        // Alert sound when first spotting player
        if ((currentState == State.Idle || currentState == State.Patrol) &&
            (newState == State.Chase || newState == State.Attack))
        {
            if (alertSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(alertSound);
            }
        }

        currentState = newState;
    }

    public void TakeDamage(float damage, bool isHeadshot = false, string attackerName = "Player")
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;

        // Alert when taking damage
        if (currentState == State.Idle || currentState == State.Patrol)
        {
            lastKnownPosition = player != null ? player.position : transform.position;
            SetState(State.Chase);
        }

        Debug.Log($"{enemyName} took {damage} damage. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(attackerName, isHeadshot);
        }
    }

    void Die(string killerName, bool wasHeadshot)
    {
        currentState = State.Dead;

        // Stop movement
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Trigger kill feed
        OnKill?.Invoke(killerName, enemyName, wasHeadshot);
        OnEnemyDeath?.Invoke(this, killerName);

        Debug.Log($"{enemyName} was killed by {killerName}!" + (wasHeadshot ? " (HEADSHOT)" : ""));

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Start death effect
        StartCoroutine(DeathEffect(wasHeadshot));
    }

    System.Collections.IEnumerator DeathEffect(bool wasHeadshot)
    {
        // Get all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // If headshot, do a more dramatic effect
        if (wasHeadshot)
        {
            // Spawn blood particles
            StartCoroutine(SpawnDeathParticles(transform.position + Vector3.up * 2f, 15));
        }
        else
        {
            StartCoroutine(SpawnDeathParticles(transform.position + Vector3.up * 1f, 8));
        }

        // Fall down animation
        float fallDuration = 0.5f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Random fall direction
        Vector3 fallDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Quaternion targetRot = Quaternion.Euler(Random.Range(-90f, 90f), transform.eulerAngles.y, 90f);

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // Ease out
            float easeT = 1f - Mathf.Pow(1f - t, 2f);

            // Fall and rotate
            transform.position = startPos + fallDirection * 0.3f * easeT + Vector3.down * 0.5f * easeT;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, easeT);

            yield return null;
        }

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        // Fade out
        float fadeDuration = 1f;
        elapsed = 0f;

        // Store original colors
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].material.color;

                // Enable transparency
                Material mat = renderers[i].material;
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Sink into ground while fading
            transform.position += Vector3.down * Time.deltaTime * 0.5f;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    Color c = originalColors[i];
                    c.a = 1f - t;
                    renderers[i].material.color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    System.Collections.IEnumerator SpawnDeathParticles(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create particle
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "DeathParticle";
            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);

            // Remove collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Red color
            Renderer renderer = particle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.8f, 0.1f, 0.1f);
                mat.SetColor("_EmissionColor", new Color(0.5f, 0f, 0f));
                mat.EnableKeyword("_EMISSION");
                renderer.material = mat;
            }

            // Random velocity
            Vector3 velocity = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(2f, 5f),
                Random.Range(-3f, 3f)
            );

            // Animate particle
            StartCoroutine(AnimateDeathParticle(particle, velocity));

            yield return new WaitForSeconds(0.02f);
        }
    }

    System.Collections.IEnumerator AnimateDeathParticle(GameObject particle, Vector3 velocity)
    {
        float lifetime = Random.Range(0.5f, 1f);
        float elapsed = 0f;
        float gravity = -15f;

        Renderer renderer = particle.GetComponent<Renderer>();
        Color startColor = renderer != null ? renderer.material.color : Color.red;

        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Apply velocity and gravity
            velocity.y += gravity * Time.deltaTime;
            particle.transform.position += velocity * Time.deltaTime;

            // Shrink and fade
            particle.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.02f, t);

            if (renderer != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                renderer.material.color = c;
            }

            yield return null;
        }

        if (particle != null)
            Destroy(particle);
    }

    // Public getters
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsDead() => currentState == State.Dead;

    // Gizmos
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Field of view
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + leftBoundary);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + rightBoundary);

        // Patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.3f);
            }
        }
    }
}