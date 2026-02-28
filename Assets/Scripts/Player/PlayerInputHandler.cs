using UnityEngine;

/// <summary>
/// Gestiona todas las entradas del jugador (teclado/raton + mando Xbox).
/// Ignora input cuando el cursor esta desbloqueado (menu ESC).
/// Se ejecuta antes que todos los demas scripts para que el input
/// este listo cuando lo lean.
///
/// MANDO XBOX:
///   Stick Izquierdo  = Mover personaje
///   Stick Derecho    = Mover camara (via InputManager "Mouse X"/"Mouse Y")
///   Trigger Derecho  = Disparar gancho
///   Trigger Izquierdo = Soltar gancho
///   Boton A          = Saltar
///   Boton RB         = Correr (sprint)
/// </summary>
[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    // Movement Input
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
    public Vector2 MoveInput => new Vector2(MoveX, MoveZ);
    public bool HasMovementInput => MoveInput.magnitude > 0.1f;

    // Actions
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool SprintHeld { get; private set; }

    // Hook
    public bool HookPressed { get; private set; }
    public bool HookReleasePressed { get; private set; }

    // Camera
    public float CameraX { get; private set; }
    public float CameraY { get; private set; }

    [Header("Input Settings - Teclado/Raton")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode hookKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode hookReleaseKey = KeyCode.Mouse0;

    [Header("Input Settings - Mando Xbox")]
    [SerializeField] private KeyCode gamepadJumpKey = KeyCode.JoystickButton0;   // Boton A
    [SerializeField] private KeyCode gamepadSprintKey = KeyCode.JoystickButton5; // Boton RB
    [SerializeField] private float triggerThreshold = 0.5f;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadCameraSensitivity = 3f;

    // Tracking para que JumpPressed no se pierda entre frames
    private bool _jumpPressedThisFrame;
    private bool _hookPressedThisFrame;
    private bool _hookReleasePressedThisFrame;

    // Tracking de triggers del mando (para detectar "pressed" como GetKeyDown)
    private float _prevHookFireTrigger;
    private float _prevHookReleaseTrigger;

    private void Update()
    {
        // Si el cursor esta desbloqueado no procesamos input de juego
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            ClearAllInput();
            return;
        }

        ProcessMovementInput();
        ProcessActionInput();
        ProcessCameraInput();
    }

    private void ClearAllInput()
    {
        MoveX = 0;
        MoveZ = 0;
        JumpPressed = false;
        JumpHeld = false;
        SprintHeld = false;
        HookPressed = false;
        HookReleasePressed = false;
        CameraX = 0;
        CameraY = 0;
        _jumpPressedThisFrame = false;
        _hookPressedThisFrame = false;
        _hookReleasePressedThisFrame = false;
        _prevHookFireTrigger = 0;
        _prevHookReleaseTrigger = 0;
    }

    private void ProcessMovementInput()
    {
        // Horizontal/Vertical ya tienen entradas de joystick en InputManager
        // Stick izquierdo funciona automaticamente
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
    }

    private void ProcessActionInput()
    {
        // === SALTO ===
        // Teclado: Space | Mando: Boton A (JoystickButton0)
        if (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(gamepadJumpKey))
            _jumpPressedThisFrame = true;

        JumpPressed = _jumpPressedThisFrame;
        JumpHeld = Input.GetKey(jumpKey) || Input.GetKey(gamepadJumpKey);

        // === SPRINT ===
        // Teclado: Shift | Mando: RB (JoystickButton5)
        SprintHeld = Input.GetKey(sprintKey) || Input.GetKey(gamepadSprintKey);

        // === GANCHO (Teclado/Raton) ===
        if (Input.GetKeyDown(hookKey))
            _hookPressedThisFrame = true;
        if (Input.GetKeyDown(hookReleaseKey))
            _hookReleasePressedThisFrame = true;

        // === GANCHO (Mando - Triggers) ===
        // Los triggers son ejes (0 a 1), no botones.
        // Detectamos "pressed" cuando cruza el threshold este frame pero no el anterior.
        float hookFireTrigger = Input.GetAxis("GamepadHookFire");
        float hookReleaseTrigger = Input.GetAxis("GamepadHookRelease");

        if (hookFireTrigger > triggerThreshold && _prevHookFireTrigger <= triggerThreshold)
            _hookPressedThisFrame = true;
        if (hookReleaseTrigger > triggerThreshold && _prevHookReleaseTrigger <= triggerThreshold)
            _hookReleasePressedThisFrame = true;

        _prevHookFireTrigger = hookFireTrigger;
        _prevHookReleaseTrigger = hookReleaseTrigger;

        HookPressed = _hookPressedThisFrame;
        HookReleasePressed = _hookReleasePressedThisFrame;
    }

    private void ProcessCameraInput()
    {
        // Raton + Stick derecho del mando
        // El stick derecho ya se suma via InputManager (entradas "Mouse X"/"Mouse Y" tipo joystick)
        // pero tambien leemos "GamepadCameraX"/"GamepadCameraY" para aplicar sensibilidad propia
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        float padX = Input.GetAxis("GamepadCameraX") * gamepadCameraSensitivity;
        float padY = Input.GetAxis("GamepadCameraY") * gamepadCameraSensitivity;

        CameraX = mouseX + padX;
        CameraY = mouseY + padY;
    }

    private void LateUpdate()
    {
        // Reset inputs de un solo frame al final del frame
        // Asi todos los scripts que lean en Update ya los habran visto
        _jumpPressedThisFrame = false;
        _hookPressedThisFrame = false;
        _hookReleasePressedThisFrame = false;
        JumpPressed = false;
        HookPressed = false;
        HookReleasePressed = false;
    }
}
