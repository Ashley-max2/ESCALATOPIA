using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona todas las entradas del jugador (teclado/raton + mando Xbox/PlayStation).
/// Auto-detecta el tipo de mando conectado y usa los ejes correctos.
/// Ignora input cuando el cursor esta desbloqueado (menu ESC).
/// Se ejecuta antes que todos los demas scripts para que el input
/// este listo cuando lo lean.
///
/// TECLADO/RATON (rebindable):
///   WASD            = Mover
///   Raton           = Camara
///   Click Der.      = Gancho
///   Click Izq.      = Soltar gancho
///   Espacio         = Saltar
///   Shift Izq.      = Sprint
///   ESC             = Pausa
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
    // ==================== REBIND SYSTEM ====================

    [System.Serializable]
    public class KeyBinding
    {
        public string actionName;
        [SerializeField] public KeyCode keyCode = KeyCode.None;
        [SerializeField] public KeyCode defaultKeyCode = KeyCode.None;
        [SerializeField] public KeyCode gamepadKeyCode = KeyCode.None;
        [SerializeField] public KeyCode defaultGamepadKeyCode = KeyCode.None;
        [SerializeField] public string gamepadAxis = "";
        [SerializeField] public string defaultGamepadAxis = "";
    }

    [Header("Key Bindings (se auto-rellena con defaults)")]
    [SerializeField] private List<KeyBinding> bindings = new List<KeyBinding>();

    /// <summary>Acceso de lectura a los bindings para la UI de controles</summary>
    public List<KeyBinding> Bindings => bindings;

    // ==================== TIPO DE MANDO ====================

    public enum GamepadType { None, Xbox, PlayStation }

    public enum InputScheme { KeyboardMouse, Gamepad }

    /// <summary>Tipo de mando detectado actualmente</summary>
    public GamepadType DetectedGamepad { get; private set; }

    /// <summary>Ultimo dispositivo usado para actualizar la UI de controles</summary>
    public InputScheme CurrentInputScheme { get; private set; } = InputScheme.KeyboardMouse;

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

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadCameraSensitivity = 3f;
    [SerializeField] private float gamepadDeadzone = 0.25f;
    [SerializeField] private float inputSchemeSwitchDelay = 0.5f;

    [Header("Gamepad Triggers")]
    [SerializeField] private float triggerThreshold = 0.5f;
    [Tooltip("Los triggers de PS van de -1 a 1 (reposo=-1). Los de Xbox van de 0 a 1.")]
    [SerializeField] private float psTriggerThreshold = 0f;

    // ==================== REBINDING ====================

    [HideInInspector] public bool IsRebinding { get; private set; }
    private string currentRebindAction;
    private Dictionary<string, KeyCode> actionToKey = new Dictionary<string, KeyCode>();
    private Dictionary<string, KeyCode> actionToGamepadKey = new Dictionary<string, KeyCode>();
    private Dictionary<string, string> actionToGamepadAxis = new Dictionary<string, string>();

    public UnityEvent OnBindingsChanged = new UnityEvent();

    // ==================== RUNTIME ====================

    // One-frame press tracking
    private bool _jumpPressedThisFrame;
    private bool _hookPressedThisFrame;
    private bool _hookReleasePressedThisFrame;

    // Trigger tracking (para detectar "pressed" como GetKeyDown)
    private float _prevHookFireTrigger;
    private float _prevHookReleaseTrigger;
    private float _prevHookCustomTrigger;
    private float _prevReleaseCustomTrigger;

    // Gamepad detection cache
    private float _nextGamepadCheck;

    // Cambio diferido de esquema de input para UI de controles
    private bool _hasPendingSchemeSwitch;
    private InputScheme _pendingInputScheme;
    private float _pendingSchemeSwitchStart;

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
    private KeyCode _gamepadCancelKey;
    private KeyCode _gamepadMenuKey;

    // ==================== LIFECYCLE ====================

    private void Awake()
    {
        SetupDefaults();
        LoadBindings();
        CacheBindings();
    }

    private void Start()
    {
        DetectGamepad();
    }

    private void Update()
    {
        // Rebinding funciona siempre, incluso con cursor desbloqueado (en menus)
        if (IsRebinding)
        {
            ProcessRebindInput();
            return;
        }

        // Re-detectar mando cada 2 segundos (tambien en menus)
        // para que la UI de controles cambie automaticamente al conectar/desconectar.
        if (Time.unscaledTime > _nextGamepadCheck)
        {
            DetectGamepad();
            _nextGamepadCheck = Time.unscaledTime + 2f;
        }

        UpdateActiveInputScheme();

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

    private void LateUpdate()
    {
        // Reset inputs de un solo frame al final del frame
        _jumpPressedThisFrame = false;
        _hookPressedThisFrame = false;
        _hookReleasePressedThisFrame = false;
        JumpPressed = false;
        HookPressed = false;
        HookReleasePressed = false;
    }

    // ==================== GAMEPAD DETECTION ====================

    /// <summary>
    /// Detecta el tipo de mando conectado por su nombre.
    /// Xbox: contiene "Xbox", "XInput", "xinput"
    /// PlayStation: contiene "Wireless Controller", "DualShock", "DualSense"
    /// </summary>
    private void DetectGamepad()
    {
        GamepadType previousType = DetectedGamepad;
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
                name.Contains("dualsense") || name.Contains("sony") ||
                name.Contains("playstation") || name.Contains("ps4") || name.Contains("ps5"))
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
                _gamepadCancelKey = KeyCode.JoystickButton1;  // B
                _gamepadMenuKey = KeyCode.JoystickButton7;    // Start
                _psUseButtonsForHook = false;
                _hookFireKey = KeyCode.None;
                _hookReleaseKey = KeyCode.None;
                break;

            case GamepadType.PlayStation:
                _cameraXAxis = "PSCameraX";
                _cameraYAxis = "PSCameraY";
                // PS4: L2/R2 son BOTONES (6/7), no solo ejes
                _psUseButtonsForHook = true;
                _hookFireKey = KeyCode.JoystickButton7;    // R2
                _hookReleaseKey = KeyCode.JoystickButton6;  // L2
                _hookFireAxis = "";
                _hookReleaseAxis = "";
                _activeTriggerThreshold = psTriggerThreshold;
                _gamepadJumpKey = KeyCode.JoystickButton1;   // Cross (X)
                _gamepadSprintKey = KeyCode.JoystickButton5;  // R1
                _gamepadCancelKey = KeyCode.JoystickButton2;  // Circulo
                _gamepadMenuKey = KeyCode.JoystickButton9;    // Options
                break;

            default:
                _cameraXAxis = "";
                _cameraYAxis = "";
                _hookFireAxis = "";
                _hookReleaseAxis = "";
                _activeTriggerThreshold = triggerThreshold;
                _gamepadJumpKey = KeyCode.None;
                _gamepadSprintKey = KeyCode.None;
                _gamepadCancelKey = KeyCode.None;
                _gamepadMenuKey = KeyCode.None;
                break;
        }

        bool typeChanged = previousType != DetectedGamepad;
        bool schemeChanged = false;

        if (DetectedGamepad == GamepadType.None)
        {
            ClearPendingSchemeSwitch();
            schemeChanged = SetInputScheme(InputScheme.KeyboardMouse);
        }
        else if (typeChanged && previousType == GamepadType.None)
        {
            // Al conectar mando, prioriza mostrar mando hasta que el jugador use teclado/raton.
            ClearPendingSchemeSwitch();
            schemeChanged = SetInputScheme(InputScheme.Gamepad);
        }

        if (typeChanged && !schemeChanged)
        {
            // Reutilizamos este evento para refrescar filas de controles en UI.
            OnBindingsChanged?.Invoke();
        }
    }

    private void UpdateActiveInputScheme()
    {
        if (DetectedGamepad == GamepadType.None)
        {
            ClearPendingSchemeSwitch();
            SetInputScheme(InputScheme.KeyboardMouse);
            return;
        }

        bool keyboardMouseUsed = HasKeyboardMouseActivity();
        bool gamepadUsed = HasGamepadActivity();

        bool hasDesiredScheme = false;
        InputScheme desiredScheme = CurrentInputScheme;

        // Prioridad a teclado/raton si ambos reportan actividad el mismo frame.
        if (keyboardMouseUsed)
        {
            hasDesiredScheme = true;
            desiredScheme = InputScheme.KeyboardMouse;
        }
        else if (gamepadUsed)
        {
            hasDesiredScheme = true;
            desiredScheme = InputScheme.Gamepad;
        }

        if (hasDesiredScheme)
        {
            if (desiredScheme == CurrentInputScheme)
            {
                ClearPendingSchemeSwitch();
            }
            else
            {
                StartOrRefreshPendingSchemeSwitch(desiredScheme);
            }
        }

        if (_hasPendingSchemeSwitch)
        {
            if (Time.unscaledTime - _pendingSchemeSwitchStart >= inputSchemeSwitchDelay)
            {
                SetInputScheme(_pendingInputScheme);
                ClearPendingSchemeSwitch();
            }
        }
    }

    private void StartOrRefreshPendingSchemeSwitch(InputScheme desiredScheme)
    {
        if (!_hasPendingSchemeSwitch || _pendingInputScheme != desiredScheme)
        {
            _hasPendingSchemeSwitch = true;
            _pendingInputScheme = desiredScheme;
            _pendingSchemeSwitchStart = Time.unscaledTime;
        }
    }

    private void ClearPendingSchemeSwitch()
    {
        _hasPendingSchemeSwitch = false;
    }

    private bool HasKeyboardMouseActivity()
    {
        if (Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.01f) return true;
        if (Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.01f) return true;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            return true;

        // Cualquier tecla de binding o ESC cuenta como actividad de teclado.
        for (int i = 0; i < bindings.Count; i++)
        {
            if (Input.GetKeyDown(bindings[i].keyCode))
                return true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            return true;

        // Si cualquier tecla baja y no es de joystick, tratar como teclado.
        if (Input.anyKeyDown)
            return !AnyJoystickButtonDown();

        return false;
    }

    private bool HasGamepadActivity()
    {
        if (Mathf.Abs(Input.GetAxisRaw("GamepadMoveX")) > gamepadDeadzone) return true;
        if (Mathf.Abs(Input.GetAxisRaw("GamepadMoveY")) > gamepadDeadzone) return true;

        if (!string.IsNullOrEmpty(_cameraXAxis))
        {
            if (Mathf.Abs(Input.GetAxisRaw(_cameraXAxis)) > gamepadDeadzone) return true;
            if (Mathf.Abs(Input.GetAxisRaw(_cameraYAxis)) > gamepadDeadzone) return true;
        }

        if (_psUseButtonsForHook)
        {
            if (Input.GetKeyDown(_hookFireKey) || Input.GetKeyDown(_hookReleaseKey)) return true;
        }
        else if (!string.IsNullOrEmpty(_hookFireAxis))
        {
            if (Input.GetAxisRaw(_hookFireAxis) > _activeTriggerThreshold) return true;
            if (Input.GetAxisRaw(_hookReleaseAxis) > _activeTriggerThreshold) return true;
        }

        if (Input.GetKeyDown(_gamepadJumpKey) || Input.GetKeyDown(_gamepadSprintKey)) return true;

        return AnyJoystickButtonDown();
    }

    private bool AnyJoystickButtonDown()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            string keyName = key.ToString();
            if (keyName.Contains("Joystick") && Input.GetKeyDown(key))
                return true;
        }

        return false;
    }

    private bool SetInputScheme(InputScheme newScheme)
    {
        if (CurrentInputScheme == newScheme)
            return false;

        CurrentInputScheme = newScheme;
        OnBindingsChanged?.Invoke();
        return true;
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
        _prevHookCustomTrigger = 0;
        _prevReleaseCustomTrigger = 0;
    }

    private void ProcessMovementInput()
    {
        // Movimiento teclado rebindable
        float left = Input.GetKey(GetBinding("Izquierda")) ? -1f : 0f;
        float right = Input.GetKey(GetBinding("Derecha")) ? 1f : 0f;
        MoveX = Mathf.Clamp(left + right, -1f, 1f);

        float forward = Input.GetKey(GetBinding("Adelante")) ? 1f : 0f;
        float backward = Input.GetKey(GetBinding("Atras")) ? -1f : 0f;
        MoveZ = Mathf.Clamp(forward + backward, -1f, 1f);

        // Sobrescribe si hay input de joystick/gamepad (stick izquierdo, no rebindable)
        // NOTA: No usamos "Horizontal"/"Vertical" porque incluyen WASD y flechas
        // hardcodeadas en el Input Manager, lo que bypasea el sistema de rebind.
        if (DetectedGamepad != GamepadType.None)
        {
            float axisX = Input.GetAxisRaw("GamepadMoveX");
            float axisZ = Input.GetAxisRaw("GamepadMoveY");
            if (Mathf.Abs(axisX) > gamepadDeadzone) MoveX = axisX;
            if (Mathf.Abs(axisZ) > gamepadDeadzone) MoveZ = axisZ;
        }
    }

    private void ProcessActionInput()
    {
        // === SALTO ===
        // Teclado rebindable
        KeyCode jumpBinding = GetBinding("Saltar");
        if (Input.GetKeyDown(jumpBinding))
            _jumpPressedThisFrame = true;
        JumpHeld = Input.GetKey(jumpBinding);

        // Mando (no rebindable, auto-detectado)
        if (DetectedGamepad != GamepadType.None)
        {
            KeyCode jumpGamepadBinding = GetGamepadBinding("Saltar");
            KeyCode effectiveJumpKey = jumpGamepadBinding != KeyCode.None ? jumpGamepadBinding : _gamepadJumpKey;

            if (effectiveJumpKey != KeyCode.None && Input.GetKeyDown(effectiveJumpKey))
                _jumpPressedThisFrame = true;
            if (effectiveJumpKey != KeyCode.None)
                JumpHeld |= Input.GetKey(effectiveJumpKey);
        }

        JumpPressed = _jumpPressedThisFrame;

        // === SPRINT ===
        // Teclado rebindable
        SprintHeld = Input.GetKey(GetBinding("Correr"));

        // Mando (no rebindable, auto-detectado)
        if (DetectedGamepad != GamepadType.None)
        {
            KeyCode sprintGamepadBinding = GetGamepadBinding("Correr");
            KeyCode effectiveSprintKey = sprintGamepadBinding != KeyCode.None ? sprintGamepadBinding : _gamepadSprintKey;
            if (effectiveSprintKey != KeyCode.None)
                SprintHeld |= Input.GetKey(effectiveSprintKey);
        }

        // === GANCHO (Teclado/Raton rebindable) ===
        KeyCode hookBinding = GetBinding("Gancho");
        KeyCode releaseBinding = GetBinding("LiberarGancho");
        if (Input.GetKeyDown(hookBinding))
            _hookPressedThisFrame = true;
        if (Input.GetKeyDown(releaseBinding))
            _hookReleasePressedThisFrame = true;

        // === GANCHO (Mando, no rebindable) ===
        if (DetectedGamepad != GamepadType.None)
        {
            KeyCode hookGamepadBinding = GetGamepadBinding("Gancho");
            KeyCode releaseGamepadBinding = GetGamepadBinding("LiberarGancho");
            string hookGamepadAxisBinding = GetGamepadAxisBinding("Gancho");
            string releaseGamepadAxisBinding = GetGamepadAxisBinding("LiberarGancho");

            bool usedCustomHook = false;
            bool usedCustomRelease = false;

            if (hookGamepadBinding != KeyCode.None)
            {
                usedCustomHook = true;
                if (Input.GetKeyDown(hookGamepadBinding))
                    _hookPressedThisFrame = true;
            }

            if (releaseGamepadBinding != KeyCode.None)
            {
                usedCustomRelease = true;
                if (Input.GetKeyDown(releaseGamepadBinding))
                    _hookReleasePressedThisFrame = true;
            }

            if (!string.IsNullOrEmpty(hookGamepadAxisBinding))
            {
                usedCustomHook = true;
                if (ReadAxisDown(hookGamepadAxisBinding, ref _prevHookCustomTrigger))
                    _hookPressedThisFrame = true;
            }

            if (!string.IsNullOrEmpty(releaseGamepadAxisBinding))
            {
                usedCustomRelease = true;
                if (ReadAxisDown(releaseGamepadAxisBinding, ref _prevReleaseCustomTrigger))
                    _hookReleasePressedThisFrame = true;
            }

            if (_psUseButtonsForHook)
            {
                // PS4/PS5: L2 y R2 son botones (6 y 7)
                if (!usedCustomHook && Input.GetKeyDown(_hookFireKey))
                    _hookPressedThisFrame = true;
                if (!usedCustomRelease && Input.GetKeyDown(_hookReleaseKey))
                    _hookReleasePressedThisFrame = true;
            }
            else if (!string.IsNullOrEmpty(_hookFireAxis))
            {
                // Xbox: triggers son ejes (0 a 1)
                try
                {
                    float hookFireTrigger = Input.GetAxisRaw(_hookFireAxis);
                    float hookReleaseTrigger = Input.GetAxisRaw(_hookReleaseAxis);

                    if (!usedCustomHook && hookFireTrigger > _activeTriggerThreshold && _prevHookFireTrigger <= _activeTriggerThreshold)
                        _hookPressedThisFrame = true;
                    if (!usedCustomRelease && hookReleaseTrigger > _activeTriggerThreshold && _prevHookReleaseTrigger <= _activeTriggerThreshold)
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

    // ==================== REBINDING ====================

    private int rebindStartFrame;

    private void ProcessRebindInput()
    {
        // Ignorar el frame en que se inicio el rebind
        // para no capturar el Enter/clic que abrio el panel
        if (Time.frameCount <= rebindStartFrame) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndRebind();
            ControlsMenu.Instance?.HideRebindPanel();
            return;
        }

        if (IsGamepadRebindCancelPressed())
        {
            EndRebind();
            ControlsMenu.Instance?.HideRebindPanel();
            return;
        }

        if (CurrentInputScheme == InputScheme.Gamepad && IsHookAction(currentRebindAction))
        {
            if (TryCaptureGamepadTriggerAxisRebind())
                return;
        }

        // Captura explicita de Espacio para evitar conflictos con Submit de UI.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (SetBinding(currentRebindAction, KeyCode.Space))
            {
                EndRebind();
                ControlsMenu.Instance?.HideRebindPanel();
            }
            return;
        }

        if (Input.anyKeyDown)
        {
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                {
                    if (SetBinding(currentRebindAction, k))
                    {
                        EndRebind();
                        ControlsMenu.Instance?.HideRebindPanel();
                    }
                    return;
                }
            }
        }
    }

    public void StartRebind(string actionName)
    {
        if (IsRebinding) return;
        IsRebinding = true;
        currentRebindAction = actionName;
        rebindStartFrame = Time.frameCount; // evitar capturar la tecla que inicio el rebind
    }

    public void EndRebind()
    {
        IsRebinding = false;
        currentRebindAction = null;
    }

    /// <summary>
    /// Asigna una nueva tecla a una accion. Retorna true si OK, false si conflicto.
    /// </summary>
    public bool SetBinding(string action, KeyCode newKey)
    {
        bool gamepadRebind = CurrentInputScheme == InputScheme.Gamepad;

        if (gamepadRebind)
        {
            if (!IsGamepadRebindableAction(action) || !IsJoystickKey(newKey))
                return false;
        }
        else
        {
            if (IsJoystickKey(newKey))
                return false;
        }

        // Busca conflicto
        string conflictingAction = null;
        foreach (var b in bindings)
        {
            if (b.actionName == action) continue;

            if (!gamepadRebind && b.keyCode == newKey)
            {
                conflictingAction = b.actionName;
                break;
            }

            if (gamepadRebind)
            {
                GetEffectiveGamepadBinding(b.actionName, out KeyCode otherKey, out _);
                if (otherKey == newKey && otherKey != KeyCode.None)
                {
                    conflictingAction = b.actionName;
                    break;
                }
            }
        }

        if (conflictingAction != null)
        {
            ControlsMenu.Instance?.ShowConflict(conflictingAction);
            return false;
        }

        // Asigna
        foreach (var b in bindings)
        {
            if (b.actionName == action)
            {
                if (gamepadRebind)
                {
                    b.gamepadKeyCode = newKey;
                    b.gamepadAxis = "";
                }
                else
                    b.keyCode = newKey;
                break;
            }
        }
        CacheBindings();
        SaveBindings();
        return true;
    }

    private bool SetGamepadAxisBinding(string action, string axisName)
    {
        if (!IsGamepadRebindableAction(action) || string.IsNullOrEmpty(axisName))
            return false;

        string conflictingAction = null;
        foreach (var b in bindings)
        {
            if (b.actionName == action) continue;

            GetEffectiveGamepadBinding(b.actionName, out _, out string otherAxis);
            if (otherAxis == axisName)
            {
                conflictingAction = b.actionName;
                break;
            }
        }

        if (conflictingAction != null)
        {
            ControlsMenu.Instance?.ShowConflict(conflictingAction);
            return false;
        }

        foreach (var b in bindings)
        {
            if (b.actionName == action)
            {
                b.gamepadAxis = axisName;
                b.gamepadKeyCode = KeyCode.None;
                break;
            }
        }

        CacheBindings();
        SaveBindings();
        return true;
    }

    // ==================== BINDING HELPERS ====================

    private void SetupDefaults()
    {
        bindings.Clear();
        // Movimiento individual (solo teclado)
        AddBinding("Adelante", KeyCode.W, KeyCode.None);
        AddBinding("Atras", KeyCode.S, KeyCode.None);
        AddBinding("Izquierda", KeyCode.A, KeyCode.None);
        AddBinding("Derecha", KeyCode.D, KeyCode.None);
        // Acciones (solo teclado/raton)
        AddBinding("Saltar", KeyCode.Space, KeyCode.None);
        AddBinding("Correr", KeyCode.LeftShift, KeyCode.None);
        AddBinding("Gancho", KeyCode.Mouse1, KeyCode.None);
        AddBinding("LiberarGancho", KeyCode.Mouse0, KeyCode.None);
    }

    private void AddBinding(string name, KeyCode keyboardDefault, KeyCode gamepadDefault)
    {
        bindings.Add(new KeyBinding
        {
            actionName = name,
            defaultKeyCode = keyboardDefault,
            keyCode = keyboardDefault,
            defaultGamepadKeyCode = gamepadDefault,
            gamepadKeyCode = gamepadDefault,
            defaultGamepadAxis = "",
            gamepadAxis = ""
        });
    }

    private KeyCode GetBinding(string action)
    {
        return actionToKey.TryGetValue(action, out var key) ? key : KeyCode.None;
    }

    private KeyCode GetGamepadBinding(string action)
    {
        return actionToGamepadKey.TryGetValue(action, out var key) ? key : KeyCode.None;
    }

    private string GetGamepadAxisBinding(string action)
    {
        return actionToGamepadAxis.TryGetValue(action, out var axis) ? axis : "";
    }

    private void CacheBindings()
    {
        actionToKey.Clear();
        actionToGamepadKey.Clear();
        actionToGamepadAxis.Clear();
        foreach (var b in bindings)
        {
            actionToKey[b.actionName] = b.keyCode;
            actionToGamepadKey[b.actionName] = b.gamepadKeyCode;
            actionToGamepadAxis[b.actionName] = b.gamepadAxis;
        }
        OnBindingsChanged?.Invoke();
    }

    private void SaveBindings()
    {
        var json = JsonUtility.ToJson(new SerializableBindings { bindings = bindings });
        PlayerPrefs.SetString("PlayerBindings", json);
        PlayerPrefs.Save();
    }

    private void LoadBindings()
    {
        if (PlayerPrefs.HasKey("PlayerBindings"))
        {
            var json = PlayerPrefs.GetString("PlayerBindings");
            var data = JsonUtility.FromJson<SerializableBindings>(json);
            if (data != null && data.bindings != null && data.bindings.Count > 0)
                bindings = data.bindings;
            CacheBindings();
        }
    }

    public void ResetDefaults()
    {
        SetupDefaults();
        CacheBindings();
        SaveBindings();
    }

    public string GetDisplayName(string action)
    {
        bool showGamepadLabels = DetectedGamepad != GamepadType.None && CurrentInputScheme == InputScheme.Gamepad;

        if (showGamepadLabels)
        {
            return GetGamepadDisplayName(action);
        }

        return GetKeyboardDisplayName(action);
    }

    private string GetKeyboardDisplayName(string action)
    {
        var key = GetBinding(action);
        return key switch
        {
            KeyCode.Space => "Espacio",
            KeyCode.LeftShift => "Shift Izq.",
            KeyCode.RightShift => "Shift Der.",
            KeyCode.Mouse0 => "Click Izq.",
            KeyCode.Mouse1 => "Click Der.",
            KeyCode.Mouse2 => "Click Medio",
            KeyCode.LeftControl => "Ctrl Izq.",
            KeyCode.RightControl => "Ctrl Der.",
            KeyCode.LeftAlt => "Alt Izq.",
            KeyCode.RightAlt => "Alt Der.",
            _ => key.ToString().ToUpper()
        };
    }

    private string GetGamepadDisplayName(string action)
    {
        if (action == "Adelante" || action == "Atras" || action == "Izquierda" || action == "Derecha")
            return "Stick Izq.";

        KeyCode custom = GetGamepadBinding(action);
        if (custom != KeyCode.None)
            return ToGamepadButtonLabel(custom);

        string customAxis = GetGamepadAxisBinding(action);
        if (!string.IsNullOrEmpty(customAxis))
            return ToGamepadAxisLabel(customAxis);

        return GetDefaultGamepadLabel(action);
    }

    private string GetDefaultGamepadLabel(string action)
    {
        if (DetectedGamepad == GamepadType.Xbox)
        {
            return action switch
            {
                "Saltar" => "A",
                "Correr" => "RB",
                "Gancho" => "RT",
                "LiberarGancho" => "LT",
                _ => "-"
            };
        }

        if (DetectedGamepad == GamepadType.PlayStation)
        {
            return action switch
            {
                "Saltar" => "X",
                "Correr" => "R1",
                "Gancho" => "R2",
                "LiberarGancho" => "L2",
                _ => "-"
            };
        }

        return "-";
    }

    private string ToGamepadButtonLabel(KeyCode key)
    {
        if (DetectedGamepad == GamepadType.PlayStation)
        {
            return key switch
            {
                KeyCode.JoystickButton0 => "Cuadrado",
                KeyCode.JoystickButton1 => "X",
                KeyCode.JoystickButton2 => "Circulo",
                KeyCode.JoystickButton3 => "Triangulo",
                KeyCode.JoystickButton4 => "L1",
                KeyCode.JoystickButton5 => "R1",
                KeyCode.JoystickButton6 => "L2",
                KeyCode.JoystickButton7 => "R2",
                KeyCode.JoystickButton8 => "Share",
                KeyCode.JoystickButton9 => "Options",
                KeyCode.JoystickButton10 => "L3",
                KeyCode.JoystickButton11 => "R3",
                _ => key.ToString().Replace("JoystickButton", "BTN ")
            };
        }

        return key switch
        {
            KeyCode.JoystickButton0 => "A",
            KeyCode.JoystickButton1 => "B",
            KeyCode.JoystickButton2 => "X",
            KeyCode.JoystickButton3 => "Y",
            KeyCode.JoystickButton4 => "LB",
            KeyCode.JoystickButton5 => "RB",
            KeyCode.JoystickButton6 => "Back",
            KeyCode.JoystickButton7 => "Start",
            KeyCode.JoystickButton8 => "LS",
            KeyCode.JoystickButton9 => "RS",
            _ => key.ToString().Replace("JoystickButton", "BTN ")
        };
    }

    private string ToGamepadAxisLabel(string axis)
    {
        if (axis == "GamepadHookFire")
            return DetectedGamepad == GamepadType.PlayStation ? "R2" : "RT";
        if (axis == "GamepadHookRelease")
            return DetectedGamepad == GamepadType.PlayStation ? "L2" : "LT";
        return axis;
    }

    private bool TryCaptureGamepadTriggerAxisRebind()
    {
        if (IsAxisPressedNow("GamepadHookFire"))
        {
            if (SetGamepadAxisBinding(currentRebindAction, "GamepadHookFire"))
            {
                EndRebind();
                ControlsMenu.Instance?.HideRebindPanel();
            }
            return true;
        }

        if (IsAxisPressedNow("GamepadHookRelease"))
        {
            if (SetGamepadAxisBinding(currentRebindAction, "GamepadHookRelease"))
            {
                EndRebind();
                ControlsMenu.Instance?.HideRebindPanel();
            }
            return true;
        }

        return false;
    }

    private bool IsAxisPressedNow(string axis)
    {
        try
        {
            return Input.GetAxisRaw(axis) > _activeTriggerThreshold;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private bool ReadAxisDown(string axis, ref float previousValue)
    {
        try
        {
            float value = Input.GetAxisRaw(axis);
            bool down = value > _activeTriggerThreshold && previousValue <= _activeTriggerThreshold;
            previousValue = value;
            return down;
        }
        catch (System.Exception)
        {
            previousValue = 0f;
            return false;
        }
    }

    private bool IsGamepadRebindCancelPressed()
    {
        if (DetectedGamepad == GamepadType.None)
            return false;

        if (_gamepadCancelKey != KeyCode.None && Input.GetKeyDown(_gamepadCancelKey))
            return true;

        if (_gamepadMenuKey != KeyCode.None && Input.GetKeyDown(_gamepadMenuKey))
            return true;

        return false;
    }

    private bool IsHookAction(string action)
    {
        return action == "Gancho" || action == "LiberarGancho";
    }

    private bool IsJoystickKey(KeyCode key)
    {
        return key.ToString().Contains("Joystick");
    }

    private bool IsGamepadRebindableAction(string action)
    {
        // El movimiento del stick izquierdo se mantiene fijo para mando.
        return action == "Saltar" || action == "Correr" || action == "Gancho" || action == "LiberarGancho";
    }

    private void GetEffectiveGamepadBinding(string action, out KeyCode key, out string axis)
    {
        key = GetGamepadBinding(action);
        axis = GetGamepadAxisBinding(action);

        // Si ya hay remapeo custom, respetarlo.
        if (key != KeyCode.None || !string.IsNullOrEmpty(axis))
            return;

        // Si no hay remapeo custom, usar defaults segun plataforma.
        switch (DetectedGamepad)
        {
            case GamepadType.Xbox:
                switch (action)
                {
                    case "Saltar":
                        key = KeyCode.JoystickButton0; // A
                        break;
                    case "Correr":
                        key = KeyCode.JoystickButton5; // RB
                        break;
                    case "Gancho":
                        axis = "GamepadHookFire"; // RT
                        break;
                    case "LiberarGancho":
                        axis = "GamepadHookRelease"; // LT
                        break;
                }
                break;

            case GamepadType.PlayStation:
                switch (action)
                {
                    case "Saltar":
                        key = KeyCode.JoystickButton1; // X
                        break;
                    case "Correr":
                        key = KeyCode.JoystickButton5; // R1
                        break;
                    case "Gancho":
                        key = KeyCode.JoystickButton7; // R2
                        break;
                    case "LiberarGancho":
                        key = KeyCode.JoystickButton6; // L2
                        break;
                }
                break;
        }
    }

    [System.Serializable]
    private class SerializableBindings
    {
        public List<KeyBinding> bindings;
    }
}
