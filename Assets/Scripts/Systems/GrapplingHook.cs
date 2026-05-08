using UnityEngine;

/// <summary>
/// Sistema de gancho (Grappling Hook).
/// Permite al jugador engancharse a puntos específicos y viajar hacia ellos.
/// También permite atraer objetos con la capa pullableMask.
/// Pulsar "R" cambia entre modo hookpoint y modo atraer objetos.
/// </summary>
public class GrapplingHook : MonoBehaviour
{
    [Header("=== DETECTION ===")]
    [SerializeField] private float maxRange = 25f;
    #pragma warning disable CS0414
    [SerializeField] private float detectionRadius = 3f;
    #pragma warning restore CS0414
    [SerializeField] private LayerMask hookableMask;
    [SerializeField] private string hookPointTag = "HookPoint";
    
    [Header("=== TRAVEL ===")]
    public float travelSpeed = 20f;
    [SerializeField] private float cooldown = 1f;
    
    [Header("=== PULL OBJECTS ===")]
    [Tooltip("Capa de objetos que se pueden atraer con el gancho")]
    [SerializeField] private LayerMask pullableMask;
    [SerializeField] private float pullSpeed = 15f;
    [SerializeField] private float pullStopDistance = 1.5f;
    
    [Header("=== VISUALS ===")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Transform hookOrigin;
    [SerializeField] private punteia uiPunteia;
    [Tooltip("Desactivar si hay un CuerdaRenderer externo manejando la cuerda")]
    [SerializeField] private bool usarRendererInterno = true;
    
    public event System.Action OnHookAttached;
    
    // Properties
    public float TravelSpeed => travelSpeed;
    public float PullSpeed => pullSpeed;
    public float PullStopDistance => pullStopDistance;
    
    public void SetMaxRange(float val) => maxRange = val;
    public void SetTravelSpeed(float val) => travelSpeed = val;
    public void SetPullSpeed(float val) => pullSpeed = val;
    public bool IsActive { get; private set; }
    public Vector3 CurrentTarget { get; set; }
    public bool IsPulling { get; private set; }
    public Rigidbody PulledObject { get; private set; }
    public Transform HookOrigin => hookOrigin;
    public CuerdaRenderer Cuerda { get; private set; }
    
    /// <summary>
    /// Modo actual del gancho: true = modo atraer objetos, false = modo hookpoint
    /// </summary>
    public bool ModoPull { get; private set; } = false;
    
    // Runtime
    private float _lastFireTime;
    private Transform _currentHookPoint;
    private Transform _playerTransform;
    
    // Referencia al sistema de agarrar para saber si tiene un objeto
    private AgarrarLanzarSoltar _agarrarSystem;
    
    private void Awake()
    {
        _playerTransform = GetComponentInParent<PlayerStateMachine>()?.transform ?? transform.parent;
        
        if (hookOrigin == null)
            hookOrigin = transform;
        
        // Buscar CuerdaRenderer en hijos
        Cuerda = GetComponentInChildren<CuerdaRenderer>();
        
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
    
    private void Start()
    {
        // Buscar el sistema de agarrar en el player
        if (_playerTransform != null)
            _agarrarSystem = _playerTransform.GetComponentInChildren<AgarrarLanzarSoltar>();
        if (_agarrarSystem == null)
            _agarrarSystem = FindObjectOfType<AgarrarLanzarSoltar>();
    }
    
    private void Update()
    {
        // Cambiar modo con R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ModoPull = !ModoPull;
            Debug.Log($"Gancho modo: {(ModoPull ? "ATRAER OBJETOS" : "HOOKPOINT")}");
        }
        
        UpdateRopeVisual();
        
        // Actualizar el color de la UI
        if (uiPunteia != null)
        {
            if (!IsActive)
            {
                bool hayObjetivo = false;

                if (ModoPull)
                    hayObjetivo = TryFindPullTarget() != null;
                else
                    hayObjetivo = FindBestTarget() != Vector3.zero;

                if (hayObjetivo)
                    uiPunteia.ActivarColorHookpoint();
                else
                    uiPunteia.DesactivarColorHookpoint();
            }
            else
            {
                uiPunteia.DesactivarColorHookpoint();
            }
        }
    }
    
    /// <summary>
    /// Verifica si el gancho puede dispararse
    /// </summary>
    public bool CanFire()
    {
        if (IsActive) return false;
        if (Time.time - _lastFireTime < cooldown) return false;
        
        return true;
    }
    
    /// <summary>
    /// Comprueba si hay un objeto agarrado (no se puede enganchar un objeto que ya tienes)
    /// </summary>
    public bool TieneObjetoAgarrado()
    {
        return _agarrarSystem != null && _agarrarSystem.TieneObjeto;
    }
    
    /// <summary>
    /// Busca y devuelve el mejor punto de gancho disponible
    /// </summary>
    public Vector3 FindBestTarget()
    {
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        
        // Usar la direccion de la camara para apuntar (hay que mirar al hook point)
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 aimForward = cam != null ? cam.forward : _playerTransform.forward;
        
        // Find all potential hook points
        Collider[] colliders = Physics.OverlapSphere(_playerTransform.position, maxRange, hookableMask);
        
        foreach (var col in colliders)
        {
            // Check tag (optional)
            if (!string.IsNullOrEmpty(hookPointTag) && !col.CompareTag(hookPointTag))
                continue;
            
            Vector3 targetPos = col.transform.position;
            Vector3 directionToTarget = targetPos - _playerTransform.position;
            
            // Must be where the camera is looking (within 30 degree cone)
            float angle = Vector3.Angle(aimForward, directionToTarget);
            if (angle > 15f) continue;
            
            // Line of sight check
            if (Physics.Raycast(_playerTransform.position + Vector3.up, directionToTarget.normalized, 
                directionToTarget.magnitude - 0.5f, ~hookableMask))
                continue;
            
            // Score based on angle and distance (lower is better)
            float distance = directionToTarget.magnitude;
            float score = angle + (distance * 0.5f);
            
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = col.transform;
            }
        }
        
        _currentHookPoint = bestTarget;
        return bestTarget != null ? bestTarget.position : Vector3.zero;
    }
    
