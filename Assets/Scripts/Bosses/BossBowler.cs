using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Boss que lanza rocas al jugador de forma periódica.
/// Se activa llamando a Activate() desde el UnityEvent de CharacterDialogue / NPCInteractable.
///
/// Setup en escena:
///   · Asignar rockPrefab (debe tener BossRock + Rigidbody + Collider)
///   · Asignar throwPoint (Transform hijo desde donde salen las rocas)
///   · Asignar playerTransform (o dejar vacío para buscarlo por tag al activar)
/// </summary>
public class BossBowler : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  REFERENCIAS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Referencias ──")]
    [Tooltip("Prefab de roca a lanzar. Necesita el componente BossRock.")]
    public GameObject rockPrefab;

    [Tooltip("Punto de origen del lanzamiento (hijo del boss). Si no se asigna, usa la posición del boss.")]
    public Transform throwPoint;

    [Tooltip("Transform del jugador. Si no se asigna, se busca automáticamente por tag 'Player' al activarse.")]
    public Transform playerTransform;

    // ─────────────────────────────────────────────────────────────────────────
    //  PARÁMETROS DE LANZAMIENTO
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Lanzamiento ──")]
    [Tooltip("Segundos entre lanzamientos.")]
    public float throwInterval = 2.5f;

    [Tooltip("Fuerza con la que sale la roca.")]
    public float throwForce = 14f;

    [Tooltip("Offset de altura sobre el jugador para apuntar (evita disparar al suelo).")]
    public float aimHeightOffset = 1f;

    [Tooltip("Ángulo de dispersión aleatoria (grados). 0 = apunta exacto.")]
    [Range(0f, 45f)]
    public float spreadAngle = 5f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PARÁMETROS DEL PROYECTIL (se copian a BossRock al instanciar)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Efecto en el player ──")]
    [Tooltip("Metros que cae el jugador al recibir el impacto.")]
    public float knockdownDistance = 1f;

    [Tooltip("Fuerza de impulso hacia abajo al recibir el impacto.")]
    public float knockdownImpulse = 8f;

    [Tooltip("Multiplicador de velocidad durante el slow (0.35 = 35% de velocidad).")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.35f;

    [Tooltip("Duración del efecto de ralentización en segundos.")]
    public float slowDuration = 3f;

    [Tooltip("Tiempo en segundos antes de que la roca se destruya si no impacta nada.")]
    public float rockLifetime = 8f;

    // ─────────────────────────────────────────────────────────────────────────
    //  ROTACIÓN HACIA EL PLAYER
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Comportamiento ──")]
    [Tooltip("Velocidad a la que el boss rota para mirar al jugador (grados/seg).")]
    public float rotationSpeed = 5f;

    [Tooltip("Si está activo, el boss gira para encarar al jugador mientras lanza.")]
    public bool facePlayer = true;

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMACIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Animación ──")]
    [Tooltip("Nombre del trigger de animación de lanzamiento (opcional).")]
    public string throwAnimTrigger = "Throw";

    // ─────────────────────────────────────────────────────────────────────────
    //  EVENTOS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Eventos ──")]
    public UnityEvent onActivated;
    public UnityEvent onRockThrown;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool _isActive = false;
    private Animator _animator;
    private Coroutine _throwRoutine;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_isActive || playerTransform == null) return;

        if (facePlayer)
        {
            Vector3 dir = playerTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target,
                    rotationSpeed * Time.deltaTime);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ACTIVACIÓN (llamar desde UnityEvent del diálogo)
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Activa el boss. Llámalo desde CharacterDialogue.onInitialDialogueFinished
    /// o NPCInteractable.onAllDialogueFinished en el Inspector.
    /// </summary>
    public void Activate()
    {
        if (_isActive) return;

        // Auto-buscar el player si no se asignó
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
            {
                Debug.LogWarning("[BossBowler] No se encontró el Player en la escena. Asigna playerTransform manualmente.");
                return;
            }
        }

        _isActive = true;
        onActivated?.Invoke();
        _throwRoutine = StartCoroutine(ThrowLoop());
        Debug.Log("[BossBowler] Activado. Empezando a lanzar rocas.");
    }

    /// <summary>Desactiva el boss y detiene el bucle de lanzamiento.</summary>
    public void Deactivate()
    {
        _isActive = false;
        if (_throwRoutine != null)
        {
            StopCoroutine(_throwRoutine);
            _throwRoutine = null;
        }
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
            Debug.LogWarning("[BossBowler] rockPrefab no asignado.");
            return;
        }

        // Punto de origen
        Vector3 origin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up * 1.5f;

        // Dirección base hacia el player (con offset de altura)
        Vector3 target  = playerTransform.position + Vector3.up * aimHeightOffset;
        Vector3 baseDir = (target - origin).normalized;

        // Aplicar dispersión aleatoria
        Vector3 dir = ApplySpread(baseDir, spreadAngle);

        // Instanciar roca
        GameObject rockObj = Instantiate(rockPrefab, origin, Quaternion.LookRotation(dir));

        // Configurar parámetros en BossRock
        BossRock rock = rockObj.GetComponent<BossRock>();
        if (rock != null)
        {
            rock.knockdownDistance = knockdownDistance;
            rock.knockdownImpulse  = knockdownImpulse;
            rock.slowMultiplier    = slowMultiplier;
            rock.slowDuration      = slowDuration;
            rock.lifetime          = rockLifetime;
        }

        // Aplicar fuerza
        Rigidbody rb = rockObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(dir * throwForce, ForceMode.Impulse);

        // Trigger de animación
        if (_animator != null && !string.IsNullOrEmpty(throwAnimTrigger))
            _animator.SetTrigger(throwAnimTrigger);

        onRockThrown?.Invoke();
        Debug.Log($"[BossBowler] Roca lanzada hacia {playerTransform.name}.");
    }

    /// <summary>Añade dispersión aleatoria a una dirección.</summary>
    private Vector3 ApplySpread(Vector3 baseDir, float maxAngle)
    {
        if (maxAngle <= 0f) return baseDir;
        float angle = Random.Range(-maxAngle, maxAngle);
        return Quaternion.AngleAxis(angle, Vector3.up) * baseDir;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up * 1.5f;
        Gizmos.DrawWireSphere(origin, 0.2f);

        if (playerTransform != null)
        {
            Gizmos.DrawLine(origin, playerTransform.position + Vector3.up * aimHeightOffset);
        }
    }
}
