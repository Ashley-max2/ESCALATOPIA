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
    private HookedFreeAnimController animController;

    // Impulso variables
    private bool isImpulsing = false;
    private float impulseTimer = 0f;
    
    // Nuevo: Timer de seguridad para evitar enganche permanente
    private float safetyTimer = 0f;
    private const float MAX_HOOK_TIME = 3f;

    void Start()
    {
        hookSystem = GetComponent<HookSystem>();
        playerTransform = hookSystem.playerTransform;
        playerRb = hookSystem.PlayerRigidbody;
        animController = GetComponent<HookedFreeAnimController>();
    }

    public void LaunchHook()
    {
        if (hookSystem.TargetFinder.CurrentTarget != null)
        {
            hookTarget = hookSystem.TargetFinder.CurrentTarget.HookPoint;
            isTraveling = true;
            safetyTimer = MAX_HOOK_TIME; // Iniciar timer de seguridad
        }
    }

    public void UpdateHookTravel()
    {
        if (!isTraveling || playerRb == null) return;

        Vector3 direction = (hookTarget - playerTransform.position).normalized;
        playerRb.velocity = direction * hookSpeed;
        
        // Actualizar timer de seguridad
        safetyTimer -= Time.deltaTime;
        if (safetyTimer <= 0f)
        {
            CancelHook();
        }
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
        safetyTimer = MAX_HOOK_TIME; // Reiniciar timer de seguridad
        
        // Forzar alineación con la dirección del gancho
        Vector3 directionToHook = (hookTarget - playerTransform.position).normalized;
        directionToHook.y = 0; // Solo rotación horizontal
        
        if (directionToHook != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.LookRotation(directionToHook);
        }
    }

    public void UpdateImpulsePull()
    {
        if (!isImpulsing || playerRb == null || hookSystem.CurrentHookPoint == null || impulseConfig == null)
            return;

        // Verificar timer de seguridad
        safetyTimer -= Time.deltaTime;
        if (safetyTimer <= 0f)
        {
            CancelHook();
            return;
        }

        Vector3 targetPos = hookSystem.CurrentHookPoint.HookPoint;
        Vector3 currentPos = playerTransform.position;

        Vector3 dir = (targetPos - currentPos).normalized;
        float dist = Vector3.Distance(currentPos, targetPos);

        // Si llega muy cerca del objetivo, terminar el impulso suavemente
        if (dist <= impulseConfig.stopDistance)
        {
            isImpulsing = false;
            Debug.Log("[HOOK] Llegado al objetivo, terminando impulso. isImpulsing = false");
            
            // Reducir velocidad gradualmente en lugar de detenerla bruscamente
            if (playerRb != null)
            {
                playerRb.velocity *= 0.3f; // Reducir a 30% de la velocidad actual
                playerRb.useGravity = true;
                playerRb.freezeRotation = true;
            }
            
            // Cancelar el gancho completamente
            CancelHook();
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
            Debug.Log("[HOOK] Impulso terminado por timer, IsImpulsing = false, restaurando gravedad");
            // Al terminar el impulso: restaurar gravedad y rotación congelada
            if (playerRb != null)
            {
                playerRb.useGravity = true;
                playerRb.freezeRotation = true;
            }
            // Cancelar el gancho completamente
            CancelHook();
        }
    }

    public bool IsImpulsing => isImpulsing;

    // DESACTIVADO: No permitir movimiento durante el gancho
    /*public void ApplyHookMovement()
    {
        if (hookSystem.CurrentHookPoint == null || playerRb == null) return;

        Vector3 hookDirection = (hookSystem.CurrentHookPoint.HookPoint - playerTransform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(hookDirection, Vector3.up);

        float horizontalInput = Input.GetAxis("Horizontal");
        playerRb.AddForce(perpendicular * horizontalInput * swingForce, ForceMode.Acceleration);
        playerRb.AddForce(hookDirection * swingForce * 0.5f, ForceMode.Acceleration);
    }*/

    public void CancelHook()
    {
        Debug.Log($"[HOOK] CancelHook() llamado - isImpulsing: {isImpulsing} -> false");
        isTraveling = false;
        hookSystem.CurrentHookPoint = null;
        isImpulsing = false;
        
        // Asegurar que la gravedad y rotación se restauran
        if (playerRb != null)
        {
            playerRb.useGravity = true;
            playerRb.freezeRotation = true;
            Debug.Log("[HOOK] Gravedad y rotación restauradas en CancelHook()");
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
            // Terminar el arrastre, restaurar gravedad y cancelar gancho
            if (playerRb != null)
            {
                playerRb.useGravity = true;
                playerRb.freezeRotation = true;
            }
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
    
    public Vector3 GetCurrentTargetPosition()
    {
        return hookTarget;
    }
}