using UnityEngine;

/// <summary>
/// Handles player climbing detection and mechanics.
/// Single Responsibility: Climbing logic.
/// Implements IClimbingController interface.
/// </summary>
public class PlayerClimbingComponent : MonoBehaviour, IClimbingController
{
    [Header("Climbing Settings")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private Vector3 triggerOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float triggerRadius = 0.8f;
    [SerializeField] private float reentryDelay = 0.5f;

    private bool inClimbableZone = false;
    private SphereCollider climbTrigger;
    private float reentryTimer = 0f;
    private bool canReenter = true;
    private Vector3 climbingSurfaceNormal = Vector3.forward;

    // IClimbingController implementation
    public bool IsInClimbableZone => inClimbableZone;
    public Vector3 ClimbingSurfaceNormal => climbingSurfaceNormal;

    private void Awake()
    {
        SetupClimbingTrigger();
    }

    private void Update()
    {
        UpdateReentryTimer();
    }

    /// <summary>
    /// Setup the climbing detection trigger.
    /// </summary>
    private void SetupClimbingTrigger()
    {
        // Create trigger GameObject
        GameObject triggerObj = new GameObject("ClimbingTrigger");
        triggerObj.transform.SetParent(transform);
        triggerObj.transform.localPosition = triggerOffset;
        triggerObj.tag = "Player";

        // Add sphere collider as trigger
        climbTrigger = triggerObj.AddComponent<SphereCollider>();
        climbTrigger.isTrigger = true;
        climbTrigger.radius = triggerRadius;

        // Add rigidbody (required for triggers)
        Rigidbody triggerRb = triggerObj.AddComponent<Rigidbody>();
        triggerRb.isKinematic = true;
        triggerRb.useGravity = false;
    }

    /// <summary>
    /// Update the reentry timer.
    /// </summary>
    private void UpdateReentryTimer()
    {
        if (reentryTimer > 0f)
        {
            reentryTimer -= Time.deltaTime;
            canReenter = false;
        }
        else
        {
            canReenter = true;
        }
    }

    /// <summary>
    /// Check if climbing can be started.
    /// </summary>
    public bool CanStartClimbing()
    {
        // Get stamina component
        IStaminaUser stamina = GetComponent<IStaminaUser>();
        
        return inClimbableZone && 
               canReenter && 
               (stamina == null || stamina.HasStamina(1f));
    }

    /// <summary>
    /// Perform climbing movement.
    /// </summary>
    public void Climb(Vector3 direction, float speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;

        // Override gravity during climbing
        rb.useGravity = false;

        // Apply climbing movement
        Vector3 climbVelocity = direction.normalized * speed;
        rb.velocity = climbVelocity;
    }

    /// <summary>
    /// Enter a climbable zone (called by trigger).
    /// </summary>
    public void EnterClimbableZone()
    {
        inClimbableZone = true;
        Debug.Log("[Climbing] Entered climbable zone");
    }

    /// <summary>
    /// Exit a climbable zone (called by trigger).
    /// </summary>
    public void ExitClimbableZone()
    {
        inClimbableZone = false;
        Debug.Log("[Climbing] Exited climbable zone");
    }

    /// <summary>
    /// Start the reentry delay timer.
    /// Called after wall jump to prevent immediate re-climbing.
    /// </summary>
    public void StartReentryDelay()
    {
        reentryTimer = reentryDelay;
        canReenter = false;
    }

    /// <summary>
    /// Detect climbable surface normal.
    /// </summary>
    public void DetectClimbingSurface(Collider climbableCollider)
    {
        if (climbableCollider == null)
            return;

        // Raycast to get surface normal
        RaycastHit hit;
        Vector3 direction = climbableCollider.transform.position - transform.position;
        
        if (Physics.Raycast(transform.position, direction, out hit, 2f))
        {
            climbingSurfaceNormal = hit.normal;
        }
    }

    /// <summary>
    /// Enable gravity (called when exiting climbing state).
    /// </summary>
    public void EnableGravity()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
        }
    }

    // Trigger detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            EnterClimbableZone();
            DetectClimbingSurface(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            ExitClimbableZone();
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (climbTrigger != null)
        {
            Gizmos.color = inClimbableZone ? Color.cyan : Color.blue;
            Gizmos.DrawWireSphere(climbTrigger.transform.position, climbTrigger.radius);
        }
    }
}
