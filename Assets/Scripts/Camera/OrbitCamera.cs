using UnityEngine;

/// <summary>
/// Camara 3D clasica estilo third-person.
/// Sigue al player de forma rigida (sin lag lateral).
/// Rota con raton + stick derecho del mando (Xbox o PlayStation).
/// Auto-detecta el tipo de mando via PlayerInputHandler.
/// Colisiona con las capas que indiques para no atravesar paredes.
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
    [SerializeField] private float gamepadSensitivity = 3f;
    [SerializeField] private float gamepadDeadzone = 0.25f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 70f;

    [Header("=== COLLISION ===")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionBuffer = 0.3f;
    [SerializeField] private float collisionRecoverSpeed = 10f;

    // Runtime
    private float _currentDistance;
    private float _targetDistance;
    private float _horizontalAngle;
    private float _verticalAngle;
    private Vector3 _smoothedTargetPos;

    // Referencia al input handler para saber tipo de mando
    private PlayerInputHandler _inputHandler;

    private void Awake()
    {
        // Buscar player si no esta asignado
        if (target == null)
        {
            var player = FindObjectOfType<PlayerStateMachine>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Buscar input handler
        _inputHandler = FindObjectOfType<PlayerInputHandler>();

        _currentDistance = defaultDistance;
        _targetDistance = defaultDistance;

        // Iniciar rotacion basandose en la posicion actual
        if (target != null)
        {
            _smoothedTargetPos = GetTargetPosition();
            Vector3 direction = transform.position - _smoothedTargetPos;
            _horizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _verticalAngle = Mathf.Asin(direction.normalized.y) * Mathf.Rad2Deg;
            _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        // No mover la camara si el cursor esta desbloqueado (menu ESC)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Rotar con el raton
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotar con el stick derecho del mando
        float padX = 0f;
        float padY = 0f;

        if (_inputHandler != null && _inputHandler.DetectedGamepad != PlayerInputHandler.GamepadType.None)
        {
            // Elegir ejes segun tipo de mando detectado
            string axisX, axisY;
            if (_inputHandler.DetectedGamepad == PlayerInputHandler.GamepadType.PlayStation)
            {
                axisX = "PSCameraX";
                axisY = "PSCameraY";
            }
            else
            {
                axisX = "GamepadCameraX";
                axisY = "GamepadCameraY";
            }

            try
            {
                float rawX = Input.GetAxisRaw(axisX);
                float rawY = Input.GetAxisRaw(axisY);

                // Deadzone manual para evitar drift del stick
                if (Mathf.Abs(rawX) > gamepadDeadzone) padX = rawX * gamepadSensitivity;
                if (Mathf.Abs(rawY) > gamepadDeadzone) padY = rawY * gamepadSensitivity;
            }
            catch (System.Exception)
            {
                // Eje no existe en InputManager todavia, ignorar
            }
        }

        _horizontalAngle += mouseX + padX;
        _verticalAngle -= (mouseY + padY);
        _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);

        // Zoom con la rueda
        float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        _targetDistance = Mathf.Clamp(_targetDistance - scroll, minDistance, maxDistance);
    }

    private void UpdateCameraPosition()
    {
        // Calcular rotacion directa (sin smooth para que no haga lag lateral)
        Quaternion rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0);

        // Posicion del target (player + offset) con suavizado para evitar tirones al saltar
        Vector3 rawTargetPos = GetTargetPosition();
        float smoothSpeed = 25f; // rapido pero suave, evita tirones
        float dt = Time.unscaledDeltaTime;
        _smoothedTargetPos = Vector3.Lerp(_smoothedTargetPos, rawTargetPos, smoothSpeed * dt);

        // Distancia con smooth solo para el zoom
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, dt * collisionRecoverSpeed);

        // Posicion deseada de la camara
        Vector3 desiredPosition = _smoothedTargetPos - (rotation * Vector3.forward * _currentDistance);

        // Check colision para que no atraviese paredes
        float actualDistance = CheckCollision(_smoothedTargetPos, desiredPosition);
        Vector3 finalPosition = _smoothedTargetPos - (rotation * Vector3.forward * actualDistance);

        // Posicion directa, sin SmoothDamp = camara rigida que no se mueve sola
        transform.position = finalPosition;

        // Mirar al target
        transform.LookAt(_smoothedTargetPos);
    }

    private Vector3 GetTargetPosition()
    {
        return target.position + targetOffset;
    }

    private float CheckCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        Vector3 direction = desiredPos - targetPos;
        float maxDist = direction.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(targetPos, collisionBuffer, direction.normalized, out hit, maxDist, collisionMask))
        {
            // Si choca, acercar la camara
            return Mathf.Max(hit.distance - collisionBuffer, 0.5f);
        }

        return maxDist;
    }

    /// <summary>
    /// Para asignar target desde otro script
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Forward de la camara sin componente Y (para movimiento relativo)
    /// </summary>
    public Vector3 GetFlatForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    /// <summary>
    /// Right de la camara sin componente Y
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
