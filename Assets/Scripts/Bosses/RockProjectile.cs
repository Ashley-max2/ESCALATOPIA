using System.Collections;
using UnityEngine;

/// <summary>
/// Proyectil de roca lanzado por BossRockThrower.
///
/// Al impactar al Player:
///   · Le aplica un impulso hacia abajo (lo hace caer)
///   · Le aplica un empuje horizontal hacia fuera
///   · Le ralentiza la velocidad durante unos segundos
///
/// Al impactar cualquier otro objeto:
///   · La roca se queda quieta y se destruye tras destroyDelay segundos.
///
/// Setup del Prefab:
///   · Rigidbody (useGravity = true  → la roca cae en arco)
///   · SphereCollider (no trigger)
///   · MeshRenderer con material de roca
///   · Este script (RockProjectile)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RockProjectile : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  PARÁMETROS (rellenados por BossRockThrower al instanciar)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Knockdown ──")]
    [Tooltip("Impulso hacia abajo aplicado al Rigidbody del player al ser golpeado.")]
    public float knockdownImpulse = 10f;

    [Tooltip("Fuerza de empuje horizontal al golpear al player.")]
    public float pushForce = 5f;

    [Header("── Slow ──")]
    [Tooltip("Multiplicador de velocidad durante el slow (0.35 = 35% de velocidad normal).")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.35f;

    [Tooltip("Duración del efecto de ralentización en segundos.")]
    public float slowDuration = 3f;

    [Header("── Proyectil ──")]
    [Tooltip("Segundos hasta auto-destruirse si no golpea nada.")]
    public float lifetime = 10f;

    [Tooltip("Segundos hasta destruirse tras colisionar.")]
    public float destroyDelay = 3f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool      _hasHit = false;
    private Rigidbody _rb;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb             = GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        _rb.useGravity  = true;

        // Desactivar el collider al nacer para no colisionar con el boss
        // al instanciarse. Se activa tras un pequeño delay.
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private void Start()
    {
        // Activar colisiones tras salir del boss y destruir si no golpea nada
        StartCoroutine(EnableColliderDelayed());
        Destroy(gameObject, lifetime);
    }

    private IEnumerator EnableColliderDelayed()
    {
        yield return new WaitForSeconds(0.15f);
        if (!_hasHit)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  COLISIÓN
    // ─────────────────────────────────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        // Ignorar otras rocas
        if (collision.gameObject.GetComponent<RockProjectile>() != null) return;
        if (collision.gameObject.GetComponent<BossRock>()       != null) return;

        _hasHit = true;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[RockProjectile] Impacto con el Player.");
            ApplyHitEffects(collision.gameObject);
        }
        else
        {
            Debug.Log($"[RockProjectile] Impacto con: {collision.gameObject.name}");
        }

        // Detener la roca y destruirla tras el delay
        FreezeRock();
        Destroy(gameObject, destroyDelay);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTOS AL GOLPEAR AL PLAYER
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyHitEffects(GameObject playerObj)
    {
        Rigidbody       playerRb = playerObj.GetComponent<Rigidbody>();
        PlayerStateMachine sm    = playerObj.GetComponent<PlayerStateMachine>();

        if (playerRb != null)
        {
            // 1. Empuje horizontal hacia fuera desde la roca
            Vector3 pushDir = playerObj.transform.position - transform.position;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude < 0.01f) pushDir = playerObj.transform.forward;
            pushDir.Normalize();

            playerRb.AddForce(pushDir * pushForce, ForceMode.Impulse);

            // 2. Impulso hacia abajo — lo hace caer
            playerRb.AddForce(Vector3.down * knockdownImpulse, ForceMode.Impulse);
        }

        // 3. Ralentización de velocidad
        // La corrutina se lanza desde el PlayerStateMachine para que no muera
        // cuando la roca se destruya.
        if (sm != null)
        {
            sm.StartCoroutine(SlowPlayerCoroutine(sm));
        }
    }

    /// <summary>
    /// Guarda las velocidades del player (caminar, correr, escalar, gancho),
    /// aplica el slow a todas y las restaura al acabar.
    /// </summary>
    private IEnumerator SlowPlayerCoroutine(PlayerStateMachine sm)
    {
        // Guardar velocidades originales
        float origWalk  = sm.WalkSpeed;
        float origRun   = sm.RunSpeed;
        float origClimb = sm.ClimbSpeed;

        HookSystem hook = sm.GetComponent<HookSystem>();
        float origHook  = hook != null ? hook.HookSpeed : 0f;

        // Aplicar slow a todas
        sm.SetWalkSpeed (origWalk  * slowMultiplier);
        sm.SetRunSpeed  (origRun   * slowMultiplier);
        sm.SetClimbSpeed(origClimb * slowMultiplier);
        if (hook != null) hook.SetHookSpeed(origHook * slowMultiplier);

        yield return new WaitForSeconds(slowDuration);

        // Restaurar solo si el player sigue activo
        if (sm != null && sm.isActiveAndEnabled)
        {
            sm.SetWalkSpeed (origWalk);
            sm.SetRunSpeed  (origRun);
            sm.SetClimbSpeed(origClimb);
            if (hook != null) hook.SetHookSpeed(origHook);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────────────────────────────────
    private void FreezeRock()
    {
        // Desactivar colisiones para evitar re-triggers
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Detener física
        if (_rb != null)
        {
            _rb.velocity        = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic     = true;
        }
    }
}
