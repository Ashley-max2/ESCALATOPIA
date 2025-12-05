using UnityEngine;
using UnityEngine.InputSystem;

/*/// <summary>
/// Puente entre Input System (gamepad) y los controles existentes (teclado/ratón).
/// NO elimina el sistema actual, solo AÑADE soporte para gamepad.
/// NOTA: Este script se activará cuando instales el Input System Package.
/// Por ahora, compila sin errores y estará listo para usar.
/// </summary>
public class GamepadInputBridge : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FirstPersonCameraController fpCamera;
    [SerializeField] private ThirdPersonCameraController tpCamera;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private HookSystem hookSystem;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Configuración Gamepad")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private bool gamepadEnabled = true;

    // Variables de input del gamepad
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool runPressed;
    private bool climbPressed;
    private bool aimPressed;

    private void Start()
    {
        // Buscar referencias automáticamente si no están asignadas
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        if (fpCamera == null)
            fpCamera = FindObjectOfType<FirstPersonCameraController>();
        
        if (tpCamera == null)
            tpCamera = FindObjectOfType<ThirdPersonCameraController>();
        
        if (cameraManager == null)
            cameraManager = FindObjectOfType<CameraManager>();
        
        if (hookSystem == null)
            hookSystem = GetComponentInChildren<HookSystem>();
        
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        if (!gamepadEnabled) return;

        // SOLO aplicar input del gamepad si el PlayerController existe
        if (playerController != null)
        {
            // Movimiento: SUMA el input del gamepad al input existente de teclado
            // El PlayerController ya lee teclado, esto solo añade el gamepad
            playerController.inputH += moveInput.x;
            playerController.inputV += moveInput.y;

            // Botones: OR lógico (si presionas teclado O gamepad, funciona)
            playerController.inputCorrer = playerController.inputCorrer || runPressed;
            playerController.inputEscalar = playerController.inputEscalar || climbPressed;
        }

        // Rotación de cámara con Right Stick
        if (lookInput.sqrMagnitude > 0.01f)
        {
            ApplyLookInput(lookInput * lookSensitivity);
        }
    }

    private void ApplyLookInput(Vector2 lookDelta)
    {
        // Aplicar rotación a la cámara activa (usar ternario en lugar de método)
        if (fpCamera != null && fpCamera.isActiveAndEnabled)
        {
            fpCamera.RotateByInput(lookDelta);
        }
        else if (tpCamera != null && tpCamera.isActiveAndEnabled)
        {
            tpCamera.RotateByInput(lookDelta);
        }
    }

    #region Input System Callbacks

    // Move - Left Stick
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Look - Right Stick
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Jump - Botón A
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && playerController != null)
        {
            playerController.inputSalto = true;
        }
    }

    // Run - LB (Left Shoulder)
    public void OnRun(InputAction.CallbackContext context)
    {
        runPressed = context.ReadValue<float>() > 0.5f;
    }

    // Climb - LT (Left Trigger)
    public void OnClimb(InputAction.CallbackContext context)
    {
        climbPressed = context.ReadValue<float>() > 0.5f;
    }

    // Grapple - RB (Right Shoulder)
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started && hookSystem != null)
        {
            if (Input.GetMouseButton(0))
            {
                hookSystem.LaunchHook();
            }
        }
    }

    // Aim - Right Stick Press
    public void OnAimToggle(InputAction.CallbackContext context)
    {
        if (context.started && hookSystem != null)
        {
            hookSystem.ChangeState(new HookAimingState(hookSystem));
        }
        else if (context.canceled)
        {
            if (hookSystem != null)
            {
                hookSystem.ChangeState(new HookIdleState(hookSystem));
            }
        }
    }

    // Toggle Camera - Y Button
    public void OnToggleCamera(InputAction.CallbackContext context)
    {
        if (context.started && cameraManager != null)
        {
            cameraManager.AlternarCamara();
        }
    }

    // Pause - Start Button
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started && pauseMenu != null)
        {
            pauseMenu.TogglePauseMenu();
        }
    }

    #endregion

    // Método público para habilitar/deshabilitar gamepad
    public void SetGamepadEnabled(bool enabled)
    {
        gamepadEnabled = enabled;
    }
}
*/

