using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookMovementController : MonoBehaviour
{
    [Header("Hook Movement")]
    public float hookSpeed = 15f;
    public float swingForce = 10f;
    public float playerSpeedDuringHook = 5f;

    private Rigidbody playerRb;
    private Vector3 hookTarget;
    private bool isTraveling;
    private HookSystem hookSystem;
    private Transform playerTransform;

    void Start()
    {
        hookSystem = GetComponent<HookSystem>();
        playerTransform = hookSystem.playerTransform;
        playerRb = hookSystem.PlayerRigidbody;
    }

    public void LaunchHook()
    {
        if (hookSystem.TargetFinder.CurrentTarget != null)
        {
            hookTarget = hookSystem.TargetFinder.CurrentTarget.HookPoint;
            isTraveling = true;
        }
    }

    public void UpdateHookTravel()
    {
        if (!isTraveling || playerRb == null) return;

        Vector3 direction = (hookTarget - playerTransform.position).normalized;
        playerRb.velocity = direction * hookSpeed;
    }

    public bool HasReachedTarget()
    {
        if (!isTraveling) return false;

        float distance = Vector3.Distance(playerTransform.position, hookTarget);
        if (distance < 1f)
        {
            isTraveling = false;
            hookSystem.CurrentHookPoint = hookSystem.TargetFinder.CurrentTarget;
            return true;
        }
        return false;
    }

    public void OnHookAttached()
    {
        if (playerRb != null)
            playerRb.velocity = Vector3.zero;
    }

    public void ApplyHookMovement()
    {
        if (hookSystem.CurrentHookPoint == null || playerRb == null) return;

        Vector3 hookDirection = (hookSystem.CurrentHookPoint.HookPoint - playerTransform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(hookDirection, Vector3.up);

        float horizontalInput = Input.GetAxis("Horizontal");
        playerRb.AddForce(perpendicular * horizontalInput * swingForce);
        playerRb.AddForce(hookDirection * swingForce * 0.5f);
    }

    public void CancelHook()
    {
        isTraveling = false;
        hookSystem.CurrentHookPoint = null;
    }
}