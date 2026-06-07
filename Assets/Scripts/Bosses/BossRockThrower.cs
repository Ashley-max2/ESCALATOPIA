using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Boss estático que lanza rocas periódicamente hacia el jugador.
/// Al impactar, la roca hace caer al player y lo ralentiza.
///
/// Setup mínimo en Inspector:
///   · rockPrefab  → Prefab con RockProjectile + Rigidbody + Collider esférico
///   · throwPoint  → Transform hijo desde donde salen las rocas (ej. mano/hombro)
///
/// Activación:
///   · Llama a Activate() desde un UnityEvent, NPCInteractable, BossManager, etc.
///   · Llama a Deactivate() para detenerlo (al reiniciar carrera, etc.)
/// </summary>
public class BossRockThrower : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  REFERENCIAS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Referencias ──")]
    [Tooltip("Prefab de la roca. Debe tener el componente RockProjectile.")]
    public GameObject rockPrefab;

    [Tooltip("Punto de origen del lanzamiento (Transform hijo del boss, ej. mano).")]
    public Transform throwPoint;

    [Tooltip("Transform del jugador. Si no se asigna se busca automáticamente por tag 'Player'.")]
    public Transform playerTransform;

    // ─────────────────────────────────────────────────────────────────────────
    //  LANZAMIENTO
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Lanzamiento ──")]
    [Tooltip("Segundos entre cada lanzamiento.")]
    public float throwInterval = 4f;

    [Tooltip("Fuerza de lanzamiento de la roca.")]
    public float throwForce = 16f;

    [Tooltip("Offset de altura sobre el jugador para apuntar (evita disparar al suelo).")]
    public float aimHeightOffset = 1.2f;

    [Tooltip("Ángulo de dispersión aleatoria (grados). 0 = apunta exacto al player.")]
    [Range(0f, 45f)]
    public float spreadAngle = 8f;

    [Tooltip("Si es true, el boss rota para mirar al jugador mientras está activo.")]
    public bool facePlayer = true;

    [Tooltip("Velocidad de rotación hacia el jugador (grados/seg).")]
    public float rotationSpeed = 4f;

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTO EN EL PLAYER
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Efecto en el Player ──")]
    [Tooltip("Impulso hacia abajo al recibir el impacto (hace que caiga).")]
    public float knockdownImpulse = 10f;

    [Tooltip("Fuerza de empuje horizontal al recibir la roca.")]
    public float pushForce = 5f;

    [Tooltip("Multiplicador de velocidad del slow (0.35 = 35% de la velocidad normal).")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.35f;

    [Tooltip("Duración del efecto de ralentización en segundos.")]
    public float slowDuration = 3f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PROYECTIL
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Proyectil ──")]
    [Tooltip("Segundos hasta que la roca se destruye si no golpea nada.")]
    public float rockLifetime = 10f;

    [Tooltip("Segundos hasta que la roca desaparece tras colisionar.")]
    public float rockDestroyDelay = 3f;

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMACIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Animación ──")]
    [Tooltip("Nombre del trigger de lanzamiento en el Animator (dejar vacío para ignorar).")]
    public string throwAnimTrigger = "Throw";

    // ─────────────────────────────────────────────────────────────────────────
    //  EVENTOS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Eventos ──")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;
    public UnityEvent onRockThrown;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool      _isActive    = false;
    private Coroutine _throwRoutine;
    private Animator  _animator;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_isActive || !facePlayer || playerTransform == null) return;

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target,
                rotationSpeed * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Activa el lanzador. Llámalo desde un UnityEvent, BossManager, etc.
    /// </summary>
    public void Activate()
    {
        if (_isActive) return;

        // Auto-buscar player si no se asignó
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
            {
                Debug.LogWarning("[BossRockThrower] No se encontró el Player. Asigna playerTransform en el Inspector.");
                return;
            }
        }

        _isActive = true;
        _throwRoutine = StartCoroutine(ThrowLoop());
        onActivated?.Invoke();
        Debug.Log("[BossRockThrower] Activado.");
    }

    /// <summary>
    /// Desactiva el lanzador y detiene el bucle de rocas.
    /// </summary>
    public void Deactivate()
    {
        _isActive = false;
        if (_throwRoutine != null)
        {
            StopCoroutine(_throwRoutine);
            _throwRoutine = null;
        }
        onDeactivated?.Invoke();
        Debug.Log("[BossRockThrower] Desactivado.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BUCLE DE LANZAMIENTO
    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator ThrowLoop()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(throwInterval);

            if (_isActive && playerTransform != null)
                ThrowRock();
        }
    }

    private void ThrowRock()
    {
        if (rockPrefab == null)
        {
            Debug.LogError("[BossRockThrower] rockPrefab no asignado.");
            return;
        }

        // Origen del lanzamiento
        Vector3 origin = throwPoint != null
            ? throwPoint.position
            : transform.position + Vector3.up * 1.5f;

        // Dirección hacia el player con offset de altura y dispersión
        Vector3 aimTarget = playerTransform.position + Vector3.up * aimHeightOffset;
        Vector3 baseDir   = (aimTarget - origin).normalized;
        Vector3 dir       = ApplySpread(baseDir, spreadAngle);

        // Instanciar la roca
        GameObject rockObj = Instantiate(rockPrefab, origin, Quaternion.LookRotation(dir));

        // Configurar parámetros del proyectil
        RockProjectile rock = rockObj.GetComponent<RockProjectile>();
        if (rock != null)
        {
            rock.knockdownImpulse  = knockdownImpulse;
            rock.pushForce         = pushForce;
            rock.slowMultiplier    = slowMultiplier;
            rock.slowDuration      = slowDuration;
            rock.lifetime          = rockLifetime;
            rock.destroyDelay      = rockDestroyDelay;
        }

        // Aplicar fuerza a la roca
        Rigidbody rb = rockObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(dir * throwForce, ForceMode.Impulse);

        // Trigger de animación
        if (_animator != null && !string.IsNullOrEmpty(throwAnimTrigger))
            _animator.SetTrigger(throwAnimTrigger);

        onRockThrown?.Invoke();
        Debug.Log($"[BossRockThrower] Roca lanzada hacia {playerTransform.name}.");
    }

    private Vector3 ApplySpread(Vector3 baseDir, float maxAngle)
    {
        if (maxAngle <= 0f) return baseDir;

        // Dispersión aleatoria en yaw y pitch
        float yaw   = Random.Range(-maxAngle, maxAngle);
        float pitch = Random.Range(-maxAngle * 0.5f, maxAngle * 0.5f);
        return Quaternion.Euler(pitch, yaw, 0f) * baseDir;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = throwPoint != null
            ? throwPoint.position
            : transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, 0.25f);

        if (playerTransform != null)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawLine(origin, playerTransform.position + Vector3.up * aimHeightOffset);
        }
    }
}
