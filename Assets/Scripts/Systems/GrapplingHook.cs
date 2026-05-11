using UnityEngine;
using DG.Tweening;

/// <summary>
/// Sistema de gancho (Grappling Hook).
/// Permite al jugador engancharse a puntos específicos o atraer objetos.
/// </summary>
public class GrapplingHook : MonoBehaviour
{
    [Header("=== DETECTION ===")]
    [SerializeField] private float maxRange = 25f;
    #pragma warning disable CS0414
    [SerializeField] private float detectionRadius = 3f;
    #pragma warning restore CS0414
    [SerializeField] private LayerMask hookableMask;
    [SerializeField] private LayerMask pullableMask; // Capa para objetos que se pueden atraer
    [SerializeField] private string hookPointTag = "HookPoint";

    [Header("=== TRAVEL ===")]
    [SerializeField] private float travelSpeed = 20f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float pullSpeed = 15f; // Velocidad para atraer objetos

    [Header("=== VISUALS ===")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Transform hookOrigin;
    [SerializeField] private punteia uiPunteia;

    // Properties
    public float TravelSpeed => travelSpeed;
    public bool IsActive { get; private set; }
    public Vector3 CurrentTarget { get; set; }
    public Transform CurrentHookPoint => _currentHookPoint;
    public bool IsPulling { get; private set; }
    public Transform PulledObject { get; private set; }
    public bool ModoPull { get; private set; }

    // Runtime
    private float _lastFireTime;
    private Transform _currentHookPoint;
    private Transform _playerTransform;
    private float _lostTargetTime;
    private Transform _lastValidTarget;

    // Pull mode timers
    private float _pullStuckTimer;
    private float _pullDurationTimer;
    private PlayerInputHandler _inputHandler;

    private void Awake()
    {
        _playerTransform = GetComponentInParent<PlayerStateMachine>()?.transform ?? transform.parent;
        _inputHandler = GetComponentInParent<PlayerInputHandler>();

        if (hookOrigin == null)
            hookOrigin = transform;

        // Setup line renderer if not assigned
        if (ropeRenderer == null)
        {
            ropeRenderer = gameObject.AddComponent<LineRenderer>();
            ropeRenderer.startWidth = 0.05f;
            ropeRenderer.endWidth = 0.05f;
            ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            ropeRenderer.startColor = Color.gray;
            ropeRenderer.endColor = Color.white;
        }

        ropeRenderer.enabled = false;
    }

    private void Update()
    {
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 aimForward = cam != null ? cam.forward : _playerTransform.forward;

        UpdateRopeVisual();

        // Dibujar rayo rojo suavizado para debug (dirección de apuntado)
        Vector3 debugOrigin = cam != null ? cam.position : _playerTransform.position + Vector3.up;
        Debug.DrawRay(debugOrigin, aimForward * maxRange, Color.red);

        // Cambiar modo con la acción rebindeable "CambiarGancho"
        if (IsToggleModePressed())
        {
            ModoPull = !ModoPull;
            Debug.Log($"Gancho modo: {(ModoPull ? "ATRAER OBJETOS" : "HOOKPOINT")}");
        }

        Vector3 bestTargetPos = FindBestTarget();

        // Actualizar el color de la UI
        if (uiPunteia != null)
        {
            if (!IsActive && bestTargetPos != Vector3.zero)
            {
                uiPunteia.ActivarColorHookpoint();
            }
            else
            {
                uiPunteia.DesactivarColorHookpoint();
            }
        }

        // Si estamos atrayendo un objeto, moverlo hacia nosotros
        if (IsActive && IsPulling)
        {
            if (PulledObject == null)
            {
                Release();
            }
            else
            {
                Vector3 targetPos = hookOrigin.position;
                float distBefore = Vector3.Distance(PulledObject.position, targetPos);

                PulledObject.position = Vector3.MoveTowards(PulledObject.position, targetPos, pullSpeed * Time.deltaTime);

                float distAfter = Vector3.Distance(PulledObject.position, targetPos);
                _pullDurationTimer += Time.deltaTime;

                // Si apenas se ha movido, incrementar el contador de "atascado"
                if (Mathf.Abs(distBefore - distAfter) < 0.01f)
                {
                    _pullStuckTimer += Time.deltaTime;
                }
                else
                {
                    _pullStuckTimer = 0f;
                }

                // Si llega cerca del origen, o si lleva 0.5s atascado, o si han pasado 3s en total, soltar
                if (distAfter < 1.5f || _pullStuckTimer > 0.5f || _pullDurationTimer > 3f)
                {
                    Release();
                }
            }
        }
    }

    private bool IsToggleModePressed()
    {
        if (_inputHandler != null)
        {
            KeyCode toggleKey = _inputHandler.GetActionKey("CambiarGancho");
            if (toggleKey != KeyCode.None)
                return Input.GetKeyDown(toggleKey);
        }

        return Input.GetKeyDown(KeyCode.R);
    }

    public bool CanFire()
    {
        if (IsActive) return false;
        if (Time.time - _lastFireTime < cooldown) return false;

        return true;
    }

    public Vector3 FindBestTarget()
    {
        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 aimForward = cam != null ? cam.forward : _playerTransform.forward;
        Vector3 originPos = cam != null ? cam.position : _playerTransform.position + Vector3.up;

        // Dependiendo del modo, buscamos hookableMask o pullableMask.
        LayerMask maskToUse = ModoPull ? (pullableMask != 0 ? pullableMask : hookableMask) : hookableMask;

        Collider[] colliders = Physics.OverlapSphere(originPos, maxRange, maskToUse);

        foreach (var col in colliders)
        {
            if (!ModoPull)
            {
                if (!string.IsNullOrEmpty(hookPointTag) && !col.CompareTag(hookPointTag))
                    continue;
            }
            else
            {
                // En modo pull, necesitamos un rigidbody (o algo agarrable)
                if (col.GetComponent<Rigidbody>() == null)
                    continue;

                // No intentar atraer partes del player
                if (col.CompareTag("Player") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
                    continue;
            }

            Vector3 targetPos = col.transform.position;
            Vector3 directionToTarget = targetPos - originPos;

            float angle = Vector3.Angle(aimForward, directionToTarget);
            if (angle > 15f) continue;

            int layerMaskToIgnore = maskToUse.value | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast"));
            int obstacleMask = ~layerMaskToIgnore;

            if (Physics.Raycast(originPos, directionToTarget.normalized, directionToTarget.magnitude - 0.5f, obstacleMask))
                continue;

            float distance = directionToTarget.magnitude;
            float score = angle + (distance * 0.5f);

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = col.transform;
            }
        }

        if (bestTarget != null)
        {
            _currentHookPoint = bestTarget;
            _lastValidTarget = bestTarget;
            _lostTargetTime = Time.time;
        }
        else
        {
            if (Time.time - _lostTargetTime <= 0.1f && _lastValidTarget != null)
            {
                bestTarget = _lastValidTarget;
                _currentHookPoint = bestTarget;
            }
            else
            {
                _currentHookPoint = null;
                _lastValidTarget = null;
            }
        }

        return bestTarget != null ? bestTarget.position : Vector3.zero;
    }

    public Vector3 Fire()
    {
        Vector3 target = FindBestTarget();

        if (target == Vector3.zero)
        {
            Debug.Log("No valid hook/pull target found");
            return Vector3.zero;
        }

        IsActive = true;
        CurrentTarget = target;
        _lastFireTime = Time.time;

        if (ModoPull)
        {
            IsPulling = true;
            PulledObject = _currentHookPoint;
            _pullStuckTimer = 0f;
            _pullDurationTimer = 0f;

            // Deshabilitar gravedad temporalmente
            Rigidbody rb = PulledObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
            }
        }
        else
        {
            IsPulling = false;
            PulledObject = null;
        }

        ropeRenderer.enabled = true;

        Debug.Log($"Hook fired to {target}");
        return target;
    }