    /// <summary>
    /// Busca un objeto atraíble en la dirección de la cámara (raycast directo).
    /// Solo busca si pullableMask está configurado.
    /// No puede atraer objetos que estén agarrados por el player.
    /// </summary>
    public Rigidbody TryFindPullTarget()
    {
        if (pullableMask == 0) return null;
        
        Transform cam = Camera.main?.transform;
        if (cam == null) return null;
        
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxRange, pullableMask))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                // No atraer objetos que están siendo agarrados por el player
                if (_agarrarSystem != null && _agarrarSystem.EsObjetoAgarrado(rb.gameObject))
                    return null;
            }
            return rb;
        }
        return null;
    }
    
    /// <summary>
    /// Dispara el gancho según el modo actual.
    /// En modo hookpoint: se engancha al punto más cercano.
    /// En modo pull: atrae un objeto hacia el jugador.
    /// Retorna el target o Vector3.zero si falla (modo hookpoint).
    /// Para modo pull, usar FireInCurrentMode() que retorna info completa.
    /// </summary>
    public Vector3 Fire()
    {
        Vector3 target = FindBestTarget();
        
        if (target == Vector3.zero)
        {
            Debug.Log("No valid hook point found");
            return Vector3.zero;
        }
        
        IsActive = true;
        CurrentTarget = target;
        _lastFireTime = Time.time;
        
        // Show rope
        ropeRenderer.enabled = true;
        if (Cuerda != null) { /* Automaticamente detectado por CuerdaRenderer */ }
        
        Debug.Log($"Hook fired to {target}");
        OnHookAttached?.Invoke();
        return target;
    }
    
    /// <summary>
    /// Dispara el gancho para atraer un objeto hacia el jugador.
    /// Retorna el Rigidbody del objeto si se encontró, null si no.
    /// </summary>
    public Rigidbody FirePull()
    {
        Rigidbody target = TryFindPullTarget();
        if (target == null) return null;
        
        IsActive = true;
        IsPulling = true;
        PulledObject = target;
        CurrentTarget = target.position;
        _lastFireTime = Time.time;
        
        ropeRenderer.enabled = true;
        if (Cuerda != null) { /* Automaticamente detectado por CuerdaRenderer */ }
        
        Debug.Log($"Hook pulling object {target.name}");
        OnHookAttached?.Invoke();
        return target;
    }
    
    /// <summary>
    /// Libera el gancho
    /// </summary>
    public void Release()
    {
        IsActive = false;
        CurrentTarget = Vector3.zero;
        _currentHookPoint = null;
        
        // Hide rope
        ropeRenderer.enabled = false;
        if (Cuerda != null) { /* Automaticamente detectado por CuerdaRenderer */ }
    }
    
    /// <summary>
    /// Libera el gancho de atracción de objeto, deteniendo el objeto
    /// </summary>
    public void ReleasePull()
    {
        if (PulledObject != null)
        {
            PulledObject.velocity = Vector3.zero;
            PulledObject.angularVelocity = Vector3.zero;
        }
        IsPulling = false;
        PulledObject = null;
        Release();
    }
    
    private void UpdateRopeVisual()
    {
        if (!usarRendererInterno) return;
        if (!IsActive || ropeRenderer == null) return;
        
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, hookOrigin.position);
        
        // Si estamos atrayendo un objeto, la cuerda va hacia él
        if (IsPulling && PulledObject != null)
            ropeRenderer.SetPosition(1, PulledObject.position);
        else
            ropeRenderer.SetPosition(1, CurrentTarget);
    }
    
    private void OnDrawGizmosSelected()
    {
        // En edit mode _playerTransform aun no existe, usamos transform
        Transform origin = Application.isPlaying ? (_playerTransform ?? transform) : transform;
        
        if (origin == null) return;
        
        // Rango de deteccion
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(origin.position, maxRange);
        
        // Cono de deteccion
        Gizmos.color = Color.cyan;
        Vector3 forward = origin.forward * maxRange;
        Vector3 leftEdge = Quaternion.Euler(0, -15, 0) * forward;
        Vector3 rightEdge = Quaternion.Euler(0, 15, 0) * forward;
        
        Gizmos.DrawRay(origin.position + Vector3.up, leftEdge);
        Gizmos.DrawRay(origin.position + Vector3.up, rightEdge);
        
        // Target actual
        if (IsActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(CurrentTarget, 0.5f);
            Gizmos.DrawLine(hookOrigin?.position ?? origin.position, CurrentTarget);
        }
    }
}
