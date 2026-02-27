using UnityEngine;

/// <summary>
/// Sistema de gancho (Grappling Hook).
/// Permite al jugador engancharse a puntos específicos y viajar hacia ellos.
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
    [SerializeField] private float travelSpeed = 20f;
    [SerializeField] private float cooldown = 1f;
    
    [Header("=== VISUALS ===")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Transform hookOrigin;
    
    // Properties
    public float TravelSpeed => travelSpeed;
    public bool IsActive { get; private set; }
    public Vector3 CurrentTarget { get; private set; }
    
    // Runtime
    private float _lastFireTime;
    private Transform _currentHookPoint;
    private Transform _playerTransform;
    
    private void Awake()
    {
        _playerTransform = GetComponentInParent<PlayerStateMachine>()?.transform ?? transform.parent;
        
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
        UpdateRopeVisual();
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
    /// Dispara el gancho hacia el mejor objetivo disponible
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
        
        Debug.Log($"Hook fired to {target}");
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
    }
    
    private void UpdateRopeVisual()
    {
        if (!IsActive || ropeRenderer == null) return;
        
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, hookOrigin.position);
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
