using UnityEngine;

/// <summary>
/// Handles player ground movement and rotation.
/// Single Responsibility: Movement logic.
/// Implements IMovementController interface.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementComponent : MonoBehaviour, IMovementController
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSmoothness = 10f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;

    private Rigidbody rb;
    private Transform cameraTransform;

    // IMovementController implementation
    public bool IsGrounded => CheckGrounded();
    public Vector3 Velocity => rb.velocity;
    public Rigidbody Rigidbody => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        // Create ground check point if not assigned
        if (groundCheckPoint == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheckPoint = gc.transform;
        }
    }

    /// <summary>
    /// Move the player in a given direction.
    /// </summary>
    public void Move(Vector3 direction, float speed)
    {
        if (!IsGrounded)
            return;

        // Calculate target velocity
        Vector3 targetVelocity = direction.normalized * speed;

        // Apply movement using physics
        Vector3 currentVelocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 velocityChange = targetVelocity - currentVelocityXZ;

        rb.AddForce(velocityChange * 10f, ForceMode.Acceleration);
    }

    /// <summary>
    /// Move the player based on camera-relative input.
    /// </summary>
    public void MoveRelativeToCamera(Vector2 input, bool isRunning)
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Calculate camera-relative direction
        Vector3 direction = (cameraTransform.right * input.x + cameraTransform.forward * input.y);
        direction.y = 0;
        direction.Normalize();

        // Determine speed
        float speed = isRunning ? runSpeed : walkSpeed;

        // Move
        Move(direction, speed);

        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Rotate(direction, rotationSmoothness);
        }
    }

    /// <summary>
    /// Rotate the player to face a direction.
    /// </summary>
    public void Rotate(Vector3 direction, float smoothness)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothness * Time.deltaTime);
    }

    /// <summary>
    /// Check if the player is grounded.
    /// </summary>
    private bool CheckGrounded()
    {
        if (groundCheckPoint == null)
            return false;

        bool grounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // Debug visualization
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckRadius,
                     grounded ? Color.green : Color.red);

        return grounded;
    }

    /// <summary>
    /// Apply a jump force.
    /// </summary>
    public void ApplyJumpForce(float jumpForce)
    {
        if (!IsGrounded)
            return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Apply a wall jump force.
    /// </summary>
    public void ApplyWallJumpForce(float upwardForce, float lateralForce, Vector3 wallNormal)
    {
        Vector3 jumpDirection = (Vector3.up * upwardForce + wallNormal * lateralForce).normalized;
        rb.velocity = new Vector3(rb.velocity.x * 0.5f, 0, rb.velocity.z * 0.5f); // Reset velocity
        rb.AddForce(jumpDirection * (upwardForce + lateralForce), ForceMode.Impulse);
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
