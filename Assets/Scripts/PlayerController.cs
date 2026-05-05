using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    // Components
    private CharacterController controller;
    private Camera playerCamera;

    // Movement variables
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;

    // Input variables
    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Validate ground check
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        HandleInput();
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleJump();
        ApplyGravity();

        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Lock cursor with left click
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Sprint input (only when moving forward and not crouching)
        isSprinting = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0 && !isCrouching;
    }

    void HandleMouseLook()
    {
        // Horizontal rotation (rotate the player body)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (rotate only the camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        // Calculate move direction
        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        moveDirection = moveDirection.normalized;

        // Determine current speed
        float currentSpeed = walkSpeed;
        if (isSprinting)
            currentSpeed = sprintSpeed;
        else if (isCrouching)
            currentSpeed = crouchSpeed;

        // Apply movement
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    void HandleCrouch()
    {
        // Toggle crouch with V or hold with Left Ctrl
        if (Input.GetKeyDown(KeyCode.V))
        {
            isCrouching = !isCrouching;
        }

        // Hold to crouch
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else if (!Input.GetKey(KeyCode.V) && Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
        }

        // Smoothly transition height
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        // Adjust camera position based on height
        Vector3 cameraTargetPos = playerCamera.transform.localPosition;
        cameraTargetPos.y = Mathf.Lerp(cameraTargetPos.y, (controller.height / 2f) - 0.1f, crouchTransitionSpeed * Time.deltaTime);
        playerCamera.transform.localPosition = cameraTargetPos;
    }

    void HandleJump()
    {
        // Jump only when grounded and not crouching
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            // Play jump sound
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayJump();
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Public methods for external scripts
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
    public bool IsSprinting() => isSprinting;
    public float GetCurrentSpeed()
    {
        if (isSprinting) return sprintSpeed;
        if (isCrouching) return crouchSpeed;
        return walkSpeed;
    }

    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}