using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [System.Serializable]
    public class KeyBinding
    {
        public string actionName;
        [SerializeField] public KeyCode keyCode = KeyCode.None;
        [SerializeField] public KeyCode defaultKeyCode = KeyCode.None;
    }

    [Header("Key Bindings (se auto-rellena con defaults)")]
    [SerializeField] private List<KeyBinding> bindings = new List<KeyBinding>();

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

    [Header("Input Settings - Mando Xbox (NO REBIND)")]
    [SerializeField] private KeyCode gamepadJumpKey = KeyCode.JoystickButton0;   // Boton A
    [SerializeField] private KeyCode gamepadSprintKey = KeyCode.JoystickButton5; // Boton RB
    [SerializeField] private float triggerThreshold = 0.5f;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadCameraSensitivity = 3f;

    // Rebinding
    [HideInInspector] public bool IsRebinding { get; private set; }
    private string currentRebindAction;
    private Dictionary<string, KeyCode> actionToKey = new Dictionary<string, KeyCode>();

    public UnityEvent OnBindingsChanged = new UnityEvent();

    // Tracking
    private bool _jumpPressedThisFrame;
    private bool _hookPressedThisFrame;
    private bool _hookReleasePressedThisFrame;

    // Tracking de triggers del mando (para detectar "pressed" como GetKeyDown)
    private float _prevHookFireTrigger;
    private float _prevHookReleaseTrigger;

    void Awake()
    {
        SetupDefaults();
        LoadBindings();
        CacheBindings();
    }

    void SetupDefaults()
    {
        bindings.Clear();
        // Movimiento individual (solo teclado)
        AddBinding("Adelante", KeyCode.W);
        AddBinding("Atras", KeyCode.S);
        AddBinding("Izquierda", KeyCode.A);
        AddBinding("Derecha", KeyCode.D);
        // Acciones (solo teclado/raton)
        AddBinding("Saltar", KeyCode.Space);
        AddBinding("Correr", KeyCode.LeftShift);
        AddBinding("Gancho", KeyCode.Mouse1);
        AddBinding("LiberarGancho", KeyCode.Mouse0);
        // Añade más si necesitas
    }

    void AddBinding(string name, KeyCode def)
    {
        bindings.Add(new KeyBinding { actionName = name, defaultKeyCode = def, keyCode = def });
    }

    KeyCode GetBinding(string action) => actionToKey.TryGetValue(action, out var key) ? key : KeyCode.None;

    void CacheBindings()
    {
        actionToKey.Clear();
        foreach (var b in bindings)
            actionToKey[b.actionName] = b.keyCode;
        OnBindingsChanged?.Invoke();
    }

    public void StartRebind(string actionName)
    {
        if (IsRebinding) return;
        IsRebinding = true;
        currentRebindAction = actionName;
    }

    public void EndRebind()
    {
        IsRebinding = false;
        currentRebindAction = null;
    }

    // Retorna true si asignado OK, false si conflicto
    public bool SetBinding(string action, KeyCode newKey)
    {
        // Busca conflicto
        string conflictingAction = null;
        foreach (var b in bindings)
        {
            if (b.actionName != action && b.keyCode == newKey)
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

        // Asigna
        foreach (var b in bindings)
        {
            if (b.actionName == action)
            {
                b.keyCode = newKey;
                break;
            }
        }
        CacheBindings();
        SaveBindings();
        return true;
    }

    void Update()
    {
        // Si el cursor esta desbloqueado no procesamos input de juego
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            ClearAllInput();
            return;
        }

        if (!IsRebinding)
        {
            ProcessMovementInput();
            ProcessActionInput();
            ProcessCameraInput();
        }
        else
        {
            ProcessRebindInput();
        }
    }

    private void ProcessRebindInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndRebind();
            ControlsMenu.Instance?.HideRebindPanel();
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

    private void ProcessMovementInput()
    {
        // Movimiento teclado rebindable
        float left = Input.GetKey(GetBinding("Izquierda")) ? -1f : 0f;
        float right = Input.GetKey(GetBinding("Derecha")) ? 1f : 0f;
        MoveX = Mathf.Clamp(left + right, -1f, 1f);

        float forward = Input.GetKey(GetBinding("Adelante")) ? 1f : 0f;
        float backward = Input.GetKey(GetBinding("Atras")) ? -1f : 0f;
        MoveZ = Mathf.Clamp(forward + backward, -1f, 1f);

        // Sobrescribe si hay input de joystick (mando no rebindable)
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisZ = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(axisX) > 0.1f) MoveX = axisX;
        if (Mathf.Abs(axisZ) > 0.1f) MoveZ = axisZ;
    }

    private void ProcessActionInput()
    {
        // === SALTO ===
        // Teclado rebindable
        var jumpKey = GetBinding("Saltar");
        if (Input.GetKeyDown(jumpKey)) _jumpPressedThisFrame = true;
        JumpHeld = Input.GetKey(jumpKey);

        // Mando no rebindable
        if (Input.GetKeyDown(gamepadJumpKey)) _jumpPressedThisFrame = true;
        JumpHeld |= Input.GetKey(gamepadJumpKey);

        JumpPressed = _jumpPressedThisFrame;

        // === SPRINT ===
        // Teclado rebindable
        SprintHeld = Input.GetKey(GetBinding("Correr"));

        // Mando no rebindable
        SprintHeld |= Input.GetKey(gamepadSprintKey);

        // === GANCHO ===
        // Teclado/raton rebindable
        var hookKey = GetBinding("Gancho");
        var releaseKey = GetBinding("LiberarGancho");
        if (Input.GetKeyDown(hookKey)) _hookPressedThisFrame = true;
        if (Input.GetKeyDown(releaseKey)) _hookReleasePressedThisFrame = true;

        // Mando triggers no rebindable
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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        float padX = Input.GetAxis("GamepadCameraX") * gamepadCameraSensitivity;
        float padY = Input.GetAxis("GamepadCameraY") * gamepadCameraSensitivity;

        CameraX = mouseX + padX;
        CameraY = mouseY + padY;
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

    void SaveBindings()
    {
        var json = JsonUtility.ToJson(new SerializableBindings { bindings = bindings });
        PlayerPrefs.SetString("PlayerBindings", json);
        PlayerPrefs.Save();
    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey("PlayerBindings"))
        {
            var json = PlayerPrefs.GetString("PlayerBindings");
            var data = JsonUtility.FromJson<SerializableBindings>(json);
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

    [System.Serializable]
    private class SerializableBindings
    {
        public List<KeyBinding> bindings;
    }
}
