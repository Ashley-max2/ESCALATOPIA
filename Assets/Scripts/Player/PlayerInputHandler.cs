using UnityEngine;

/// <summary>
/// Gestiona todas las entradas del jugador de forma centralizada.
/// Separa la lógica de entrada del resto del sistema (Single Responsibility Principle).
/// </summary>
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
    
    // Climbing
    public bool ClimbHeld { get; private set; }
    public bool ClimbJumpPressed { get; private set; }
    
    // Hook
    public bool HookPressed { get; private set; }
    public bool HookReleasePressed { get; private set; }
    
    // Camera
    public float CameraX { get; private set; }
    public float CameraY { get; private set; }
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode climbKey = KeyCode.E;
    [SerializeField] private KeyCode hookKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode hookReleaseKey = KeyCode.Mouse0;
    
    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    
    private void Update()
    {
        ProcessMovementInput();
        ProcessActionInput();
        ProcessCameraInput();
    }
    
    private void ProcessMovementInput()
    {
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
    }
    
    private void ProcessActionInput()
    {
        JumpPressed = Input.GetKeyDown(jumpKey);
        JumpHeld = Input.GetKey(jumpKey);
        SprintHeld = Input.GetKey(sprintKey);
        
        ClimbHeld = Input.GetKey(climbKey);
        ClimbJumpPressed = ClimbHeld && JumpPressed;
        
        HookPressed = Input.GetKeyDown(hookKey);
        HookReleasePressed = Input.GetKeyDown(hookReleaseKey);
    }
    
    private void ProcessCameraInput()
    {
        CameraX = Input.GetAxis("Mouse X") * mouseSensitivity;
        CameraY = Input.GetAxis("Mouse Y") * mouseSensitivity;
    }
    
    private void LateUpdate()
    {
        // Reset per-frame inputs
        JumpPressed = false;
        HookPressed = false;
        HookReleasePressed = false;
        ClimbJumpPressed = false;
    }
}
