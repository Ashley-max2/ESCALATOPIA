using UnityEngine;

/// <summary>
/// Boss movement component implementing IBossMovement.
/// Single Responsibility: Boss movement logic only.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossMovementComponent : MonoBehaviour, IBossMovement
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private LayerMask climbableLayer;

    private Rigidbody rb;
    private bool isMoving = false;
    private bool isClimbing = false;
    private bool isHooking = false;

    // IBossMovement implementation
    public bool IsOnClimbableWall => CheckClimbableWall();
    public bool IsMoving => isMoving;
    public bool IsClimbing => isClimbing;
    public bool IsHooking => isHooking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Move towards a target position.
    /// </summary>
    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on same vertical level for ground movement

        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
        isMoving = true;
        isClimbing = false;

        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Climb vertically.
    /// </summary>
    public void Climb(Vector3 direction, float speed)
    {
        if (!IsOnClimbableWall)
        {
            Debug.LogWarning("[BossMovement] Cannot climb - not on climbable wall");
            return;
        }

        rb.useGravity = false;
        rb.velocity = direction.normalized * speed;
        isClimbing = true;
        isMoving = false;
    }

    /// <summary>
    /// Launch hook towards a target.
    /// </summary>
    public void LaunchHook(IHookable hookPoint)
    {
        if (hookPoint == null)
        {
            Debug.LogWarning("[BossMovement] Cannot launch hook - no hook point");
            return;
        }

        // Boss hook logic would go here
        // For now, just set the flag
        isHooking = true;
        isMoving = false;
        isClimbing = false;

        Debug.Log($"[BossMovement] Launching hook to {hookPoint.HookPoint}");
    }

    /// <summary>
    /// Stop all movement.
    /// </summary>
    public void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.useGravity = true;
        isMoving = false;
        isClimbing = false;
        isHooking = false;
    }

    /// <summary>
    /// Check if on a climbable wall.
    /// </summary>
    private bool CheckClimbableWall()
    {
        // Raycast forward to detect climbable wall
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, 1.5f, climbableLayer))
        {
            return hit.collider.CompareTag("escalable");
        }

        return false;
    }

    /// <summary>
    /// Get distance to target.
    /// </summary>
    public float GetDistanceToTarget(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    /// <summary>
    /// Check if reached target position.
    /// </summary>
    public bool HasReachedTarget(Vector3 targetPosition, float threshold = 1f)
    {
        return GetDistanceToTarget(targetPosition) <= threshold;
    }
}
