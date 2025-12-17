using UnityEngine;

/// <summary>
/// Refactored HookMovementController implementing IHookPhysicsService.
/// Single Responsibility: Hook physics calculations and player impulse.
/// </summary>
public class HookPhysicsService : MonoBehaviour, IHookPhysicsService
{
    [Header("Hook Physics Settings")]
    [SerializeField] private float hookSpeed = 15f;
    [SerializeField] private float impulseSpeed = 20f;
    [SerializeField] private float impulseDuration = 1.5f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float maxHookTime = 3f;

    [Header("Impulse Configuration")]
    [SerializeField] private HookImpulseConfig impulseConfig;

    private bool isImpulsing = false;
    private float impulseTimer = 0f;
    private float safetyTimer = 0f;
    private Vector3 hookTargetPosition;

    // IHookPhysicsService implementation
    public bool IsImpulsing => isImpulsing;

    /// <summary>
    /// Calculate the impulse force for the hook.
    /// </summary>
    public Vector3 CalculateImpulseForce(Vector3 playerPosition, Vector3 hookPosition, Rigidbody playerRigidbody)
    {
        Vector3 direction = (hookPosition - playerPosition).normalized;
        float distance = Vector3.Distance(playerPosition, hookPosition);

        // Calculate force based on distance (stronger when farther)
        float forceMagnitude = Mathf.Lerp(impulseSpeed * 0.5f, impulseSpeed, distance / 10f);

        return direction * forceMagnitude;
    }

    /// <summary>
    /// Apply impulse to the player towards the hook point.
    /// </summary>
    public void ApplyImpulse(Rigidbody playerRigidbody, Vector3 hookPosition)
    {
        if (playerRigidbody == null)
            return;

        hookTargetPosition = hookPosition;
        isImpulsing = true;
        impulseTimer = impulseConfig != null ? impulseConfig.impulseDuration : impulseDuration;
        safetyTimer = maxHookTime;

        // Disable gravity during impulse
        playerRigidbody.useGravity = false;

        // Apply initial impulse
        Vector3 impulseForce = CalculateImpulseForce(playerRigidbody.position, hookPosition, playerRigidbody);
        
        if (impulseConfig != null && impulseConfig.useVelocity)
        {
            playerRigidbody.velocity = impulseForce;
        }
        else
        {
            playerRigidbody.AddForce(impulseForce, ForceMode.Impulse);
        }

        Debug.Log($"[HookPhysics] Impulse started - Duration: {impulseTimer}s");
    }

    /// <summary>
    /// Update the hook impulse (called each frame while impulsing).
    /// </summary>
    public void UpdateImpulse(Vector3 playerPosition, Vector3 hookPosition, Rigidbody playerRigidbody)
    {
        if (!isImpulsing || playerRigidbody == null)
            return;

        // Update timers
        impulseTimer -= Time.deltaTime;
        safetyTimer -= Time.deltaTime;

        // Safety check - force stop if too long
        if (safetyTimer <= 0f)
        {
            Debug.LogWarning("[HookPhysics] Safety timer expired, stopping impulse");
            StopImpulse();
            return;
        }

        // Check if reached destination
        if (HasReachedHookPoint(playerPosition, hookPosition))
        {
            Debug.Log("[HookPhysics] Reached hook point");
            StopImpulse();
            return;
        }

        // Check if timer expired
        if (impulseTimer <= 0f)
        {
            Debug.Log("[HookPhysics] Impulse timer expired");
            StopImpulse();
            return;
        }

        // Continue applying force towards hook point
        Vector3 direction = (hookPosition - playerPosition).normalized;
        
        if (impulseConfig != null)
        {
            if (impulseConfig.useVelocity)
            {
                playerRigidbody.velocity = direction * impulseConfig.initialImpulseSpeed;
            }
            else
            {
                playerRigidbody.AddForce(direction * impulseConfig.forceMultiplier, ForceMode.Acceleration);
            }
        }
        else
        {
            playerRigidbody.AddForce(direction * impulseSpeed, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Stop the impulse.
    /// </summary>
    public void StopImpulse()
    {
        if (!isImpulsing)
            return;

        isImpulsing = false;
        impulseTimer = 0f;
        safetyTimer = 0f;

        Debug.Log("[HookPhysics] Impulse stopped");
    }

    /// <summary>
    /// Check if the player has reached the hook point.
    /// </summary>
    public bool HasReachedHookPoint(Vector3 playerPosition, Vector3 hookPosition)
    {
        float distance = Vector3.Distance(playerPosition, hookPosition);
        float threshold = impulseConfig != null ? impulseConfig.stopDistance : stopDistance;
        
        return distance <= threshold;
    }

    /// <summary>
    /// Reset the physics service state.
    /// </summary>
    public void Reset()
    {
        StopImpulse();
        hookTargetPosition = Vector3.zero;
    }
}
