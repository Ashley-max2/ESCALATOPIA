using UnityEngine;

/// <summary>
/// Cámara orbital estilo Zelda Breath of the Wild.
/// Sigue al jugador con suavizado y permite rotación con el ratón.
/// Incluye colisión con el entorno para evitar atravesar paredes.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("=== TARGET ===")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);
    
    [Header("=== DISTANCE ===")]
    [SerializeField] private float defaultDistance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float scrollSensitivity = 2f;
    
    [Header("=== ROTATION ===")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 70f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    
    [Header("=== FOLLOW ===")]
    [SerializeField] private float followSmoothTime = 0.15f;
    
    [Header("=== COLLISION ===")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionBuffer = 0.3f;
    
    // Runtime
    private float _currentDistance;
    private float _targetDistance;
    private float _horizontalAngle;
    private float _verticalAngle;
    private Vector3 _currentVelocity;
    private Vector2 _rotationVelocity;
    private Vector2 _currentRotation;
    
    private void Awake()
    {
        // Find player if not assigned
        if (target == null)
        {
            var player = FindObjectOfType<PlayerStateMachine>();
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        _currentDistance = defaultDistance;
        _targetDistance = defaultDistance;
        
        // Initialize rotation based on current position
        if (target != null)
        {
            Vector3 direction = transform.position - GetTargetPosition();
            _horizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _verticalAngle = Mathf.Asin(direction.normalized.y) * Mathf.Rad2Deg;
        }
        
        _currentRotation = new Vector2(_verticalAngle, _horizontalAngle);
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        // Rotation input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        _horizontalAngle += mouseX;
        _verticalAngle -= mouseY;
        _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);
        
        // Zoom input
        float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        _targetDistance = Mathf.Clamp(_targetDistance - scroll, minDistance, maxDistance);
    }
    
    private void UpdateCameraPosition()
    {
        // Smooth rotation
        Vector2 targetRotation = new Vector2(_verticalAngle, _horizontalAngle);
        _currentRotation = Vector2.SmoothDamp(_currentRotation, targetRotation, ref _rotationVelocity, rotationSmoothTime);
        
        // Smooth distance
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * 10f);
        
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(_currentRotation.x, _currentRotation.y, 0);
        Vector3 targetPosition = GetTargetPosition();
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * _currentDistance);
        
        // Collision check
        float actualDistance = CheckCollision(targetPosition, desiredPosition);
        Vector3 finalPosition = targetPosition - (rotation * Vector3.forward * actualDistance);
        
        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, followSmoothTime);
        
        // Look at target
        transform.LookAt(targetPosition);
    }
    
    private Vector3 GetTargetPosition()
    {
        return target.position + targetOffset;
    }
    
    private float CheckCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        Vector3 direction = desiredPos - targetPos;
        float maxDistance = direction.magnitude;
        
        RaycastHit hit;
        if (Physics.SphereCast(targetPos, collisionBuffer, direction.normalized, out hit, maxDistance, collisionMask))
        {
            return hit.distance - collisionBuffer;
        }
        
        return maxDistance;
    }
    
    /// <summary>
    /// Establece el objetivo de la cámara (llamar desde PlayerStateMachine)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Obtiene la dirección forward de la cámara (sin componente Y)
    /// </summary>
    public Vector3 GetFlatForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    /// <summary>
    /// Obtiene la dirección right de la cámara (sin componente Y)
    /// </summary>
    public Vector3 GetFlatRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetTargetPosition(), 0.2f);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(GetTargetPosition(), transform.position);
    }
}
