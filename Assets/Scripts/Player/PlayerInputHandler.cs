using UnityEngine;

/// <summary>
/// Gestiona todas las entradas del jugador (teclado/raton + mando Xbox/PlayStation).
/// Auto-detecta el tipo de mando conectado y usa los ejes correctos.
/// Ignora input cuando el cursor esta desbloqueado (menu ESC).
/// Se ejecuta antes que todos los demas scripts para que el input
/// este listo cuando lo lean.
///
/// MANDO XBOX:                       MANDO PLAYSTATION:
///   Stick Izquierdo = Mover           Stick Izquierdo = Mover
///   Stick Derecho   = Camara          Stick Derecho   = Camara
///   RT (Trigger R)  = Gancho          R2              = Gancho
///   LT (Trigger L)  = Soltar gancho   L2              = Soltar gancho
///   Boton A         = Saltar          Boton X (Cross) = Saltar
///   Boton RB        = Sprint          Boton R1        = Sprint
///   Start           = Pausa           Options         = Pausa
/// </summary>
[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    // ==================== TIPO DE MANDO ====================
    public enum GamepadType { None, Xbox, PlayStation }

    /// <summary>Tipo de mando detectado actualmente</summary>
    public GamepadType DetectedGamepad { get; private set; }

    // ==================== OUTPUT PROPERTIES ====================

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

    // ==================== SETTINGS ====================

    [Header("Input Settings - Teclado/Raton")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode hookKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode hookReleaseKey = KeyCode.Mouse0;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadCameraSensitivity = 3f;
    [SerializeField] private float gamepadDeadzone = 0.25f;

    [Header("Gamepad Triggers")]
    [SerializeField] private float triggerThreshold = 0.5f;
    [Tooltip("Los triggers de PS van de -1 a 1 (reposo=-1). Los de Xbox van de 0 a 1.")]
    [SerializeField] private float psTriggerThreshold = 0f;

    // ==================== RUNTIME ====================

    // One-frame press tracking
    private bool _jumpPressedThisFrame;
    private bool _hookPressedThisFrame;
    private bool _hookReleasePressedThisFrame;

    // Trigger tracking (para detectar "pressed" como GetKeyDown)
    private float _prevHookFireTrigger;
    private float _prevHookReleaseTrigger;

    // Gamepad detection cache
    private float _nextGamepadCheck;

    // Nombres de ejes segun tipo de mando
    private string _cameraXAxis;
    private string _cameraYAxis;
    private string _hookFireAxis;
    private string _hookReleaseAxis;

    // Botones de gancho (PS4 usa botones, Xbox usa ejes)
    private KeyCode _hookFireKey;
    private KeyCode _hookReleaseKey;
    private bool _psUseButtonsForHook;
    private float _activeTriggerThreshold;

    // Botones de mando
    private KeyCode _gamepadJumpKey;
    private KeyCode _gamepadSprintKey;

    private void Start()
    {
        // Detectar mando al iniciar
        DetectGamepad();
    }

    private void Update()
    {
        // Si el cursor esta desbloqueado no procesamos input de juego
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            ClearAllInput();
            return;
        }

        // Re-detectar mando cada 2 segundos
        if (Time.unscaledTime > _nextGamepadCheck)
        {
            DetectGamepad();
            _nextGamepadCheck = Time.unscaledTime + 2f;
        }

        ProcessMovementInput();
        ProcessActionInput();
        ProcessCameraInput();
    }

    // ==================== GAMEPAD DETECTION ====================

    /// <summary>
    /// Detecta el tipo de mando conectado por su nombre.
    /// Xbox: contiene "Xbox", "XInput", "xinput"
    /// PlayStation: contiene "Wireless Controller", "DualShock", "DualSense"
    /// </summary>
    private void DetectGamepad()
    {
        DetectedGamepad = GamepadType.None;

        string[] joysticks = Input.GetJoystickNames();
        for (int i = 0; i < joysticks.Length; i++)
        {
            if (string.IsNullOrEmpty(joysticks[i])) continue;

            string name = joysticks[i].ToLower();

            if (name.Contains("xbox") || name.Contains("xinput"))
            {
                DetectedGamepad = GamepadType.Xbox;
                break;
            }
            if (name.Contains("wireless controller") || name.Contains("dualshock") ||
                name.Contains("dualsense") || name.Contains("sony"))
            {
                DetectedGamepad = GamepadType.PlayStation;
                break;
            }

            // Si hay un mando conectado pero no reconocemos el nombre, asumir Xbox (XInput es default en Windows)
            DetectedGamepad = GamepadType.Xbox;
        }

        // Configurar ejes y botones segun tipo
        switch (DetectedGamepad)
        {
            case GamepadType.Xbox:
                _cameraXAxis = "GamepadCameraX";
                _cameraYAxis = "GamepadCameraY";
                _hookFireAxis = "GamepadHookFire";
                _hookReleaseAxis = "GamepadHookRelease";
                _activeTriggerThreshold = triggerThreshold;
                _gamepadJumpKey = KeyCode.JoystickButton0;   // A
                _gamepadSprintKey = KeyCode.JoystickButton5;  // RB
                _psUseButtonsForHook = false;
                _hookFireKey = KeyCode.None;
                _hookReleaseKey = KeyCode.None;
                break;

            case GamepadType.PlayStation:
                _cameraXAxis = "PSCameraX";
                _cameraYAxis = "PSCameraY";
                // PS4: L2/R2 son BOTONES (6/7), no solo ejes!
                _psUseButtonsForHook = true;
                _hookFireKey = KeyCode.JoystickButton7;    // R2
                _hookReleaseKey = KeyCode.JoystickButton6;  // L2
                _hookFireAxis = "";
                _hookReleaseAxis = "";
                _activeTriggerThreshold = psTriggerThreshold;
                _gamepadJumpKey = KeyCode.JoystickButton1;   // Cross (X)
                _gamepadSprintKey = KeyCode.JoystickButton5;  // R1
                break;

            default:
                _cameraXAxis = "";
                _cameraYAxis = "";
                _hookFireAxis = "";
                _hookReleaseAxis = "";
                _activeTriggerThreshold = triggerThreshold;
                _gamepadJumpKey = KeyCode.None;
                _gamepadSprintKey = KeyCode.None;
                break;
        }
    }

    // ==================== INPUT PROCESSING ====================

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
        // Stick izquierdo funciona automaticamente para Xbox y PS
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
    }

    private void ProcessActionInput()
    {
        // === SALTO ===
        // Teclado: Space | Xbox: A (button0) | PS: Cross (button1)
        bool gamepadJump = DetectedGamepad != GamepadType.None && Input.GetKeyDown(_gamepadJumpKey);
        if (Input.GetKeyDown(jumpKey) || gamepadJump)
            _jumpPressedThisFrame = true;

        JumpPressed = _jumpPressedThisFrame;

        bool gamepadJumpHeld = DetectedGamepad != GamepadType.None && Input.GetKey(_gamepadJumpKey);
        JumpHeld = Input.GetKey(jumpKey) || gamepadJumpHeld;

        // === SPRINT ===
        // Teclado: Shift | Xbox: RB (button5) | PS: R1 (button5)
        bool gamepadSprint = DetectedGamepad != GamepadType.None && Input.GetKey(_gamepadSprintKey);
        SprintHeld = Input.GetKey(sprintKey) || gamepadSprint;

        // === GANCHO (Teclado/Raton) ===
        if (Input.GetKeyDown(hookKey))
            _hookPressedThisFrame = true;
        if (Input.GetKeyDown(hookReleaseKey))
            _hookReleasePressedThisFrame = true;

        // === GANCHO (Mando) ===
        if (DetectedGamepad != GamepadType.None)
        {
            if (_psUseButtonsForHook)
            {
                // PS4/PS5: L2 y R2 son botones (6 y 7)
                if (Input.GetKeyDown(_hookFireKey))
                    _hookPressedThisFrame = true;
                if (Input.GetKeyDown(_hookReleaseKey))
                    _hookReleasePressedThisFrame = true;
            }
            else if (!string.IsNullOrEmpty(_hookFireAxis))
            {
                // Xbox: triggers son ejes (0 a 1)
                try
                {
                    float hookFireTrigger = Input.GetAxisRaw(_hookFireAxis);
                    float hookReleaseTrigger = Input.GetAxisRaw(_hookReleaseAxis);

                    if (hookFireTrigger > _activeTriggerThreshold && _prevHookFireTrigger <= _activeTriggerThreshold)
                        _hookPressedThisFrame = true;
                    if (hookReleaseTrigger > _activeTriggerThreshold && _prevHookReleaseTrigger <= _activeTriggerThreshold)
                        _hookReleasePressedThisFrame = true;

                    _prevHookFireTrigger = hookFireTrigger;
                    _prevHookReleaseTrigger = hookReleaseTrigger;
                }
                catch (System.Exception)
                {
                    // Eje no existe todavia, ignorar
                }
            }
        }

        HookPressed = _hookPressedThisFrame;
        HookReleasePressed = _hookReleasePressedThisFrame;
    }

    private void ProcessCameraInput()
    {
        // Raton
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Stick derecho del mando (con deadzone manual anti-drift)
        float padX = 0f;
        float padY = 0f;

        if (DetectedGamepad != GamepadType.None && !string.IsNullOrEmpty(_cameraXAxis))
        {
            try
            {
                float rawX = Input.GetAxisRaw(_cameraXAxis);
                float rawY = Input.GetAxisRaw(_cameraYAxis);
                if (Mathf.Abs(rawX) > gamepadDeadzone) padX = rawX * gamepadCameraSensitivity;
                if (Mathf.Abs(rawY) > gamepadDeadzone) padY = rawY * gamepadCameraSensitivity;
            }
            catch (System.Exception)
            {
                // Eje no existe todavia, ignorar
            }
        }

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
