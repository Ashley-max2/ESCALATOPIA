using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookMovementController : MonoBehaviour
{
    [Header("Hook Movement")]
    public float hookSpeed = 15f;
    public float swingForce = 10f;
    public float playerSpeedDuringHook = 5f;
    public float autoPullSpeed = 20f;
    public float aimPullSpeed = 8f;

    [Header("Impulse Settings")]
    public float attachImpulseSpeed = 30f;     // Velocidad alta inicial (ajústalo a tu gusto)
    public float impulseDuration = 0.25f;      // Duración en segundos del tirón (ajusta también)

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
            playerRb.useGravity = false; // parar caída mientras es arrastrado
        }
        // INICIA EL IMPULSO al enganchar
        StartImpulsePull();
    }

    // IMPULSE/TIRÓN: inicia el impulso
    public void StartImpulsePull()
    {
        isImpulsing = true;
        impulseTimer = impulseDuration;
    }

    // Actualiza el impulso si está activo
    public void UpdateImpulsePull()
    {
        if (!isImpulsing || playerRb == null || hookSystem.CurrentHookPoint == null) return;

        Vector3 dir = (hookSystem.CurrentHookPoint.HookPoint - playerTransform.position).normalized;

        playerRb.useGravity = false;
        playerRb.velocity = dir * attachImpulseSpeed;

        impulseTimer -= Time.deltaTime;
        // Si termina el tiempo o el jugador llega cerca del punto del gancho, detén el impulso.
        if (impulseTimer <= 0f || Vector3.Distance(playerTransform.position, hookSystem.CurrentHookPoint.HookPoint) < 1.5f)
        {
            isImpulsing = false;
        }
    }

    // ¿Está en tirón/impulso?
    public bool IsImpulsing => isImpulsing;

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
        isImpulsing = false;

        // Recuperar físicas normales
        if (playerRb != null)
        {
            playerRb.useGravity = true;
            playerRb.velocity = Vector3.zero;
        }
    }

    public void PullPlayerToHook()
    {
        if (hookSystem.CurrentHookPoint == null || playerRb == null) return;

        Vector3 hookPoint = hookSystem.CurrentHookPoint.HookPoint;
        Vector3 dir = (hookPoint - playerTransform.position).normalized;

        playerRb.useGravity = false;
        playerRb.velocity = dir * autoPullSpeed;

        if (Vector3.Distance(playerTransform.position, hookPoint) < 2f)
        {
            hookSystem.CancelHook();
        }
    }

    public void PullPlayerTowardsAim()
    {
        if (playerRb == null) return;

        // Dirección exacta de tu mira (desde el transform de la cámara)
        Vector3 aimDirection = hookSystem.playerCamera.transform.forward;
        playerRb.useGravity = false;
        playerRb.velocity = aimDirection * aimPullSpeed;
    }
}