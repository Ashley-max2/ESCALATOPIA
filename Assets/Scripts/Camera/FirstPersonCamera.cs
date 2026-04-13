using UnityEngine;

/// <summary>
/// Camara en primera persona siguiendo el concepto de OrbitCamera.
/// La direccion a la que mira el jugador depende de la direccion a la que mire la camara.
/// Rota con raton + stick derecho del mando (Xbox o PlayStation).
/// Auto-detecta el tipo de mando via PlayerInputHandler.
/// </summary>
public class FirstPersonCamera : MonoBehaviour
{
    [Header("=== TARGET ===")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 headOffset = new Vector3(0, 0.6f, 0);

    [Header("=== ROTATION ===")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float gamepadSensitivity = 3f;
    [SerializeField] private float gamepadDeadzone = 0.25f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    // Runtime
    private float _horizontalAngle;
    private float _verticalAngle;

    // Referencia al input handler para saber tipo de mando
    private PlayerInputHandler _inputHandler;
    private CharacterController _characterController;

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

        // Buscar CharacterController
        if (target != null)
        {
            _characterController = target.GetComponent<CharacterController>();
        }

        // Buscar input handler
        _inputHandler = FindObjectOfType<PlayerInputHandler>();

        // Inicializar angulos basandose en la rotacion actual del target
        if (target != null)
        {
            _horizontalAngle = target.eulerAngles.y;
            _verticalAngle = 0f;
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

        // Reintentar buscar input handler si no se encontro antes
        if (_inputHandler == null)
            _inputHandler = FindObjectOfType<PlayerInputHandler>();

        // Rotar con el raton
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotar con el stick derecho del mando
        float padX = 0f;
        float padY = 0f;

        // Determinar ejes segun tipo de mando detectado
        string axisX = "GamepadCameraX";   // Default Xbox
        string axisY = "GamepadCameraY";

        if (_inputHandler != null && _inputHandler.DetectedGamepad == PlayerInputHandler.GamepadType.PlayStation)
        {
            axisX = "PSCameraX";
            axisY = "PSCameraY";
        }

        try
        {
            float rawX = Input.GetAxisRaw(axisX);
            float rawY = Input.GetAxisRaw(axisY);

            // Deadzone manual para evitar drift del stick
            if (Mathf.Abs(rawX) > gamepadDeadzone) padX = rawX * gamepadSensitivity;
            if (Mathf.Abs(rawY) > gamepadDeadzone) padY = rawY * gamepadSensitivity;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FirstPersonCamera] Error leyendo eje: {e.Message}");
        }

        // Aplicar rotaciones
        _horizontalAngle += mouseX + padX;
        _verticalAngle -= (mouseY + padY);
        _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);

        // Actualizar rotacion del target (jugador mira en la direccion horizontal de la camara)
        target.eulerAngles = new Vector3(0, _horizontalAngle, 0);
    }

    private void UpdateCameraPosition()
    {
        // Calcular rotacion de la camara (horizontal + vertical)
        Quaternion cameraRotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0);

        // Posicion final de la camara (en la cabeza del jugador) - sin suavizado para juego rapido
        Vector3 headPos = GetHeadPosition();
        transform.position = headPos;

        // Rotacion de la camara
        transform.rotation = cameraRotation;
    }

    private Vector3 GetHeadPosition()
    {
        return target.position + Vector3.Scale(target.lossyScale, headOffset);
    }

    /// <summary>
    /// Para asignar target desde otro script
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            _characterController = target.GetComponent<CharacterController>();
        }
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

    /// <summary>
    /// Retorna el angulo horizontal de la camara (Y rotation)
    /// </summary>
    public float GetHorizontalAngle()
    {
        return _horizontalAngle;
    }

    /// <summary>
    /// Retorna el angulo vertical de la camara (X rotation)
    /// </summary>
    public float GetVerticalAngle()
    {
        return _verticalAngle;
    }

    /// <summary>
    /// Forward completo de la camara (incluida vertical)
    /// </summary>
    public Vector3 GetForward()
    {
        return transform.forward;
    }

    /// <summary>
    /// Right de la camara
    /// </summary>
    public Vector3 GetRight()
    {
        return transform.right;
    }
}
