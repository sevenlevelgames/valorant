using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.005f;
    [SerializeField] private float maxSway = 0.015f;
    [SerializeField] private float smoothness = 8f;

    [Header("Rotation Sway")]
    [SerializeField] private float rotationSwayAmount = 1f;
    [SerializeField] private float maxRotationSway = 2f;

    [Header("Movement Bob")]
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobHorizontalAmplitude = 0.002f;
    [SerializeField] private float bobVerticalAmplitude = 0.003f;

    // Initial position and rotation
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float bobTimer;

    // References
    private PlayerController playerController;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        HandleSway();
        HandleBob();
    }

    void HandleSway()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Calculate position sway
        float swayX = Mathf.Clamp(-mouseX * swayAmount, -maxSway, maxSway);
        float swayY = Mathf.Clamp(-mouseY * swayAmount, -maxSway, maxSway);

        Vector3 targetPosition = new Vector3(swayX, swayY, 0) + initialPosition;

        // Calculate rotation sway
        float rotX = Mathf.Clamp(-mouseY * rotationSwayAmount, -maxRotationSway, maxRotationSway);
        float rotY = Mathf.Clamp(mouseX * rotationSwayAmount, -maxRotationSway, maxRotationSway);

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, 0) * initialRotation;

        // Apply smooth sway
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothness * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothness * Time.deltaTime);
    }

    void HandleBob()
    {
        if (playerController == null) return;

        // Check if player is moving on ground
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f) && playerController.IsGrounded();

        if (isMoving)
        {
            // Increase bob timer
            float speedMultiplier = playerController.IsSprinting() ? 1.2f : 1f;
            bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;

            // Calculate bob offset
            float bobX = Mathf.Cos(bobTimer) * bobHorizontalAmplitude * speedMultiplier;
            float bobY = Mathf.Abs(Mathf.Sin(bobTimer)) * bobVerticalAmplitude * speedMultiplier;

            // Apply bob
            Vector3 bobOffset = new Vector3(bobX, bobY, 0);
            transform.localPosition += bobOffset;
        }
        else
        {
            // Reset timer when not moving
            bobTimer = 0;
        }
    }

    // Call this when ADS to reduce sway
    public void SetAiming(bool aiming)
    {
        if (aiming)
        {
            swayAmount *= 0.3f;
            bobHorizontalAmplitude *= 0.3f;
            bobVerticalAmplitude *= 0.3f;
        }
        else
        {
            swayAmount /= 0.3f;
            bobHorizontalAmplitude /= 0.3f;
            bobVerticalAmplitude /= 0.3f;
        }
    }
}