using UnityEngine;

/// <summary>
/// Gestiona todas las entradas del jugador.
/// Ignora input cuando el cursor esta desbloqueado (menu ESC).
/// Se ejecuta antes que todos los demas scripts para que el input
/// este listo cuando lo lean.
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
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode hookKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode hookReleaseKey = KeyCode.Mouse0;
    
    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    
    // Tracking para que JumpPressed no se pierda entre frames
    private bool _jumpPressedThisFrame;
    private bool _hookPressedThisFrame;
    private bool _hookReleasePressedThisFrame;
    
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
    }
    
    private void ProcessMovementInput()
    {
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
    }
    
    private void ProcessActionInput()
    {
        // GetKeyDown solo es true un frame, lo guardamos para que no se pierda
        if (Input.GetKeyDown(jumpKey))
            _jumpPressedThisFrame = true;
        
        JumpPressed = _jumpPressedThisFrame;
        JumpHeld = Input.GetKey(jumpKey);
        SprintHeld = Input.GetKey(sprintKey);
        
        if (Input.GetKeyDown(hookKey))
            _hookPressedThisFrame = true;
        if (Input.GetKeyDown(hookReleaseKey))
            _hookReleasePressedThisFrame = true;
        
        HookPressed = _hookPressedThisFrame;
        HookReleasePressed = _hookReleasePressedThisFrame;
    }
    
    private void ProcessCameraInput()
    {
        CameraX = Input.GetAxis("Mouse X") * mouseSensitivity;
        CameraY = Input.GetAxis("Mouse Y") * mouseSensitivity;
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