    public void Release()
    {
        if (IsPulling && PulledObject != null)
        {
            Rigidbody rb = PulledObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true; // Restaurar gravedad
            }
        }

        IsActive = false;
        CurrentTarget = Vector3.zero;
        _currentHookPoint = null;
        _lastValidTarget = null;
        IsPulling = false;
        PulledObject = null;

        ropeRenderer.enabled = false;
    }

    private void UpdateRopeVisual()
    {
        if (!IsActive || ropeRenderer == null) return;

        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, hookOrigin.position);
        ropeRenderer.SetPosition(1, IsPulling && PulledObject != null ? PulledObject.position : CurrentTarget);
    }

    private void OnDrawGizmosSelected()
    {
        Transform playerOrTransform = Application.isPlaying ? (_playerTransform ?? transform) : transform;
        Transform camTransform = Camera.main != null ? Camera.main.transform : null;

        if (playerOrTransform == null) return;

        Vector3 originPos = camTransform != null ? camTransform.position : playerOrTransform.position + Vector3.up;
        Vector3 forwardDir = camTransform != null ? camTransform.forward : playerOrTransform.forward;

        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(originPos, maxRange);

        Gizmos.color = Color.cyan;
        Vector3 forward = forwardDir * maxRange;
        Vector3 leftEdge = Quaternion.Euler(0, -15, 0) * forward;
        Vector3 rightEdge = Quaternion.Euler(0, 15, 0) * forward;

        Gizmos.DrawRay(originPos, leftEdge);
        Gizmos.DrawRay(originPos, rightEdge);

        if (IsActive)
        {
            Gizmos.color = Color.green;
            Vector3 target = IsPulling && PulledObject != null ? PulledObject.position : CurrentTarget;
            Gizmos.DrawWireSphere(target, 0.5f);
            Gizmos.DrawLine(hookOrigin?.position ?? originPos, target);
        }
    }
}
