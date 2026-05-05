using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.35f;
    [SerializeField] private float crouchStepInterval = 0.7f;

    private PlayerController playerController;
    private CharacterController characterController;
    private float stepTimer;
    private bool wasGrounded;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerController == null || characterController == null) return;
        if (SoundManager.Instance == null) return;

        bool isGrounded = playerController.IsGrounded();
        bool isMoving = characterController.velocity.magnitude > 0.5f;

        // Landing sound
        if (isGrounded && !wasGrounded)
        {
            SoundManager.Instance.PlayLand();
        }
        wasGrounded = isGrounded;

        // Footsteps
        if (isGrounded && isMoving)
        {
            float interval = walkStepInterval;

            if (playerController.IsSprinting())
                interval = sprintStepInterval;
            else if (playerController.IsCrouching())
                interval = crouchStepInterval;

            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                SoundManager.Instance.PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
}