/// <summary>
/// VERSIÓN COMPLETA DEL GAMEPAD INPUT BRIDGE
/// Usa este archivo DESPUÉS de instalar el Input System Package.
/// 
/// INSTRUCCIONES:
/// 1. Instala el Input System Package desde Package Manager
/// 2. Borra o renombra GamePadInputBridge.cs
/// 3. Renombra este archivo a GamePadInputBridge.cs
/// </summary>
public class GamepadInputBridge : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FirstPersonCameraController fpCamera;
    [SerializeField] private ThirdPersonCameraController tpCamera;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private HookSystem hookSystem;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Configuración Gamepad")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private bool gamepadEnabled = true;

    // Variables de input del gamepad
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool runPressed;
    private bool climbPressed;
    private bool aimPressed;

    private void Start()
    {
        // Buscar referencias automáticamente si no están asignadas
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        if (fpCamera == null)
            fpCamera = FindObjectOfType<FirstPersonCameraController>();
        
        if (tpCamera == null)
            tpCamera = FindObjectOfType<ThirdPersonCameraController>();
        
        if (cameraManager == null)
            cameraManager = FindObjectOfType<CameraManager>();
        
        if (hookSystem == null)
            hookSystem = GetComponentInChildren<HookSystem>();
        
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        if (!gamepadEnabled) return;

        // SOLO aplicar input del gamepad si el PlayerController existe
        if (playerController != null)
        {
            // Movimiento: SUMA el input del gamepad al input existente de teclado
            // El PlayerController ya lee teclado, esto solo añade el gamepad
            playerController.inputH += moveInput.x;
            playerController.inputV += moveInput.y;

            // Botones: OR lógico (si presionas teclado O gamepad, funciona)
            playerController.inputCorrer = playerController.inputCorrer || runPressed;
            playerController.inputEscalar = playerController.inputEscalar || climbPressed;
        }

        // Rotación de cámara con Right Stick
        if (lookInput.sqrMagnitude > 0.01f)
        {
            ApplyLookInput(lookInput * lookSensitivity);
        }
    }

    private void ApplyLookInput(Vector2 lookDelta)
    {
        // Aplicar rotación a la cámara activa
        if (cameraManager != null && cameraManager.EstaEnPrimeraPersona())
        {
            if (fpCamera != null)
            {
                fpCamera.RotateByInput(lookDelta);
            }
        }
        else
        {
            if (tpCamera != null)
            {
                tpCamera.RotateByInput(lookDelta);
            }
        }
    }

    #region Input System Callbacks (llamados por PlayerInput)

    // Move - Left Stick
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Look - Right Stick
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Jump - Botón A
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && playerController != null)
        {
            playerController.inputSalto = true;
        }
    }

    // Run - LB (Left Shoulder)
    public void OnRun(InputAction.CallbackContext context)
    {
        runPressed = context.performed || context.started;
    }

    // Climb - RB (Right Shoulder)
    public void OnClimb(InputAction.CallbackContext context)
    {
        climbPressed = context.performed || context.started;
    }

    // Grapple - RT (Right Trigger)
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.performed && hookSystem != null)
        {
            // Si tiene target válido, lanzar gancho
            if (hookSystem.TargetFinder != null && hookSystem.TargetFinder.HasValidTarget)
            {
                hookSystem.LaunchHook();
            }
        }
        else if (context.canceled && hookSystem != null)
        {
            // Al soltar, cancelar gancho
            hookSystem.CancelHook();
            if (hookSystem.HookMovement != null)
            {
                hookSystem.HookMovement.CancelHook();
            }
        }
    }

    // AimToggle - LT (Left Trigger) - Mantener para primera persona
    public void OnAimToggle(InputAction.CallbackContext context)
    {
        if (cameraManager == null) return;

        if (context.performed || context.started)
        {
            // Cambiar a primera persona al presionar
            cameraManager.CambiarAPrimeraPersona();
            aimPressed = true;

            // También activar modo aiming del gancho
            if (hookSystem != null && !hookSystem.IsHooking)
            {
                hookSystem.ChangeState(new HookAimingState(hookSystem));
            }
        }
        else if (context.canceled)
        {
            // Volver a tercera persona al soltar
            cameraManager.CambiarATerceraPersona();
            aimPressed = false;

            // Cancelar aiming del gancho
            if (hookSystem != null)
            {
                hookSystem.ChangeState(new HookIdleState(hookSystem));
            }
        }
    }

    // ToggleCamera - Botón Y - Alternar cámara
    public void OnToggleCamera(InputAction.CallbackContext context)
    {
        if (context.performed && cameraManager != null)
        {
            cameraManager.AlternarCamara();
        }
    }

    // Pause - Start
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed && pauseMenu != null)
        {
            pauseMenu.TogglePauseMenu();
        }
    }

    #endregion

    // Método público para habilitar/deshabilitar gamepad
    public void SetGamepadEnabled(bool enabled)
    {
        gamepadEnabled = enabled;
    }
}
