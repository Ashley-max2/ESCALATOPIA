using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Boss Levitante: al activarse vuela suavemente desde su posición inicial
/// hasta el Transform "ObjetivoFinal", con un efecto de levitación ondulante.
///
/// Setup en escena:
///   · Asignar ObjetivoFinal (Transform vacío en la posición de llegada)
///   · Asignar BossBowler (para activarlo junto con este boss)
///   · En CharacterDialogue.onInitialDialogueFinished (o NPCInteractable.onAllDialogueFinished)
///     conectar → BossLevitante.Activate()
///
/// Opcionalmente también conectar → BossBowler.Activate() en el mismo evento
/// si quieres que ambos se activen a la vez.
/// </summary>
public class BossLevitante : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  REFERENCIAS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Referencias ──")]
    [Tooltip("Transform destino al que vuela el boss. Crea un GameObject vacío en la escena y asígnalo aquí.")]
    public Transform objetivoFinal;

    [Tooltip("BossBowler a activar automáticamente junto con este boss. Puede dejarse vacío si lo activas por separado desde el diálogo.")]
    public BossBowler bossBowler;

    // ─────────────────────────────────────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Movimiento ──")]
    [Tooltip("Velocidad de vuelo hacia ObjetivoFinal (unidades/seg).")]
    public float flySpeed = 3f;

    [Tooltip("Curva de aceleración/deceleración del vuelo. Deja la curva por defecto para un movimiento suave.")]
    public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Tiempo mínimo de vuelo en segundos (aunque la distancia sea corta).")]
    public float minFlyDuration = 2f;

    // ─────────────────────────────────────────────────────────────────────────
    //  LEVITACIÓN / OSCILACIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Levitación ──")]
    [Tooltip("Amplitud del movimiento ondulante vertical mientras levita (metros).")]
    public float hoverAmplitude = 0.25f;

    [Tooltip("Frecuencia del movimiento ondulante vertical (ciclos/seg).")]
    public float hoverFrequency = 1.2f;

    [Tooltip("Amplitud del balanceo horizontal (metros).")]
    public float swayAmplitude = 0.08f;

    [Tooltip("Frecuencia del balanceo horizontal.")]
    public float swayFrequency = 0.8f;

    // ─────────────────────────────────────────────────────────────────────────
    //  ROTACIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Rotación ──")]
    [Tooltip("Velocidad de giro hacia el ObjetivoFinal mientras vuela.")]
    public float rotationSpeed = 4f;

    [Tooltip("Eje de rotación de balanceo lento (visual flavor).")]
    public float tiltAmplitude = 4f;

    [Tooltip("Frecuencia del balanceo rotacional.")]
    public float tiltFrequency = 0.6f;

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMACIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Animación ──")]
    [Tooltip("Nombre del parámetro bool del Animator para el estado de levitación (opcional).")]
    public string levitateAnimBool = "IsLevitating";

    [Tooltip("Nombre del trigger de animación de llegada (opcional).")]
    public string arriveAnimTrigger = "Arrive";

    // ─────────────────────────────────────────────────────────────────────────
    //  ACTIVACIÓN DEL BOWLER
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Timing ──")]
    [Tooltip("Segundos de retraso entre el fin del diálogo y el inicio del vuelo.")]
    public float activationDelay = 0.5f;

    [Tooltip("Si es true, activa el BossBowler automáticamente al empezar a volar.")]
    public bool activateBowlerOnStart = true;

    [Tooltip("Si es true, el BossBowler también se activa cuando el Levitante llega a ObjetivoFinal.")]
    public bool activateBowlerOnArrival = false;

    // ─────────────────────────────────────────────────────────────────────────
    //  EVENTOS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Eventos ──")]
    public UnityEvent onActivated;
    public UnityEvent onFlightStarted;
    public UnityEvent onArrived;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private bool _isActive    = false;
    private bool _hasArrived  = false;
    private bool _isFlying    = false;
    private float _hoverTime  = 0f;

    private Vector3 _startPosition;
    private Vector3 _initialPosition;
    private Animator _animator;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _initialPosition = transform.position;
        _startPosition = _initialPosition;
    }

    private void Update()
    {
        if (!_isActive) return;

        // Siempre aplica el efecto de levitación ondulante
        ApplyHoverEffect();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ACTIVACIÓN PÚBLICA (conectar desde UnityEvent del diálogo)
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Activa el boss levitante. Conéctalo a:
    ///   · CharacterDialogue.onInitialDialogueFinished
    ///   · NPCInteractable.onAllDialogueFinished
    /// </summary>
    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        onActivated?.Invoke();

        if (activateBowlerOnStart && bossBowler != null)
            bossBowler.Activate();

        StartCoroutine(ActivationSequence());
        Debug.Log("[BossLevitante] Activado.");
    }

    /// <summary>Detiene el boss en su posición actual.</summary>
    public void Deactivate()
    {
        _isActive = false;
        _isFlying = false;
        StopAllCoroutines();
        SetLevitateAnim(false);
    }

    /// <summary>Detiene el boss y lo devuelve a su posición original.</summary>
    public void ResetBoss()
    {
        Deactivate();
        transform.position = _initialPosition;
        _startPosition = _initialPosition;
        _hasArrived = false;
        
        if (bossBowler != null)
        {
            bossBowler.Deactivate();
        }
        
        Debug.Log("[BossLevitante] Reseteado a la posición inicial.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SECUENCIA
    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator ActivationSequence()
    {
        // 1. Pequeño delay dramático
        yield return new WaitForSeconds(activationDelay);

        // 2. Despegue con animación
        SetLevitateAnim(true);
        onFlightStarted?.Invoke();

        // 3. Si hay objetivo, volar hacia él
        if (objetivoFinal != null)
            yield return StartCoroutine(FlyToTarget());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  VUELO HACIA OBJETIVOFINAL
    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator FlyToTarget()
    {
        _isFlying = true;

        Vector3 origin = transform.position;
        Vector3 destination = objetivoFinal.position;
        float distance = Vector3.Distance(origin, destination);
        float duration = Mathf.Max(minFlyDuration, distance / flySpeed);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curved = flyCurve.Evaluate(t);

            // Posición base interpolada (sin el hover, que se aplica en Update)
            // Guardamos la base en _startPosition para que ApplyHoverEffect
            // la tome como referencia
            _startPosition = Vector3.Lerp(origin, destination, curved);

            // Rotar suavemente hacia el destino
            Vector3 dir = (destination - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                    rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Llegar exactamente al destino
        _startPosition = destination;
        _isFlying = false;
        _hasArrived = true;

        // Animación de llegada
        if (_animator != null && !string.IsNullOrEmpty(arriveAnimTrigger))
            _animator.SetTrigger(arriveAnimTrigger);

        onArrived?.Invoke();

        if (activateBowlerOnArrival && bossBowler != null)
            bossBowler.Activate();

        Debug.Log("[BossLevitante] Llegó a ObjetivoFinal.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTO DE LEVITACIÓN ONDULANTE
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyHoverEffect()
    {
        _hoverTime += Time.deltaTime;

        float verticalOffset   = Mathf.Sin(_hoverTime * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
        float horizontalOffset = Mathf.Sin(_hoverTime * swayFrequency  * Mathf.PI * 2f) * swayAmplitude;

        transform.position = _startPosition
            + Vector3.up    * verticalOffset
            + transform.right * horizontalOffset;

        // Balanceo rotacional sutil (roll)
        float tilt = Mathf.Sin(_hoverTime * tiltFrequency * Mathf.PI * 2f) * tiltAmplitude;
        Vector3 euler = transform.eulerAngles;
        euler.z = tilt;
        transform.eulerAngles = euler;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private void SetLevitateAnim(bool value)
    {
        if (_animator != null && !string.IsNullOrEmpty(levitateAnimBool))
            _animator.SetBool(levitateAnimBool, value);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (objetivoFinal == null) return;

        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.8f);
        Gizmos.DrawLine(transform.position, objetivoFinal.position);
        Gizmos.DrawWireSphere(objetivoFinal.position, 0.4f);

        Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        // Arco de vuelo aproximado
#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0.4f, 0.8f, 1f, 0.3f);
        UnityEditor.Handles.DrawDottedLine(transform.position, objetivoFinal.position, 4f);
#endif
    }
}
