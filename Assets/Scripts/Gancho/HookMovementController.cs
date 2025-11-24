using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookMovementController : MonoBehaviour
{
    [Header("Hook Movement")]
    public float hookSpeed = 15f;
    public float swingForce = 10f;
    public float playerSpeedDuringHook = 5f;
    public float aimPullSpeed = 8f;

    [Header("Impulse Config (ScriptableObject)")]
    public HookImpulseConfig impulseConfig;

    private Rigidbody playerRb;
    private Vector3 hookTarget;
    private bool isTraveling;
    private HookSystem hookSystem;
    private Transform playerTransform;

    // Impulso variables
    private bool isImpulsing = false;
    private float impulseTimer = 0f;

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
        {
            playerRb.velocity = Vector3.zero;
            playerRb.useGravity = false;
        }

        StartImpulsePull();
    }

    public void StartImpulsePull()
    {
        if (impulseConfig == null)
        {
            Debug.LogWarning("HookImpulseConfig no asignado en HookMovementController.");
            return;
        }

        isImpulsing = true;
        impulseTimer = impulseConfig.impulseDuration;
    }

    public void UpdateImpulsePull()
    {
        if (!isImpulsing || playerRb == null || hookSystem.CurrentHookPoint == null || impulseConfig == null)
            return;

        Vector3 targetPos = hookSystem.CurrentHookPoint.HookPoint;
        Vector3 currentPos = playerTransform.position;

        Vector3 dir = (targetPos - currentPos).normalized;
        float dist = Vector3.Distance(currentPos, targetPos);

        if (dist <= impulseConfig.stopDistance)
        {
            StopAllHookMotion();
            hookSystem.CancelHook();
            return;
        }

        playerRb.useGravity = false;

        if (impulseConfig.useVelocity)
        {
            playerRb.velocity = dir * impulseConfig.initialImpulseSpeed;
        }
        else
        {
            playerRb.AddForce(dir * impulseConfig.forceMultiplier, ForceMode.Acceleration);
        }

        impulseTimer -= Time.deltaTime;
        if (impulseTimer <= 0f)
        {
            isImpulsing = false;
        }
    }

    public bool IsImpulsing => isImpulsing;

    public void ApplyHookMovement()
    {
        if (hookSystem.CurrentHookPoint == null || playerRb == null) return;

        Vector3 hookDirection = (hookSystem.CurrentHookPoint.HookPoint - playerTransform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(hookDirection, Vector3.up);

        float horizontalInput = Input.GetAxis("Horizontal");
        playerRb.AddForce(perpendicular * horizontalInput * swingForce, ForceMode.Acceleration);
        playerRb.AddForce(hookDirection * swingForce * 0.5f, ForceMode.Acceleration);
    }

    public void CancelHook()
    {
        isTraveling = false;
        hookSystem.CurrentHookPoint = null;
        isImpulsing = false;

        StopAllHookMotion();
    }

    private void StopAllHookMotion()
    {
        if (playerRb != null)
        {
            if (impulseConfig != null && impulseConfig.enableGravityOnStop)
                playerRb.useGravity = true;
            else
                playerRb.useGravity = false;
        }
    }

    public void PullPlayerToHook()
    {
        if (hookSystem.CurrentHookPoint == null || playerRb == null || impulseConfig == null) return;
        if (!impulseConfig.continuePullAfterImpulse) return;
        if (isImpulsing) return;

        Vector3 targetPos = hookSystem.CurrentHookPoint.HookPoint;
        Vector3 currentPos = playerTransform.position;

        Vector3 dir = (targetPos - currentPos).normalized;
        float dist = Vector3.Distance(currentPos, targetPos);

        if (dist <= impulseConfig.stopDistance)
        {
            StopAllHookMotion();
            hookSystem.CancelHook();
            return;
        }

        playerRb.useGravity = false;
        playerRb.velocity = dir * impulseConfig.pullSpeed;
    }

    public void PullPlayerTowardsAim()
    {
        if (playerRb == null) return;

        Vector3 aimDirection = hookSystem.playerCamera.transform.forward;
        playerRb.useGravity = false;
        playerRb.velocity = aimDirection * aimPullSpeed;
    }
}