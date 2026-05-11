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

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool _hasHit = false;
    private Rigidbody _rb;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  COLISIÓN
    // ─────────────────────────────────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        Debug.Log($"[BossRock] Colisión con: {collision.gameObject.name} (tag: {collision.gameObject.tag})");

        if (collision.gameObject.CompareTag("Player"))
        {
            _hasHit = true;
            Debug.Log("[BossRock] ¡Golpeó al Player!");
            ApplyHitEffects(collision.gameObject);
            Destroy(gameObject);
        }
        else if (collision.gameObject.GetComponent<BossRock>() != null)
        {
            // Colisión con otra roca — ignorar completamente
            Debug.Log("[BossRock] Colisión con otra roca, ignorada.");
            return;
        }
        else if (collision.gameObject.CompareTag("BossRockIgnore"))
        {
            // Ignorar, seguir volando
            Debug.Log("[BossRock] Colisión ignorada (BossRockIgnore)");
        }
        else
        {
            // Golpea el suelo u otro objeto — destruir sin efecto
            _hasHit = true;
            Debug.Log($"[BossRock] Colisión con obstáculo, destruyendo...");
            Destroy(gameObject);
        }
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
            // Cancelar velocidad vertical actual y aplicar impulso hacia abajo
            Vector3 vel = rb.velocity;
            vel.y = 0f;
            rb.velocity = vel;
            rb.AddForce(Vector3.down * knockdownImpulse, ForceMode.Impulse);
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
