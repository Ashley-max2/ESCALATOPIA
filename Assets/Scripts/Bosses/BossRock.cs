using System.Collections;
using UnityEngine;

/// <summary>
/// Proyectil de roca lanzado por BossBowler.
/// Al impactar al player:
///   · Lo empuja 1 metro hacia abajo (knockdown)
///   · Le aplica una ralentización durante unos segundos
/// Todos los valores son editables en el Inspector o configurables por BossBowler.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossRock : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  PARÁMETROS EDITABLES (rellenados desde BossBowler al instanciar)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Knockdown ──")]
    [Tooltip("Metros que cae el player al ser golpeado.")]
    public float knockdownDistance = 1f;

    [Tooltip("Fuerza de impulso hacia abajo aplicada al Rigidbody del player.")]
    public float knockdownImpulse = 8f;

    [Header("── Slow Effect ──")]
    [Tooltip("Multiplicador de velocidad mientras dure el slow (0.3 = 30% de la velocidad normal).")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.35f;

    [Tooltip("Duración en segundos del efecto de ralentización.")]
    public float slowDuration = 3f;

    [Header("── Proyectil ──")]
    [Tooltip("Segundos antes de auto-destruirse si no golpea nada.")]
    public float lifetime = 15f;

    [Tooltip("Activa/desactiva las trazas de movimiento si el objeto tiene TrailRenderer.")]
    public bool enableTrail = true;

[Tooltip("Fuerza de empuje aplicada al player en horizontal cuando es golpeado (metros aproximados).")]
public float pushForce = 6f;

[Tooltip("Segundos que pasan desde la colisión hasta destruir la roca.")]
public float collisionDestroyDelay = 5f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool _hasHit = false;
    private Rigidbody _rb;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Desactivar gravedad para vuelo recto → rango infinito
        _rb.useGravity = false;
        // No auto-destruir: rocas tienen alcance infinito hasta colisión. Se destruirán tras colisión en DestroyAfterDelayCoroutine.
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  COLISIÓN
    // ─────────────────────────────────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        Debug.Log($"[BossRock] Colisión con: {collision.gameObject.name} (tag: {collision.gameObject.tag})");

        // Ignorar colisiones con otras rocas
        if (collision.gameObject.GetComponent<BossRock>() != null)
        {
            Debug.Log("[BossRock] Colisión con otra roca, ignorada.");
            return;
        }

        _hasHit = true;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[BossRock] ¡Golpeó al Player!");
            ApplyHitEffects(collision.gameObject);
        }
        else
        {
            Debug.Log($"[BossRock] Colisión con obstáculo: {collision.gameObject.name}");
        }

        // Desactivar collider y física para que la roca quede quieta
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        // Destruir la roca pasado un tiempo fijo tras la colisión
        StartCoroutine(DestroyAfterDelayCoroutine(collisionDestroyDelay));
    }

    private IEnumerator DestroyAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTOS AL GOLPEAR AL PLAYER
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyHitEffects(GameObject playerObj)
    {
        var sm  = playerObj.GetComponent<PlayerStateMachine>();
        var rb  = playerObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Empuje horizontal hacia fuera desde la roca (unos metros)
            Vector3 pushDir = playerObj.transform.position - transform.position;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude < 0.01f)
                pushDir = playerObj.transform.forward;
            pushDir.Normalize();
            rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }

        if (sm != null)
        {
            sm.StartCoroutine(SlowPlayerCoroutine(sm));
        }
    }

    /// <summary>
    /// Guarda las velocidades originales, aplica el slow y las restaura al acabar.
    /// </summary>
    private IEnumerator SlowPlayerCoroutine(PlayerStateMachine sm)
    {
        float origWalk = sm.WalkSpeed;
        float origRun  = sm.RunSpeed;

        sm.SetWalkSpeed(origWalk * slowMultiplier);
        sm.SetRunSpeed (origRun  * slowMultiplier);

        yield return new WaitForSeconds(slowDuration);

        // Restaurar solo si el player sigue vivo y el componente activo
        if (sm != null && sm.isActiveAndEnabled)
        {
            sm.SetWalkSpeed(origWalk);
            sm.SetRunSpeed (origRun);
        }
    }
}
